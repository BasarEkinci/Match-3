# Match-3

Sonsuz oynanan, klasik kurallara sadık bir match-3 oyunu. 8x8 tahtada komşu taşlar
yer değiştirilerek en az üçlü eşleşmeler kurulur; kaybetme koşulu, hamle limiti ya da
süre limiti yoktur. Tek amaç en yüksek skoru yapmaktır.

Tasarımın tamamı [Docs/GDD.md](Docs/GDD.md), iş listesi [Docs/IMPLEMENTATION_TASKS.md](Docs/IMPLEMENTATION_TASKS.md).

## Gereksinimler

- Unity **6000.3.17f1**
- Paketler `Packages/manifest.json` üzerinden gelir: VContainer 1.19.0, LitMotion, UniTask,
  Input System, URP, TextMesh Pro, Test Framework.

Projeyi Unity Hub'a ekleyip `Assets/Scenes/Main.unity` sahnesini açmak yeterli.

## Mimari

MVC; katmanlar assembly definition ile ayrılmıştır ve bağımlılık tek yönlüdür.

```
Match3.Model  (saf C#, Unity referansı yok)
   ↑
Match3.Signals ← Match3.Controller (Unity referansı yok)
   ↑
Match3.View (MonoBehaviour, LitMotion, Input System)
```

| Katman | Klasör | Sorumluluk |
|---|---|---|
| Model | `Assets/Match3/Scripts/Model` | Tahta, eşleşme bulma, yerçekimi, özel taş etkileri, skor. Unity'siz, tamamen test edilebilir. |
| Controller | `Assets/Match3/Scripts/Controller` | Hamle döngüsü, cascade, girdi yorumlama, ekran akışı, kayıt. `UnityEngine` kullanmaz. |
| View | `Assets/Match3/Scripts/View` | Taş görselleri, animasyon, HUD, ekranlar, dokunma girdisi. |
| Data | `Assets/Match3/Scripts/Data` | `ScriptableObject` ayarları ve PlayerPrefs kayıt implementasyonu. |
| Signals | `Assets/Match3/Scripts/Signals` | Katmanlar arası mesaj tipleri. |
| Core | `Assets/Match3Core` | Projeden bağımsız altyapı: DI bootstrap, event bus (pipe), `ISignal`. |

### İletişim

Sınıflar birbirine doğrudan referans vermez; haberleşme sinyal üzerinden yapılır.
İki pipe vardır:

- **ProjectPipe** — sahne ömrünü aşan olaylar (ekran akışı, tur başlangıcı/bitişi, kayıt).
- **GamePipe** — bir turun içinde kalan olaylar (takas, eşleşme, cascade, skor).

Sinyal envanteri `Docs/IMPLEMENTATION_TASKS.md` içindeki tabloda.

### Bağımlılık enjeksiyonu

VContainer. Sahnede elle referans atanmaz; her şey installer'lardan çözülür:

- `ProjectInstaller` → `ProjectPipeInstaller`
- `MainSceneInstaller` → `GamePipeInstaller`, `Match3ModelInstaller`
- View tarafı: `BoardViewInstaller`, `ScoreViewInstaller`, `FeedbackViewInstaller`, `ScreenInstaller`

Yeni bir sistem eklemek, ilgili installer'a bir satır eklemek demektir.

## Oynanış

- **Tahta:** 8x8, 6 renk. Başlangıçta hazır eşleşme bulunmaz, en az bir geçerli hamle garantidir.
- **Hamle:** Komşu iki taşın takası; eşleşme oluşmuyorsa taşlar geri döner. Özel taş takasları her zaman geçerlidir.
- **Cascade:** Düşüş sonrası oluşan yeni eşleşmeler zincir sayılır, tahta durana kadar otomatik çözülür.
- **Deadlock:** Geçerli hamle kalmazsa tahta karıştırılır; skor ve çarpan korunur.

### Özel taşlar

| Eşleşme | Taş | Etki |
|---|---|---|
| 4'lü yatay | Yatay Roket | Satırı temizler |
| 4'lü dikey | Dikey Roket | Sütunu temizler |
| L / T (5 taş) | Bomba | 3x3 alanı temizler |
| 5'li düz | Renk Bombası | Bir rengin tüm taşlarını temizler |

İki özel taş takas edildiğinde etkileri birleşir (roket+roket, roket+bomba, bomba+bomba,
renk bombası kombinasyonları). Tablo `SpecialCombinationResolver` içinde çözülür.

### Skor

Patlayan taş başına temel puan; her cascade adımı çarpanı bir kademe yükseltir.
Özel taş oluşturmak ve kombinasyon tetiklemek bonus verir. En yüksek skor PlayerPrefs'te tutulur.

## Ayarlar

Değerler kodda değil `Assets/Match3/Data` altındaki ScriptableObject'lerde:

- `BoardSettings` — genişlik, yükseklik, renk sayısı, minimum eşleşme uzunluğu, hücre boyutu.
- `ScoreSettings` — taş puanı, çarpan adımı ve tavanı, özel taş/kombinasyon bonusları.
- `HintSettings` — ipucu bekleme süresi, tekrar aralığı, vurgu şiddeti ve süresi.

## Testler

EditMode testleri `Assets/Match3/Tests/EditMode` altında; Model ve Controller katmanlarını
Unity çalıştırmadan kapsar (tahta, eşleşme, yerçekimi, özel taşlar, kombinasyonlar, skor,
ekran akışı, kayıt). Unity içinden **Window → General → Test Runner → EditMode**.

Komut satırından:

```
& "C:\Program Files\Unity\Hub\Editor\6000.3.17f1\Editor\Unity.exe" -batchmode -quit -nographics -projectPath "C:\Unity\Match-3" -logFile -
```

## Editör araçları

- **Tools → Match3 → Special Injector** — play mode'da rastgele bir taşa istenen özel taşı yerleştirir.
- **Match3 → Build UI Canvas** — `Main.unity` içindeki UI hiyerarşisini koddan yeniden üretir.

## Kod kuralları

`CLAUDE.md` içinde tam listesi var; özeti:

- Async için UniTask; coroutine veya `async void` yok.
- Sahneden/Inspector'dan referans atama ve buton event bağlama yok.
- Sınıflar arası doğrudan referans yok, iletişim sinyalle.
- Controller içinde `using UnityEngine` yok.
- LINQ, magic number ve hardcoded string yok. Enum'lar `byte` tabanlı, miras verilmeyen sınıflar `sealed`.
