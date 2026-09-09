# Nebulous — Unity 2023.2.12f1

This is a self-contained, code-generated Unity implementation of a solo top-down bullet-hell boss rush. It uses no external art or packages: the arena, ship, bosses, projectiles, pickups, HUD, shop, and menu are assembled from scripts at runtime.

## Open and run

1. Open the project with Unity **2023.2.12f1**.
2. If the scenes are missing, use **Nebulous > Build Scenes** once.
3. Open `Assets/Scenes/Menu.unity` and press Play.

The project also includes an editor batch builder that creates and registers `Menu.unity` and `Main.unity`.

## Controls

- WASD / arrow keys: move
- Mouse: aim
- Space or left mouse button: fire
- Shift: shield
- E: Nova Strike

Destroy the boss's block body to expose its core. Collect credits and salvage, then buy one upgrade between encounters. The run contains 31 generated encounters, including Guardians and a final encounter.
