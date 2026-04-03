using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractObject : MonoBehaviour
{
    public CPRAction phase;

    private GameManagerUpdated gameManager;
    private bool isInteractable = false;
    private GameFlowManager flowManager;

    [SerializeField] private GameObject graphicIndicator;
    [SerializeField] private Image feedbackImage;
    [SerializeField] private TextMeshProUGUI counter;
    [SerializeField] private TextMeshProUGUI total;
    [SerializeField] private GameObject TotalContainer;


    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManagerUpdated>();

        flowManager = FindFirstObjectByType<GameFlowManager>();
        if (flowManager != null)
            flowManager.OnPhaseChanged += HandlePhaseChanged;
    }

    private void OnDestroy()
    {
        if (flowManager != null)
            flowManager.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void Update()
    {
        if (!isInteractable)
            return;
        feedbackImage.fillAmount = !IsSamePhase() ? 1.0f  : gameManager.remainingTime / gameManager.expectedTime;
        graphicIndicator.SetActive(isInteractable);
        if (!IsSamePhase())
        {
            counter.text = "";
            TotalContainer.SetActive(false);
            return;
        }
        else 
        {
            counter.text = phase == CPRAction.compression ? gameManager.CompressionCount.ToString() : gameManager.BreathCount.ToString();
            total.text = phase == CPRAction.compression ? gameManager.TargetCompressionCount.ToString() : gameManager.TargetBreathCount.ToString();
            TotalContainer.SetActive (IsSamePhase());
        }
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
    public bool IsSamePhase()
    {
        bool remaingCompressions = gameManager.CompressionCount < gameManager.TargetCompressionCount;
        if (remaingCompressions)
        {
            return phase == CPRAction.compression;
        }
        else
        {
            return phase == CPRAction.breath  && gameManager.BreathCount < gameManager.TargetBreathCount;
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
