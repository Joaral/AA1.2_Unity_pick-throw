using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckDistance = 1.1f;
    [SerializeField] private LayerMask groundMask;

    [Header("Movimiento Avanzado")]
    [SerializeField] private float sideMultiplier = 0.75f;
    [SerializeField] private float backMultiplier = 0.5f;
    [SerializeField] private float airMultiplier = 0.5f;
    [SerializeField] private float sprintMultiplier = 2f;

    [Header("JetPack")]
    [SerializeField] private float jetpackForce = 6f;
    [SerializeField] private float fuelDuration = 1f;
    [SerializeField] private float refillTime = 0.5f;
    [SerializeField] private float refillDelay = 0.5f;
    [SerializeField] private Image fuelUI;

    private float fuel = 1f;
    private bool jetpackActive;
    private bool wasGroundedLastFrame;
    private float refillTimer = 0f;


    [Header("Info")]
    [SerializeField] private PlayerInput inputActions;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Vector2 moveInput;
    [SerializeField] private bool jumpPressed;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isSprinting;

    void Awake()
    {
        inputActions = new PlayerInput();
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Jump.performed += OnJumpPerformed; //manera de suscribirse cuando se presiona el boton de salto
        inputActions.Player.Jump.canceled += OnJumpCanceled;
        inputActions.Player.Sprint.performed += OnSprintPerformed;
        inputActions.Player.Sprint.canceled += OnSprintCanceled;
    }

    void OnDisable()
    {
        inputActions.Player.Jump.performed -= OnJumpPerformed; //se deuscribe
        inputActions.Player.Jump.canceled -= OnJumpCanceled;
        inputActions.Player.Sprint.performed -= OnSprintPerformed;
        inputActions.Player.Sprint.canceled -= OnSprintCanceled;
        inputActions.Disable();
    }

    void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpPressed = true;
        jetpackActive = true;
    }
    void OnJumpCanceled(InputAction.CallbackContext context)
    {
        jetpackActive = false;
    }
    void OnSprintPerformed(InputAction.CallbackContext context)
    {
        isSprinting = true;
    }
    void OnSprintCanceled(InputAction.CallbackContext context)
    {
        isSprinting = false;
    }

    void Update()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        CheckGround();
        UpdateFuel(Time.deltaTime);
        UpdateFuelUI();
    }

    void FixedUpdate()
    {
        if (isGrounded)
        {
            MovePlayer();

            if (jumpPressed)
            {
                Jump();
                jumpPressed = false;
            }
        }
        else
        {
            jumpPressed = false;
            HandleJetpack();
        }
    }

    void CheckGround()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        isGrounded = Physics.Raycast(ray, groundCheckDistance, groundMask);

        // raycast para el editr
        Color rayColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, rayColor);

        if (isGrounded && !wasGroundedLastFrame)
        {
            if (fuel <= 0f)
                refillTimer = refillDelay;
        }

        wasGroundedLastFrame = isGrounded;
    }

    void MovePlayer()
    {
        Vector3 direction = transform.forward * moveInput.y + transform.right * moveInput.x;
        direction.Normalize();

        float speedMultiplier = 1f; //velocidad base hacia delante

        if (moveInput.y < 0) 
        {
            speedMultiplier *= backMultiplier;
        }
        else if (moveInput.x != 0)
        {
            speedMultiplier *= sideMultiplier;
        }

        if (!isGrounded)
        {
            speedMultiplier *= airMultiplier;
        }

        if (isGrounded && isSprinting && moveInput.y > 0)
        {
            speedMultiplier *= sprintMultiplier;
        }


        Vector3 velocity = new Vector3(direction.x * moveSpeed * speedMultiplier, rb.linearVelocity.y, direction.z * moveSpeed);
        rb.linearVelocity = velocity;
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void HandleJetpack()
    {
        if (jetpackActive && fuel > 0f)
        {
            rb.AddForce(Vector3.up * jetpackForce, ForceMode.Acceleration);
            fuel -= Time.fixedDeltaTime / fuelDuration;
            if (fuel < 0f) fuel = 0f;
        }
    }

    void UpdateFuel(float deltaTime)
    {
        if (isGrounded && fuel < 1f)
        {
            if (refillTimer > 0f)
            {
                refillTimer -= deltaTime; //penalizacion
            }
            else
            {
                fuel += deltaTime / refillTime;
                if (fuel > 1f) fuel = 1f;
            }
        }
    }
    void UpdateFuelUI()
    {
        if (fuelUI != null)
            fuelUI.fillAmount = fuel;
    }
}
