# Dead Run Collector

A 3D survival-collection game built with Unity 6.

## Game Overview

Dead Run Collector is a time-limited 3D survival-collection game. The player controls a third-person humanoid character in a bounded urban environment and must maximise their score within a strict 180-second session. Collect coins and diamonds while avoiding zombie pursuers that spawn 3 seconds after the game begins.

## How to Play

| Input | Action |
|-------|--------|
| W / A / S / D | Move |
| Shift | Sprint |
| Space | Jump |
| R | Restart (after game over) |

## Game Features

### Game Mechanics
- **Collection System** - Coins (+5 pts) and Diamonds (+10 pts) with weighted spawn probability
- **Buff System** - Thunder (2x speed for 8s) and Poison (0.5x speed for 8s)
- **Fall Detection** - Game over if the player falls off the map
- **180-Second Timer** - Fixed round duration with countdown display

### Game AI
- Zombie enemies use **NavMeshAgent** for pathfinding
- Zombies **match player speed** - walk when the player walks, run when the player sprints
- 3-second delayed spawn to give the player time to orient
- Boundary clamping prevents zombies from leaving the play area
- Distance-based catch detection for reliable collision

### Content Generation
- **Procedural item spawning** with weighted probability (Coin 50%, Diamond 30%, Thunder 10%, Poison 10%)
- Raycast ground validation ensures items spawn on walkable surfaces only
- Separation check prevents item overlap
- Maximum 80 active items on the map

### Animation
- Player: Idle, Walk, Run states driven by movement speed
- Zombie: Idle, Walk, Run states driven by NavMeshAgent velocity
- Smooth transitions using Animator Float parameters

### User Interface
- **Start Screen** - Start Game button with time-freeze
- **In-Game HUD** - Timer, Score, Coin count, Diamond count
- **Game Over Screen** - Final score, loss reason, Restart and Quit buttons

## Project Structure

```
Assets/
  Script/
    PlayerCharacterController.cs  - Player movement, jump, sprint
    PlayerBuff.cs                 - Thunder and Poison buff system
    EnemyController.cs            - Zombie AI, pathfinding, animation
    GameManager.cs                - Round control, timer, scoring, UI
    GameOverUI.cs                 - Restart and Quit button handlers
    TimedItemSpawner_Rect.cs      - Procedural weighted item spawning
    Collectiable.cs               - Item pickup and effect logic
    CameraFollow.cs               - Third-person camera follow
```

## Assets

- **Character Model**: [Toony Tiny People](https://assetstore.unity.com/) - Player and Zombie models with animations
- **Environment**: [Low Poly Atmospheric Locations Pack](https://assetstore.unity.com/) - Urban environment
- **Gems**: [BTM Items Gems](https://assetstore.unity.com/) - Coin and Diamond models

## Technical Details

- **Engine**: Unity 6 (6000.3.6f1)
- **Language**: C#
- **Navigation**: NavMesh Surface (Unity AI Navigation package)
- **UI**: TextMeshPro
- **Input**: Unity Input System

## Build

Download the latest release from the [Releases](https://github.com/QX-Su/Dead-Run-Collector/releases) page. Extract `BallGame.zip` and run the executable.

## Author

Su Qixiang - Student ID: 100512104

Module: CMP-6056B - Game Development
