using UnityEngine;

public class InteractObject : MonoBehaviour
{
    public CPRAction phase;

    private GameManagerUpdated gameManager;
    private bool isInteractable = false;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManagerUpdated>();

        GameFlowManager flowManager = FindFirstObjectByType<GameFlowManager>();
        if (flowManager != null)
            flowManager.OnPhaseChanged += HandlePhaseChanged;
    }

    private void OnDestroy()
    {
        GameFlowManager flowManager = FindFirstObjectByType<GameFlowManager>();
        if (flowManager != null)
            flowManager.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        switch (newPhase)
        {
            case GamePhase.TutorialBreath:
                // Only the head/breath object responds
                isInteractable = (phase == CPRAction.breath);
                break;
            case GamePhase.TutorialCompression:
                // Only the chest/compression object responds
                isInteractable = (phase == CPRAction.compression);
                break;
            case GamePhase.FullCPR:
                // Both respond
                isInteractable = true;
                break;
            default:
                isInteractable = false;
                break;
        }
    }

    public void Interact()
    {
        if (!isInteractable || gameManager == null) return;

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
