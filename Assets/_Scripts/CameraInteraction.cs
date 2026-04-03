using UnityEngine;
using UnityEngine.InputSystem;

public class CameraInteraction : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (mainCamera != null)
            {
                // Create a ray from the current mouse cursor position on the screen
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Ray ray = mainCamera.ScreenPointToRay(mousePosition);

#if UNITY_EDITOR
                // Draw the ray in the Scene view to debug exactly where it goes
                Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 2f);
#endif

                // Adding QueryTriggerInteraction.Collide just in case your BoxCollider is marked as "Is Trigger"
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    Debug.Log("Interact with");

                    // Check if the object hit has an InteractObject component
                    InteractObject interactable = hit.collider.GetComponentInChildren<InteractObject>(true);
                    if (interactable != null)
                    {
                        interactable.Interact();
                    }
                }
            }
        }
    }
}
