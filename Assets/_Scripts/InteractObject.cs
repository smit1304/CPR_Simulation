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
    [SerializeField] private Image middleImage;
    [SerializeField] private TextMeshProUGUI counter;
    [SerializeField] private TextMeshProUGUI total;
    [SerializeField] private GameObject TotalContainer;
    [SerializeField] private Color correctColor;
    [SerializeField] private Color incorrectColor;
    [SerializeField] private Color neutralColor;


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
        
        float progress = gameManager.remainingTime / gameManager.expectedTime;
        feedbackImage.fillAmount = !IsSamePhase() ? 1.0f : progress;
        graphicIndicator.SetActive(isInteractable);
        
        if (!IsSamePhase())
        {
            counter.text = "";
            TotalContainer.SetActive(false);
            if (middleImage != null) middleImage.color = neutralColor;
            return;
        }
        else 
        {
            counter.text = phase == CPRAction.compression ? gameManager.CompressionCount.ToString() : gameManager.BreathCount.ToString();
            total.text = phase == CPRAction.compression ? gameManager.TargetCompressionCount.ToString() : gameManager.TargetBreathCount.ToString();
            TotalContainer.SetActive(true);

            // Change middleImage color based on the exact timing required by GameManagerUpdated.
            // GameManagerUpdated has a TIMING_MARGIN of 0.15f before exactly hitting the expectedTime.
            // Therefore, you are in the "correct" timing range if remaining time is 0.15 seconds or less.
            bool isCorrectRange = gameManager.remainingTime <= 0.15f;

            if (middleImage != null)
            {
                middleImage.color = isCorrectRange ? correctColor : incorrectColor;
            }
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
