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
        //Debug.Log($"InteractObject clicked: {gameObject.name}, phase: {phase}");
        switch (phase)
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
