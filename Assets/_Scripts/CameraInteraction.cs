using UnityEngine;
using UnityEngine.InputSystem;

public class CameraInteraction : MonoBehaviour
{
    private Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (mainCamera != null)
            {
                // Create a ray from the current mouse cursor position on the screen
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Ray ray = mainCamera.ScreenPointToRay(mousePosition);
                
                // Draw the ray in the Scene view to debug exactly where it goes
                Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 2f);

                // Adding QueryTriggerInteraction.Collide just in case your BoxCollider is marked as "Is Trigger"
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
                {
                    
                    // Check if the object hit has an InteractObject component
                    InteractObject interactable = hit.collider.GetComponent<InteractObject>();
                    if (interactable != null)
                    {
                        interactable.Interact();
                    }
                }                
            }
        }
    }
}
