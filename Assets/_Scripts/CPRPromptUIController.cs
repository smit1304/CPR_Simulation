using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CPRPromptUIController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Header("Prompt UI")]
    [SerializeField] private GameObject btnChestPrompt;
    [SerializeField] private GameObject btnHeadPrompt;
    [SerializeField] private TMP_Text txtTimer;

    [Header("Timing")]
    [SerializeField] private float compressionTime = 0.5f;
    [SerializeField] private float breathTime = 2f;

    private float currentTimeLeft = 0f;
    private CPRAction currentPrompt = CPRAction.compression;

    private void Start()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager != null)
            gameManager.OnCPRExecute += HandleCPRExecute;

        ShowCompressionPrompt();
    }

    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.OnCPRExecute -= HandleCPRExecute;
    }

    private void Update()
    {
        if (currentTimeLeft > 0f)
            currentTimeLeft -= Time.deltaTime;

        if (txtTimer != null)
            txtTimer.text = $"Time: {Mathf.Max(0f, currentTimeLeft):F1}";
    }
    private void HandleCPRExecute((CPRAction action, bool makeMistake) result)
    {
        Debug.Log($"UI got event -> action: {result.Item1}, mistake: {result.Item2}");

        if (result.Item1 == CPRAction.compression)
        {
            ShowBreathPrompt();
        }
        else if (result.Item1 == CPRAction.breath)
        {
            ShowCompressionPrompt();
        }
        else if (result.Item1 == CPRAction.none && result.Item2)
        {
            ResetCurrentPromptTimer();
        }
    }
    private void ShowCompressionPrompt()
    {
        Debug.Log("UI -> ShowCompressionPrompt");
        currentPrompt = CPRAction.compression;

        if (btnChestPrompt != null)
            btnChestPrompt.SetActive(true);

        if (btnHeadPrompt != null)
            btnHeadPrompt.SetActive(false);

        currentTimeLeft = compressionTime;
    }

    private void ShowBreathPrompt()
    {
        Debug.Log("UI -> ShowBreathPrompt");
        currentPrompt = CPRAction.breath;

        if (btnChestPrompt != null)
            btnChestPrompt.SetActive(false);

        if (btnHeadPrompt != null)
            btnHeadPrompt.SetActive(true);

        currentTimeLeft = breathTime;
    }

    private void ResetCurrentPromptTimer()
    {
        if (currentPrompt == CPRAction.compression)
            currentTimeLeft = compressionTime;
        else if (currentPrompt == CPRAction.breath)
            currentTimeLeft = breathTime;
    }
}