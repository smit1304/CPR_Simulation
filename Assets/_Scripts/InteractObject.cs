using UnityEngine;

public class InteractObject : MonoBehaviour
{
    public CPRAction phase;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void Interact()
    {
        switch(phase)
        {
            case CPRAction.compression:
                gameManager.Compression();
                break;
            case CPRAction.breath:
                gameManager.Breath();
                break;
        }
    }
}
