using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform cameraTransform;

    private PlayerInput inputActions;
    private Vector2 lookInput;
    private float verticalRotation = 0f;
    void Awake()
    {
        inputActions = new PlayerInput();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        //Bloquea el raton al centro para evitar que salga de la pantalla como me pasaba en la torreta
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        // Rotacion de la capsula
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        // Rotación de la camara
        verticalRotation -= lookInput.y * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);

        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}
