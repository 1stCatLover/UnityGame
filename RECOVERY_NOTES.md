# Recovered People Playground project

This project was reconstructed from the `Compled` Unity WebGL release build and is configured for Unity `2023.2.12f1`.

- Open `Assets/Scenes/Menu.unity` to inspect the recovered menu scene.
- `Assets/Scenes/Main.unity` contains the recovered main scene.
- Both scenes are configured in `ProjectSettings/EditorBuildSettings.asset`.
- The original `Compled` WebGL build remains in place.

The input was a compiled IL2CPP WebGL build, not the original Unity source project. It includes serialized assets and IL2CPP metadata, but not the original C# source or managed game assemblies. `RecoveredSource/` contains AssetRipper-generated placeholder scripts kept for reference; they are not the original gameplay implementation and are intentionally outside Unity's compile path. As a result, the recovered scenes and assets are available for editing, but the shipped game behavior cannot be faithfully restored from this folder alone.

The source WebGL player identifies itself as Unity `2022.3.30f1`; the reconstructed editor project was upgraded/configured for the requested `2023.2.12f1`. A few recovered assets have names that differ only by letter case. They are retained for preservation, so Unity may show case-sensitive filesystem warnings while importing them.
