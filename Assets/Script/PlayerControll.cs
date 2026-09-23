using UnityEngine;

public class PlayerControll : MonoBehaviour
{
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float movementForce = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float pitchLimit = 90f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float probeRadius = 0.2f;
    [SerializeField] private float probeDistance = 0.1f;

    private Vector2 movementInput;
    private bool jumpRequested;
    private float pitch;

    private void Awake()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }

        if (playerRigidbody == null)
        {
            Debug.LogError("PlayerControll requires a Rigidbody.", this);
        }
        else
        {
            playerRigidbody.constraints |= RigidbodyConstraints.FreezeRotation;
        }

        Camera mainCamera = Camera.main;
        if (playerCamera != null && mainCamera != null && playerCamera != mainCamera)
        {
            Debug.LogError("PlayerControll camera must be tagged MainCamera.", this);
        }

        playerCamera = mainCamera;
        if (playerCamera == null)
        {
            Debug.LogError("PlayerControll requires a camera tagged MainCamera.", this);
        }
        else
        {
            pitch = playerCamera.transform.localEulerAngles.x;
            if (pitch > 180f)
            {
                pitch -= 360f;
            }

            float effectivePitchLimit = Mathf.Clamp(pitchLimit, 0f, 90f);
            pitch = Mathf.Clamp(pitch, -effectivePitchLimit, effectivePitchLimit);
        }

        if (groundMask.value == 0)
        {
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
            {
                groundMask = 1 << groundLayer;
            }
            else
            {
                Debug.LogError("PlayerControll could not find a layer named Ground.", this);
            }
        }
    }

    private void Update()
    {
        movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        jumpRequested = Input.GetButtonDown("Jump");

        transform.Rotate(0f, Input.GetAxis("Mouse X") * lookSensitivity, 0f);

        if (playerCamera == null)
        {
            return;
        }

        pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
        float effectivePitchLimit = Mathf.Clamp(pitchLimit, 0f, 90f);
        pitch = Mathf.Clamp(pitch, -effectivePitchLimit, effectivePitchLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void FixedUpdate()
    {
        if (playerRigidbody == null)
        {
            jumpRequested = false;
            return;
        }

        Vector3 direction = transform.TransformDirection(new Vector3(movementInput.x, 0f, movementInput.y));
        direction.y = 0f;
        playerRigidbody.AddForce(direction * movementForce, ForceMode.Force);

        bool isGrounded = groundMask.value != 0 &&
                          Physics.SphereCast(
                              transform.position,
                              probeRadius,
                              Vector3.down,
                              out _,
                              probeDistance,
                              groundMask,
                              QueryTriggerInteraction.Ignore);

        if (jumpRequested && isGrounded)
        {
            playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        jumpRequested = false;
    }
}
