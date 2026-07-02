# ETG Source Map

Findings from the game source code (Assembly-CSharp.dll) relevant to GungeonNearby.
Investigated via dnSpy. Unity 2017.4.27f1, Mono.

## Files

| File | Coverage |
|:---|:---|
| [VISUALS.md](VISUALS.md) | Sprites, animations, tk2d, outlines |
| [PLAYER.md](PLAYER.md) | PlayerController, character identity, input |
| [ENEMIES.md](ENEMIES.md) | Enemy types, spawn hooks, AI |
| [DUNGEON.md](DUNGEON.md) | Floor/room generation, scene loading |
| [PROJECTILES.md](PROJECTILES.md) | Bullet system, collections |
| [ITEMS.md](ITEMS.md) | Pickup system, ItemDB |
| [AUDIO.md](AUDIO.md) | Sound hooks |

## Notes

- `ETGMod.Assets.Collections` — registry of all loaded `tk2dSpriteCollectionData`, maintained by ModTheGungeonAPI
- `ETGMod.Assets.FindCollectionOfName(string)` — look up a collection by name at runtime
- `BraveResources` — game's asset loading wrapper, not always needed
