# Player

## Key Classes

- `PlayerController` — main player class, extends `BraveBehaviour`
- `Foyer` — lobby/character select scene controller

## Findings

### Character Identity
`PlayerController.characterIdentity` — `PlayableCharacters` enum value identifying which character the player picked.

`PlayableCharacters` enum → sprite collection name mapping (confirmed via `ETGMod.Assets.Collections`):
| Enum value | Collection name |
|:---|:---|
| Soldier | Marine |
| Pilot | SpaceRogue |
| Convict | Convict |
| Robot | Robot |
| Guide | Guide |
| CoopCultist | CoopCultist |
| Gunslinger | Gunslinger_Collection |
| Bullet | Playable_Bullet_Man |

### Spawn Hook
`Foyer.PlayerCharacterChanged(PlayerController newCharacter)` — fires when a character is selected in the Foyer. Parameter name must be `newCharacter` for Harmony postfix injection.

### Input
`PlayerController` reads from Unity `Input` — block input by patching `Input.GetKey`, `Input.GetKeyDown`, `Input.GetKeyUp` (both `KeyCode` and `string` overloads).
