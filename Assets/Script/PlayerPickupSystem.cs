using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerPickupSystem : MonoBehaviour
{
    [Header("Player")]
    public Camera playerCamera;
    public Transform holdPoint;

    [Header("Pickup")]
    public float pickupRange = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Placement")]
    public float placementRange = 2.5f;

    [Header("Item Holder")]
    public Transform itemHolder;
    public GameObject placedItem;

    [Header("Sword Settings")]
    [Tooltip("Drag and drop your sword GameObject from the Hierarchy here (e.g. child of Right Hand).")]
    public GameObject sword;

    [Tooltip("Optional: Drag and drop your sword Transform directly here.")]
    public Transform swordTransform;

    [Header("Sword Swing Motion")]
    [Tooltip("Total duration of one swing in seconds.")]
    public float swingDuration = 0.22f;

    [Tooltip("Cooldown between swings in seconds.")]
    public float swingCooldown = 0.35f;

    [Tooltip("Rotation offset during the swing slash (Pitch, Yaw, Roll).")]
    public Vector3 swingRotation = new Vector3(50f, -60f, 25f);

    [Tooltip("Position offset during the swing (e.g. forward thrust and slight drop).")]
    public Vector3 swingPositionOffset = new Vector3(0.05f, -0.05f, 0.15f);

    [Header("Sword Animation & Audio (Optional)")]
    [Tooltip("Optional Animator if using an animation clip instead of/alongside procedural swing.")]
    public Animator swordAnimator;

    [Tooltip("Animator trigger parameter name.")]
    public string swingAnimationTrigger = "Swing";

    [Tooltip("Optional sound to play on swing.")]
    public AudioClip swingSound;

    [Tooltip("Optional AudioSource. If unassigned, audio will be played at the sword position.")]
    public AudioSource audioSource;

    [Header("Sword Hit Detection (Optional)")]
    [Tooltip("Enable raycast hit detection when swinging.")]
    public bool enableHitDetection = true;

    [Tooltip("Maximum distance in front of the camera that the sword can hit.")]
    public float attackRange = 2.5f;

    [Tooltip("Damage dealt to objects that have a TakeDamage method.")]
    public float attackDamage = 25f;

    [Tooltip("Layers the sword can hit.")]
    public LayerMask hitLayers = ~0;

    private GameObject heldObject;
    private Rigidbody heldRigidbody;

    // Sword state tracking
    private bool isSwinging = false;
    private float nextSwingTime = 0f;
    private Vector3 defaultSwordLocalPos;
    private Quaternion defaultSwordLocalRot;
    private bool defaultTransformSaved = false;
    private Coroutine activeSwingCoroutine;

    private void Start()
    {
        // Make sure the final item starts disabled
        if (placedItem != null)
        {
            placedItem.SetActive(false);
        }

        SaveDefaultSwordTransform();
    }

    private void Update()
    {
        // ---------------------------------------------------------
        // 1. PICKUP & PLACEMENT (Interact Key: default E)
        // ---------------------------------------------------------
        if (IsInteractPressed())
        {
            // Empty hand → pickup
            if (heldObject == null)
            {
                TryPickup();
            }
            // Holding something → try to place
            else
            {
                TryPlace();
            }
        }

        // ---------------------------------------------------------
        // 2. SWORD SWING (Left Mouse Button)
        // ---------------------------------------------------------
        if (IsLeftClickPressed())
        {
            TrySwingSword();
        }
    }

    private bool IsInteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            return true;
#endif
        return Input.GetKeyDown(interactKey);
    }

    private bool IsLeftClickPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;
#endif
        return Input.GetMouseButtonDown(0);
    }

    // =========================================================
    // SWORD SYSTEM
    // =========================================================

    public Transform GetSwordTransform()
    {
        if (swordTransform != null)
            return swordTransform;

        if (sword != null)
            return sword.transform;

        return null;
    }

    private void SaveDefaultSwordTransform()
    {
        Transform sTransform = GetSwordTransform();
        if (sTransform != null)
        {
            defaultSwordLocalPos = sTransform.localPosition;
            defaultSwordLocalRot = sTransform.localRotation;
            defaultTransformSaved = true;
        }
    }

    public void TrySwingSword()
    {
        Transform sTransform = GetSwordTransform();
        if (sTransform == null)
        {
            Debug.LogWarning("PlayerPickupSystem: No sword assigned! Drag and drop your sword into the 'Sword' field in the Inspector.");
            return;
        }

        if (isSwinging || Time.time < nextSwingTime)
            return;

        nextSwingTime = Time.time + swingCooldown;

        if (!defaultTransformSaved)
        {
            SaveDefaultSwordTransform();
        }

        if (activeSwingCoroutine != null)
        {
            StopCoroutine(activeSwingCoroutine);
        }

        activeSwingCoroutine = StartCoroutine(PerformSwordSwing());
    }

    private IEnumerator PerformSwordSwing()
    {
        isSwinging = true;

        Transform sTransform = GetSwordTransform();
        if (sTransform == null)
        {
            isSwinging = false;
            yield break;
        }

        // Play sound if assigned
        if (swingSound != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(swingSound);
            else
                AudioSource.PlayClipAtPoint(swingSound, sTransform.position);
        }

        // Trigger Animator if assigned
        if (swordAnimator != null)
        {
            swordAnimator.SetTrigger(swingAnimationTrigger);
        }

        // Hit detection
        if (enableHitDetection)
        {
            PerformHitDetection();
        }

        // Procedural swing slash animation
        Quaternion startRot = defaultSwordLocalRot;
        Vector3 startPos = defaultSwordLocalPos;

        Quaternion peakRot = startRot * Quaternion.Euler(swingRotation);
        Vector3 peakPos = startPos + swingPositionOffset;

        float forwardDuration = Mathf.Max(0.01f, swingDuration * 0.35f);
        float returnDuration = Mathf.Max(0.01f, swingDuration * 0.65f);

        // Phase 1: Forward Slash (fast, punchy)
        float elapsed = 0f;
        while (elapsed < forwardDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / forwardDuration);
            float ease = Mathf.Sin(t * Mathf.PI * 0.5f);

            sTransform.localRotation = Quaternion.Slerp(startRot, peakRot, ease);
            sTransform.localPosition = Vector3.Lerp(startPos, peakPos, ease);
            yield return null;
        }

        sTransform.localRotation = peakRot;
        sTransform.localPosition = peakPos;

        // Phase 2: Smooth Return
        elapsed = 0f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / returnDuration);
            float ease = Mathf.SmoothStep(0f, 1f, t);

            sTransform.localRotation = Quaternion.Slerp(peakRot, startRot, ease);
            sTransform.localPosition = Vector3.Lerp(peakPos, startPos, ease);
            yield return null;
        }

        // Restore exact rest pose
        sTransform.localRotation = startRot;
        sTransform.localPosition = startPos;

        isSwinging = false;
        activeSwingCoroutine = null;
    }

    private void PerformHitDetection()
    {
        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam == null)
            return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange, hitLayers))
        {
            Debug.Log("Sword hit: " + hit.collider.gameObject.name);

            // Send damage message if receiver supports it
            hit.collider.gameObject.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);

            // Apply knockback if object has non-kinematic Rigidbody
            Rigidbody hitRb = hit.collider.attachedRigidbody;
            if (hitRb != null && !hitRb.isKinematic)
            {
                hitRb.AddForce(cam.transform.forward * 5f, ForceMode.Impulse);
            }
        }
    }

    private void OnDisable()
    {
        if (isSwinging)
        {
            Transform sTransform = GetSwordTransform();
            if (sTransform != null && defaultTransformSaved)
            {
                sTransform.localPosition = defaultSwordLocalPos;
                sTransform.localRotation = defaultSwordLocalRot;
            }
            isSwinging = false;
            activeSwingCoroutine = null;
        }
    }

    // =========================================================
    // PICKUP
    // =========================================================

    private void TryPickup()
    {
        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("PlayerPickupSystem: Camera is not assigned!");
            return;
        }

        Ray ray = new Ray(
            cam.transform.position,
            cam.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            GameObject target = hit.collider.gameObject;

            if (!target.CompareTag("Collectable"))
                return;

            heldObject = target;

            heldRigidbody = heldObject.GetComponent<Rigidbody>();

            if (heldRigidbody != null)
            {
                heldRigidbody.isKinematic = true;
                heldRigidbody.useGravity = false;
            }

            heldObject.transform.SetParent(holdPoint);

            heldObject.transform.localPosition = Vector3.zero;
            heldObject.transform.localRotation = Quaternion.identity;

            Debug.Log("Picked up: " + heldObject.name);
        }
    }

    // =========================================================
    // PLACE
    // =========================================================

    private void TryPlace()
    {
        if (itemHolder == null)
        {
            Debug.LogError("Item Holder is NOT assigned!");
            return;
        }

        if (placedItem == null)
        {
            Debug.LogError("Placed Item is NOT assigned!");
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            itemHolder.position
        );

        Debug.Log("Distance to ItemHolder: " + distance);

        if (distance > placementRange)
        {
            Debug.Log("Too far from ItemHolder.");
            return;
        }

        // -----------------------------------------
        // DESTROY ITEM IN HAND
        // -----------------------------------------

        Destroy(heldObject);

        heldObject = null;
        heldRigidbody = null;

        // -----------------------------------------
        // ENABLE EXISTING ITEM
        // -----------------------------------------

        placedItem.SetActive(true);

        Debug.Log("Item placed successfully!");
    }
}