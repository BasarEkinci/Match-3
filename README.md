# Match-3

A classic endless match-3 game built in Unity 6. An 8x8 board, six tile colors, no move limit and no timer — the only goal is a higher score.

## About

Strict MVC architecture with pure C# game logic, zero Unity coupling below the view layer, and every rule enforced at compile time by assembly definitions.

- **Classic rules** — swap adjacent tiles, three or more in a line clears
- **Special tiles** — rockets, bombs, and color bombs with combination effects
- **Cascades** — chained matches raise the score multiplier
- **Deadlock recovery** — the board reshuffles when no valid move remains, score preserved
- **Hint system** after idle time, and a persistent high score
- **No menus, no ads, no distractions** — just the game

## Architecture

```
Model/      → Pure C# (board, matching, gravity, specials, scoring) — no Unity dependencies
Controller/ → Game logic (move loop, cascade, input, screen flow, save) — no Unity dependencies
View/       → Unity layer (tiles, animation, HUD, screens, touch input)
Signals/    → Message types crossing the layers
```

- **VContainer** for dependency injection — nothing is wired in the scene, every reference resolves from an installer
- **MessagePipe** for pub/sub — classes never reference each other directly
- **Two pipes** — `ProjectPipe` for events outliving the scene (screen flow, run start/end, save), `GamePipe` for events inside a run (swap, match, cascade, score)
- **Assembly definitions** enforce layer isolation at compile time
- **UniTask** everywhere — no coroutines, full cancellation support
- **ScriptableObject settings** — board, score, and hint values live in data, never in code

## Tech Stack

Unity 6 · URP · VContainer · MessagePipe · UniTask · LitMotion · Input System · NUnit / Unity Test Framework

## Testing

118 EditMode tests covering the Model and Controller layers without entering play mode:

- **Model:** board state, generation, match finding, gravity, shuffling, move scanning, special tile creation, effects, combinations, scoring
- **Controller:** move loop and lifetime, cascade chains, input interpretation, screen flow, save/load

## How to Play

- **Swap** two adjacent tiles to line up three or more of a color
- A swap that creates no match **reverts**; swaps involving a special tile always resolve
- Match **four in a row** for a rocket, an **L or T shape** for a bomb, **five in a line** for a color bomb
- **Swap two specials** to combine their effects
- Longer cascade chains raise the **score multiplier**
- Play until you beat your **high score** — there is no way to lose

## Documentation

| Document | Description |
|----------|-------------|
| [GDD.md](Docs/GDD.md) | Game Design Document — mechanics, rules, design decisions |
| [IMPLEMENTATION_TASKS.md](Docs/IMPLEMENTATION_TASKS.md) | Phased task list and signal inventory |
| [ASSETS.md](Docs/ASSETS.md) | Art and audio asset reference |
