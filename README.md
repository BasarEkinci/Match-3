# Match-3

Sonsuz oynanan 8x8 match-3. Unity **6000.3.17f1**; VContainer, LitMotion, UniTask, Input System, URP, TMP.

## Mimari

MVC; katmanlar assembly definition ile ayrılmıştır, bağımlılık tek yönlüdür.

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
| Data | `Assets/Match3/Scripts/Data` | `ScriptableObject` ayarları (`BoardSettings`, `ScoreSettings`, `HintSettings`) ve PlayerPrefs kaydı. |
| Signals | `Assets/Match3/Scripts/Signals` | Katmanlar arası mesaj tipleri. |
| Core | `Assets/Match3Core` | Projeden bağımsız altyapı: DI bootstrap, event bus (pipe), `ISignal`. |

### İletişim

Sınıflar birbirine doğrudan referans vermez; haberleşme sinyalle yapılır. İki pipe:

- **ProjectPipe** — sahne ömrünü aşan olaylar (ekran akışı, tur başlangıcı/bitişi, kayıt).
- **GamePipe** — bir turun içinde kalan olaylar (takas, eşleşme, cascade, skor).

### Bağımlılık enjeksiyonu

VContainer; sahnede elle referans atanmaz, her şey installer'lardan çözülür:

- `ProjectInstaller` → `ProjectPipeInstaller`
- `MainSceneInstaller` → `GamePipeInstaller`, `Match3ModelInstaller`
- View: `BoardViewInstaller`, `ScoreViewInstaller`, `FeedbackViewInstaller`, `ScreenInstaller`

Yeni bir sistem eklemek, ilgili installer'a bir satır eklemek demektir.
