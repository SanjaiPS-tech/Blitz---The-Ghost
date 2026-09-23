# Player Controller Design

## Goal

Add a first-person player controller to `Assets/Script/PlayerControll.cs` using
the legacy Unity Input Manager, Rigidbody forces, single jump behavior, and a
layer-based ground check for the `Ground` layer.

## Behavior

- Horizontal movement reads `Horizontal` and `Vertical` axes and applies a
  force relative to the player.
- Jumping reads the `Jump` button and applies one upward impulse only while the
  player is grounded.
- Grounded state is detected with a downward sphere cast using a serialized
  `Ground` layer mask.
- The player Rigidbody has rotation constrained so physics cannot tip the
  player over.
- Horizontal mouse movement rotates the player; vertical mouse movement rotates
  the camera and is clamped to a natural first-person range.
- The camera is resolved using the `MainCamera` tag. A clear error is logged if
  no camera with that tag exists.

## Configuration

The controller exposes movement force, jump force, mouse sensitivity, ground
probe radius/distance, and the ground layer mask in the Inspector. The default
mask targets the layer named `Ground` when available, while still allowing the
mask to be adjusted per prefab.

## Update Model

- `Update` handles input sampling, mouse look, and jump intent.
- `FixedUpdate` applies Rigidbody movement and consumes jump intent when the
  ground check succeeds.

## Error Handling

Missing `MainCamera` is reported with `Debug.LogError` and disables mouse-look
camera rotation without preventing Rigidbody movement.

## Validation

The script will be checked for compile errors and reviewed to ensure it uses
the legacy input API, Rigidbody force APIs, `MainCamera` lookup, and a
`Ground`-layer-only ground check.
