using UnityEngine;
using UnityEngine.InputSystem;

public class CameraInteraction : MonoBehaviour
{
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
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Ray ray = mainCamera.ScreenPointToRay(mousePosition);
                
                Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 2f);

                RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
                
                Debug.Log($"[CameraInteraction] RaycastAll hit {hits.Length} objects");
                for (int i = 0; i < hits.Length; i++)
                {
                    Debug.Log($"  Hit {i}: {hits[i].collider.gameObject.name} at distance {hits[i].distance}");
                }
                
                if (hits.Length > 0)
                {
                    // Get the closest hit
                    RaycastHit closestHit = hits[0];
                    for (int i = 1; i < hits.Length; i++)
                    {
                        if (hits[i].distance < closestHit.distance)
                            closestHit = hits[i];
                    }

                    Debug.Log($"[CameraInteraction] Closest hit: {closestHit.collider.gameObject.name}");
                    
                    InteractObject interactable = closestHit.collider.GetComponent<InteractObject>();
                    if (interactable != null)
                    {
                        Debug.Log($"[CameraInteraction] Calling Interact() on {closestHit.collider.gameObject.name}");
                        interactable.Interact();
                    }
                }                
            }
        }
    }
}
