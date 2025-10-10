// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.UI;

// public enum InteractionMode
// {
//     CenterRay,
//     MouseRay
// }

// public class PlayerInteraction : MonoBehaviour
// {
//     [Header("References")]
//     public Camera playerCamera;
//     public float interactDistance = 3f;
//     public LayerMask interactMask;
//     public Transform holdPoint;
//     // public Image crosshair;
//     // public Text interactText;
//     public InteractionMode interactionMode = InteractionMode.CenterRay;

//     private IInteractable currentInteractable;
//     private GameObject heldObject;
//     private Rigidbody heldRb;

//     private PlayerInputActions inputActions;

//     void Awake()
//     {
//         inputActions = new PlayerInputActions();
//         inputActions.Player.Enable();
//         inputActions.Player.Interact.performed += ctx => TryInteract();
//         inputActions.Player.Throw.performed += ctx => ThrowObject();
//     }

//     void Update()
//     {
//         if (heldObject) return;

//         Ray ray = GetRay();
//         Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 3f, Color.red);
//         if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
//         {
//             Debug.Log("hit " + hit.collider.transform.gameObject.name);
//             if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
//             {
//                 if (interactable != currentInteractable)
//                 {
//                     currentInteractable?.OnLoseFocus();
//                     currentInteractable = interactable;
//                     currentInteractable.OnFocus();
//                 }

//                 // interactText.text = interactable.GetInteractText();
//                 // interactText.enabled = true;
//                 // crosshair.color = Color.yellow;
//                 return;
//             }
//         }

//         if (currentInteractable != null)
//         {
//             currentInteractable.OnLoseFocus();
//             currentInteractable = null;
//         }

//         // interactText.enabled = false;
//         // crosshair.color = Color.white;
//     }

//     Ray GetRay()
//     {
//         if (interactionMode == InteractionMode.MouseRay)
//         {
//             Vector2 mousePos = Mouse.current.position.ReadValue();
//             return playerCamera.ScreenPointToRay(mousePos);
//         }
//         else
//         {
//             return new Ray(playerCamera.transform.position, playerCamera.transform.forward);
//         }
//     }

//     void TryInteract()
//     {
//         if (currentInteractable != null)
//             currentInteractable.OnInteract();
//     }

//     public void PickUpObject(GameObject obj)
//     {
//         heldObject = obj;
//         heldRb = obj.GetComponent<Rigidbody>();
//         heldRb.isKinematic = true;
//         obj.transform.SetParent(holdPoint);
//         obj.transform.localPosition = Vector3.zero;
//     }

//     public void ThrowObject()
//     {
//         if (!heldObject) return;
//         heldRb.isKinematic = false;
//         heldObject.transform.SetParent(null);
//         heldRb.AddForce(playerCamera.transform.forward * 6f, ForceMode.VelocityChange);
//         heldObject = null;
//         heldRb = null;
//     }
// }


using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public float interactDistance = 3f;
    public LayerMask interactMask;
    public Transform holdPoint;
    public Image crosshair;
    public Text interactText;

    private IInteractable currentInteractable;
    private GameObject heldObject;
    private Rigidbody heldRb;

    private PlayerInputActions inputActions;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        inputActions.Player.Interact.performed += ctx => TryInteract();
        inputActions.Player.Throw.performed += ctx => ThrowObject();
    }

    void Update()
    {
        if (heldObject) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                if (interactable != currentInteractable)
                {
                    currentInteractable?.OnLoseFocus();
                    currentInteractable = interactable;
                    currentInteractable.OnFocus();
                }

                interactText.text = interactable.GetInteractText();
                interactText.enabled = true;
                crosshair.color = Color.yellow;
                return;
            }
        }

        if (currentInteractable != null)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
        interactText.enabled = false;
        crosshair.color = Color.white;
    }

    void TryInteract()
    {
        if (currentInteractable != null)
            currentInteractable.OnInteract();
    }

    public void PickUpObject(GameObject obj)
    {
        heldObject = obj;
        heldRb = obj.GetComponent<Rigidbody>();
        heldRb.isKinematic = true;
        obj.transform.SetParent(holdPoint);
        obj.transform.localPosition = Vector3.zero;
    }

    public void ThrowObject()
    {
        if (!heldObject) return;
        heldRb.isKinematic = false;
        heldObject.transform.SetParent(null);
        heldRb.AddForce(playerCamera.transform.forward * 6f, ForceMode.VelocityChange);
        heldObject = null;
        heldRb = null;
    }
}
