using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickup : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask pickupLayer;

    [Header("Configuración")]
    [SerializeField] private float pickupDistance = 2f;
    [SerializeField] private float holdSmoothness = 10f;

    [Header("Pickup Mejorado")]
    [SerializeField] private float minHoldDistance = 1f;
    [SerializeField] private float maxHoldDistance = 4f;
    [SerializeField] private float scrollSpeed = 20f;
    private Vector3 lastPosition;
    private Vector3 calculatedVelocity;
    [SerializeField] private float throwMultiplier = 2f;

    [Header("info")]
    [SerializeField] private PlayerInput inputActions;
    [SerializeField] private bool isHolding = false;
    [SerializeField] private bool holdPressed = false;
    [SerializeField] private Rigidbody heldObject;
    [SerializeField] private float heldObjectDistance;

    void Awake()
    {
        inputActions = new PlayerInput();
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.PickUp.performed += OnPickupPerformed;
        inputActions.Player.PickUp.canceled += OnPickupCanceled;
    }

    void OnDisable()
    {
        inputActions.Player.PickUp.performed -= OnPickupPerformed;
        inputActions.Player.PickUp.canceled -= OnPickupCanceled;
        inputActions.Disable();
    }

    void OnPickupPerformed(InputAction.CallbackContext context)
    {
        holdPressed = true;
    }

    void OnPickupCanceled(InputAction.CallbackContext context)
    {
        holdPressed = false;
    }
    void Update()
    {

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (isHolding && Mathf.Abs(scroll) > 0f)
        {
            heldObjectDistance = Mathf.Clamp(
                heldObjectDistance + scroll * scrollSpeed * Time.deltaTime,
                minHoldDistance,
                maxHoldDistance
            );
        }

        if (!isHolding)
        {
            TryPickup();
        }
        else
        {
            HoldObject();
        }
    }

    void TryPickup()
    {
        // Raay cast
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, pickupLayer))
        {
           
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green); // Para el editor


            if (holdPressed)
            {
                Rigidbody rb = hit.collider.attachedRigidbody;
                if (rb != null)
                {
                    heldObject = rb;
                    heldObjectDistance = hit.distance;
                    heldObject.useGravity = false;
                    heldObject.linearDamping = 10f;
                    isHolding = true;
                }
            }
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * pickupDistance, Color.red);// Para el editor
        }
    }

    void HoldObject()
    {
        if (heldObject == null)
        {
            isHolding = false;
            return;
        }

        if (!holdPressed)
        {
            DropObject();
            return;
        }

        
        lastPosition = heldObject.position;

        Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * heldObjectDistance;
        Vector3 smoothedPos = Vector3.Lerp(heldObject.position, targetPos, Time.fixedDeltaTime * holdSmoothness);

        //suavidad
        heldObject.MovePosition(smoothedPos);

        
        calculatedVelocity = (heldObject.position - lastPosition) / Time.deltaTime;
    }

    void DropObject()
    {
        if (heldObject == null) return;

        heldObject.useGravity = true;
        heldObject.linearDamping = 0f;
        heldObject.angularDamping = 0f;

        // cambio de velocidad al soltar
        heldObject.linearVelocity = calculatedVelocity * throwMultiplier;

        heldObject = null;
        isHolding = false;
    }
}
