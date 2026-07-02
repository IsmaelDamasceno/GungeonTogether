# GungeonNearby

GungeonNearby is rework from https://github.com/llamerrr/GungeonTogether. Adding multiplayer for Enter the Gungeon that allows online co-op play using Steam P2P networking or direct LAN connection.

> [!WARNING]
> _THIS MOD IS STILL VERY EARLY IN DEVELOPMENT - DO NOT EXPECT A FUNCTIONAL EXPERIENCE RIGHT NOW_

> [!NOTE]
> This is a fork from https://github.com/llamerrr/GungeonTogether. Check them out!

> [!NOTE]
> For steam loobies, all players need to play from a official steam copy of the game, if that's not the case, LAN mode must be used instead.

# Features
## What works so far
| System | Status | Notes |
|:---:|:---:|:---:|
| Steam invites/lobby system | 🟩 Done | Steam lobby creation and joining functional |
| LAN Connection | 🟩 Done | Players can host and join lobbies through LAN connections |
| Steam P2P networking | 🟨 Working | Real connections and Steam invites working |
| Basic UI | 🟨 Working | Modern multiplayer menu (Ctrl+P) available |
| Player Synchronization | 🟨 Working | Basic position implemented, development ongoing |
| Enemy Synchronization | 🟥 Planned |  |
| Dungeon Synchronization | 🟥 Planned |  |

## Planed for 1.0 release
- Online co-op multiplayer for Enter the Gungeon
- LAN networking
- Steam P2P networking (no dedicated servers required), no parsec or streaming required
- Real-time synchronisation of
  - Players
  - Enemies
  - Projectiles
  - Dungeon
- Enemy scaling (set difficulty in game)
- Debug controls for testing
- Spectate when dead
- Teammate DBNO system + revive mechanic
  
## Planned for the future
- EPIC GAMES SUPPORT (EOS P2P)
- Unlimited players
- EVERYTHING synced
- Custom Game Modes
  - Race mode (competing in identical dungeons, first player to finish wins)
  - Mirror mode (identical dungeons, can hurt one another but can't see eachother, shooting is synced across dungeons)
- Teammate revive items
- Team based buffs and items

## Installation
1. Install BepInEx for Enter the Gungeon
2. Copy `GungeonTogether.dll` to `[ETG Install]/BepInEx/plugins/`
3. Launch Enter the Gungeon

## Building
This only applies to people who want to build from source, or if we forget to post builds :)
1. You will need to setup BepInExPack_ETG (either with a mod manager like r2ModMan or manually)
2. Run the build script `build.ps1` in the GungeonTogether folder
3. Pray it works because honestly I have no idea if it does
4. Launch Enter the Gungeon

## Technical Details
Built with BepInEx framework using Steam P2P networking for a seamless multiplayer experience.

# 1.0 release coming one day!!!!!!
