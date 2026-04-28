# Project Asset Layout

Top-level gameplay folders under `Assets` store assets created or maintained for this game.
The imported `Assets/Resources` folder is kept as a source library for art, audio, fonts, and video.

## Main Folders

- `Animations/Characters/Player/Knight`
  - Player animation clips and Animator Controller.
- `Prefabs/Characters/Player`
  - Player character prefabs and child prefabs.
- `Prefabs/Characters/Enemies`
  - Enemy prefabs.
- `Prefabs/Environment`
  - Reusable platforms, props, interactable objects, and level pieces.
- `Configs/Characters/Player`
  - Player ScriptableObject configs, such as `PlayerConfig` assets.
- `Input/Player`
  - Player Input System action assets.
- `Art/Characters/Player`
  - Curated or edited player art assets. Keep imported source sprites in `Resources` unless you are deliberately reorganizing them.
- `Audio/Music`
  - Music clips used by scenes or audio managers.
- `Audio/SFX`
  - Sound effects used by gameplay.
- `Materials`
  - Shared materials and visual tuning assets.
- `Physics`
  - Physics materials and collision-related assets.

## Rules

- Keep external imported source packages in `Assets/Resources` until you are ready for a deliberate migration.
- Keep gameplay-ready assets in the matching top-level folder under `Assets`.
- Move files together with their `.meta` files so Unity references keep the same GUID.
- Prefer referencing assets from Inspector fields or ScriptableObjects instead of hard-coded `Resources.Load` paths.
