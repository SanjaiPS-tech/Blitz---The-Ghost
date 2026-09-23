# Player Controller Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a Rigidbody-based first-person player controller with force movement, single jumping, Ground-layer detection, and MainCamera mouse look.

**Architecture:** Keep all behavior in the existing `PlayerControll` MonoBehaviour. Sample legacy input and mouse look in `Update`, then apply movement and jump impulses in `FixedUpdate`; use a serialized `LayerMask` and sphere cast for grounding, and resolve the camera through `Camera.main`.

**Tech Stack:** Unity C#, `MonoBehaviour`, `Rigidbody`, legacy Unity Input Manager, `Physics.SphereCast`.

## Global Constraints

- Use the legacy Input Manager (`Input.GetAxis` and `Input.GetButtonDown`).
- Movement and jumping must use Rigidbody force APIs.
- Grounding must only accept colliders on the configured `Ground` layer mask.
- The first-person camera must be resolved using the `MainCamera` tag.
- Preserve unrelated existing worktree changes.

---

### Task 1: Implement the Rigidbody controller

**Files:**
- Modify: `Assets/Script/PlayerControll.cs`

**Interfaces:**
- Consumes: `Horizontal`, `Vertical`, `Mouse X`, `Mouse Y`, and `Jump` legacy input axes/buttons; a Rigidbody on the player; a camera tagged `MainCamera`.
- Produces: `PlayerControll` fields for movement force, jump force, look sensitivity, pitch limit, ground mask, probe radius, and probe distance; runtime force movement, grounded single jump, and first-person look.

- [ ] **Step 1: Add serialized configuration and runtime state**

Add `Rigidbody`, `Camera`, movement/jump/look settings, `LayerMask groundMask`, ground probe settings, and private movement/jump/pitch state. Initialize the Rigidbody and constrain its rotation in `Awake`; resolve `Camera.main` there and log an error when it is missing.

- [ ] **Step 2: Sample input and apply mouse look**

In `Update`, read the two movement axes into a `Vector2`, store jump intent from `Input.GetButtonDown("Jump")`, and rotate the player around its Y axis from `Mouse X`. Rotate the tagged camera around local X from `Mouse Y`, clamping pitch to `-90` through `90` degrees. Skip camera rotation when no tagged camera was found.

- [ ] **Step 3: Apply force movement and single jump**

In `FixedUpdate`, transform the sampled input into player-relative horizontal direction and call `Rigidbody.AddForce(direction * movementForce, ForceMode.Force)`. Perform a downward `Physics.SphereCast` from the player transform using the configured probe radius and distance plus `groundMask`; if jump intent is set and the cast hits, call `AddForce(Vector3.up * jumpForce, ForceMode.Impulse)`. Clear jump intent after each physics tick so one button press produces at most one jump.

- [ ] **Step 4: Set the default Ground layer mask safely**

In `Awake`, if the serialized mask is zero, look up the layer named `Ground` and set the mask to that layer when it exists. If the layer does not exist, log an error and leave the mask empty so the controller cannot falsely report grounded state.

- [ ] **Step 5: Review the resulting script**

Confirm the script uses no CharacterController or direct velocity assignment, movement is horizontal, pitch is clamped, Rigidbody rotation is constrained, and every ground cast uses `groundMask`.

- [ ] **Step 6: Validate**

Run the Unity project’s available compile/test validation. If no Unity test command is configured, inspect the script for compiler issues and verify the worktree diff with:

```bash
git diff --check
git diff -- Assets/Script/PlayerControll.cs
```

- [ ] **Step 7: Commit the implementation**

```bash
git add -- 'Assets/Script/PlayerControll.cs'
git commit -m "feat: add first person player controller" -m "Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
