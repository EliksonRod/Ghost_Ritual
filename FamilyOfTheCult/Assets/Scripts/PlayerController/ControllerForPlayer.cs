using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Unity.VisualScripting;

//[RequireComponent(typeof(CharacterController))]
public class ControllerForPlayer : MonoBehaviour
{
    public static ControllerForPlayer Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] float walkSpeed = 4.0f;
    [SerializeField] float sprintSpeed = 7.0f;
    float rotationSpeed = 1.0f;
    float accelerationRate = 10.0f;
    float decelerationRate = 10f;

    [Header("Jump Settings")]
    [SerializeField] float jumpHeight = 2f;
    [SerializeField] float gravity = -20f;
    float jumpCooldown = 0.2f;

    [Header("Grounded Settings")]
    [SerializeField] float groundedOffset = .85f;
    [SerializeField] float groundedRadius = 0.3f;
    [SerializeField] LayerMask groundLayers;

    [Header("Camera Settings")]
    public CinemachineCamera virtualCamera;
    [HideInInspector] public float originalFov;
    [SerializeField] float maxCameraPitch = 70f;
    [SerializeField] float minCameraPitch = -70f;

    [Header("Headbob Settings")]
    public CinemachineBasicMultiChannelPerlin headBob;
    public float headBobAcceleration = 10f;
    public float idleBobAmp = .5f;
    public float idleBobFreq = 1f;
    public float walkBobAmp = 3f;
    public float walkBobFreq = 1f;
    public float sprintBobAmp = 4f;
    public float sprintBobFreq = 3f;

    [Header("Interact Settings")]
    public bool isInteracting = false;

    [Header("Character Input Values")]
    [HideInInspector] public Vector2 move;
    [HideInInspector] public Vector2 look;
    [HideInInspector] public bool sprint;

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private float jumpCooldownTimer;
    private float cameraPitch;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        var playerInput = GetComponent<PlayerInput>();
        Instance = this;
    }

    void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Cinemachine Virtual Camera is not assigned.");
        }
        originalFov = virtualCamera.Lens.FieldOfView;
        //currentFov = Mathf.Clamp(currentFov, (originalFov * (60f / 100f)), originalFov);
    }

    void Update()
    {
        HandleMovement();
        HandleGravity();
        GroundedCheck();
    }

    public void Look(InputAction.CallbackContext context)
    {
        //if (canUseInput == false) return;
        look = context.ReadValue<Vector2>();
        //if (isInteracting) { return; }

        Vector2 lookInput = look;
        cameraPitch += lookInput.y * rotationSpeed;
        cameraPitch = Mathf.Clamp(cameraPitch, minCameraPitch, maxCameraPitch);

        virtualCamera.transform.localEulerAngles = new Vector3(cameraPitch, 0, 0);
        transform.Rotate(Vector3.up * lookInput.x * rotationSpeed);
    }
    public void Move(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        sprint = context.ReadValueAsButton();
    }
    void HandleMovement()
    {
        if (isInteracting)
        {
            move = Vector2.zero;
            velocity = Vector3.zero;

            headBob.AmplitudeGain = idleBobAmp;
            headBob.FrequencyGain = idleBobFreq;
            return;
        }

        HeadBob();

        Vector2 input = move;
        Vector3 moveDirection = transform.right * input.x + transform.forward * input.y;

        float targetSpeed = sprint ? sprintSpeed : walkSpeed;

        if (moveDirection != Vector3.zero)
        {
            velocity.x = Mathf.Lerp(velocity.x, targetSpeed * moveDirection.x, Time.deltaTime * accelerationRate);
            velocity.z = Mathf.Lerp(velocity.z, targetSpeed * moveDirection.z, Time.deltaTime * accelerationRate);
        }
        else
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, Time.deltaTime * decelerationRate);
            velocity.z = Mathf.Lerp(velocity.z, 0, Time.deltaTime * decelerationRate);
        }

        characterController.Move(new Vector3(velocity.x, 0, velocity.z) * Time.deltaTime);
    }

    public void HandleRotation()
    {
        if (isInteracting) { return; }

        Vector2 lookInput = look;
        cameraPitch += lookInput.y * rotationSpeed;
        cameraPitch = Mathf.Clamp(cameraPitch, minCameraPitch, maxCameraPitch);

        virtualCamera.transform.localEulerAngles = new Vector3(cameraPitch, 0, 0);
        transform.Rotate(Vector3.up * lookInput.x * rotationSpeed);
    }
    void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    void HandleGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(Vector3.up * velocity.y * Time.deltaTime);
    }

    public void HandleJumping(InputAction.CallbackContext context)
    {
        if (isGrounded && context.performed)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpCooldownTimer = jumpCooldown;
        }
    }

    public void ChangeFov(float fieldOfView)
    {
        virtualCamera.Lens.FieldOfView = Mathf.Lerp(virtualCamera.Lens.FieldOfView, fieldOfView, Time.deltaTime * 4);
        //virtualCamera.Lens.FieldOfView.CompareTo(fieldOfView);
    }

    public void ResetFov()
    {
        virtualCamera.Lens.FieldOfView = Mathf.Lerp(virtualCamera.Lens.FieldOfView, originalFov, Time.deltaTime * 4);
        //virtualCamera.Lens.FieldOfView = originalFov;
    }
    void HeadBob()
    {
        float moveMagnitude = move.magnitude;
        float targetAmp = moveMagnitude > 0 ? (sprint ? sprintBobAmp : walkBobAmp) : idleBobAmp;
        float targetFreq = moveMagnitude > 0 ? (sprint ? sprintBobFreq : walkBobFreq) : idleBobFreq;

        headBob.AmplitudeGain = Mathf.Lerp(headBob.AmplitudeGain, targetAmp, Time.deltaTime * headBobAcceleration);
        headBob.FrequencyGain = Mathf.Lerp(headBob.FrequencyGain, targetFreq, Time.deltaTime * headBobAcceleration);
    }
    void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (isGrounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z), groundedRadius);
    }
}
