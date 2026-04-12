using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CPRPromptUIController : MonoBehaviour
{
    [SerializeField] private GameManagerUpdated gameManager;
    [SerializeField] private GameFlowManager gameFlowManager;

    [Header("Action Prompt Indicators")]
    [SerializeField] private GameObject chestPromptIndicator;
    [SerializeField] private GameObject headPromptIndicator;

    [Header("Timer")]
    [SerializeField] private TMP_Text txtTimer;

    [Header("Instruction & Progress")]
    [SerializeField] private TMP_Text txtInstruction;
    [SerializeField] private TMP_Text txtProgress;

    [Header("Mistake Feedback")]
    [SerializeField] private GameObject panelMistake;
    [SerializeField] private TMP_Text txtMistakesCont;
    [SerializeField] private TMP_Text txtMistakeFeedback;
    [SerializeField] private float mistakeFeedbackDuration = 1.5f;

    [Header("Completion Panel")]
    [SerializeField] private GameObject panelCompletion;
    [SerializeField] private TMP_Text txtCompletionMessage;
    [SerializeField] private Button btnContinue;

    [Header("Phase Transition Panel")]
    [SerializeField] private GameObject panelPhaseTransition;
    [SerializeField] private TMP_Text txtPhaseTitle;
    [SerializeField] private TMP_Text txtPhaseSubtitle;
    [SerializeField] private float transitionDisplayDuration = 2.5f;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject panelGameOver;
    [SerializeField] private TMP_Text txtGameOverMessage;
    [SerializeField] private Button btnRetry;

    private CPRAction currentPrompt = CPRAction.compression;
    private GamePhase currentPhase = GamePhase.MainMenu;

    private Coroutine mistakeFeedbackCoroutine;
    private Coroutine phaseTransitionCoroutine;

    // ─── Lifecycle ──────────────────────────────────────────────────
    private void Start()
    {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManagerUpdated>();
        if (gameFlowManager == null) gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        if (gameManager != null)
        {
            gameManager.OnCPRExecute += HandleCPRExecute;
            gameManager.OnLevelCompleted += HandleLevelCompleted;

            // In Start() — add these lines
            gameManager.OnGameOver += HandleGameOver;
            gameManager.OnTutorialFailed += HandleGameOver;
            gameManager.OnBreathPhaseStarted += HandleBreathPhaseStarted;
            gameManager.OnCycleCompleted += HandleCycleCompleted;
            gameManager.OnWaitTimeActivated += HandleWaitTimeActivated;
        }

        if (gameFlowManager != null)
        { 
            gameFlowManager.OnPhaseChanged += HandlePhaseChanged;
        }

        // Wire the Continue button to advance the flow
        if (btnContinue != null)
            btnContinue.onClick.AddListener(OnContinueClicked);


        if (btnRetry != null)
            btnRetry.onClick.AddListener(OnRetryClicked);
        
       
        
        HideAllUI();

        HandlePhaseChanged(gameFlowManager.CurrentPhase);

    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnCPRExecute -= HandleCPRExecute;
            gameManager.OnLevelCompleted -= HandleLevelCompleted;
            gameManager.OnWaitTimeActivated -= HandleWaitTimeActivated;
            gameManager.OnGameOver -= HandleGameOver;
            gameManager.OnTutorialFailed -= HandleGameOver;
            gameManager.OnBreathPhaseStarted -= HandleBreathPhaseStarted;
            gameManager.OnCycleCompleted -= HandleCycleCompleted;

        }
        if (gameFlowManager != null)
            gameFlowManager.OnPhaseChanged -= HandlePhaseChanged;

        if (btnContinue != null)
            btnContinue.onClick.RemoveListener(OnContinueClicked);

       
        if (btnRetry != null)
            btnRetry.onClick.RemoveListener(OnRetryClicked);
    }

    private void Update()
    {
        if (txtTimer != null)
        {
            if (gameManager.waiTimeBetweenCycles > 0)
            {
                txtTimer.text = gameManager.waiTimeBetweenCycles > 3 ? "Prepare to start" : $"Game will start in: {gameManager.waiTimeBetweenCycles:F0}";
            }
            else 
            {
                txtTimer.text =$"Time: {gameManager.remainingTime:F1}";
            }
        }
    }
    // ─── Wait Time Handler ──────────────────────────────────────────────
    private void HandleWaitTimeActivated(bool waitStarted)
    {
        if (!waitStarted) return;

        // Update progress after wait time ends
        UpdateProgressText();
    }

    // ─── Phase Changed ───────────────────────────────────────────────
    private void HandlePhaseChanged(GamePhase phase)
    {
        gameObject.SetActive(phase != GamePhase.MainMenu);
        currentPhase = phase;
        HideAllUI();

        switch (phase)
        {
            case GamePhase.TutorialBreath:
                ShowPhaseTransitionPanel("Level 1", "Rescute Breath");
                SetInstruction("Give a breath by clicking on the patient's head.");
                ShowBreathPrompt();
                UpdateProgressText();
                break;

            case GamePhase.TutorialCompression:
                ShowPhaseTransitionPanel("Level 2", "Compression");
                SetInstruction("Compress the chest by clicking on it.");
                ShowCompressionPrompt();
                UpdateProgressText();
                break;

            case GamePhase.FullCPR:
                ShowPhaseTransitionPanel("Level 3", "Save the Patient!");
                SetInstruction("30 compressions → 2 breaths. Keep the rhythm! Start with chest compressions.");
                ShowCompressionPrompt();
                UpdateProgressText();
                break;

            case GamePhase.MainMenu:
                HideAllUI();
                break;
        }
    }

    // ─── CPR Action Handler ──────────────────────────────────────────
    private void HandleCPRExecute((CPRAction action, bool makeMistake) result)
    {
        // Timeout — player took too long
        if (result.action == CPRAction.none && result.makeMistake)
        {
            ShowMistakeFeedback("Too slow! Keep the rhythm.");
            return;
        }

        if (result.makeMistake)
        {
            ShowMistakeFeedback(GetMistakeMessage(result.action));
            return; // Don't switch prompt — player must redo the correct action
        }

        // Correct action — update progress display
        UpdateProgressText();

        if (currentPhase == GamePhase.TutorialBreath || currentPhase == GamePhase.TutorialCompression)
        {
            // Stay on same prompt in tutorials, just reset the timer
        }
        else if (currentPhase == GamePhase.FullCPR)
        {
            bool compressionTargetMet = gameManager.CompressionCount >= gameManager.TargetCompressionCount 
                && gameManager.TotalRepetitions <= gameManager.CurrentRepetition;

            if (result.action == CPRAction.compression && compressionTargetMet)
            {
                SetInstruction("Great! Now give 2 breaths.");
                ShowBreathPrompt();
            }
            else if (result.action == CPRAction.breath && !compressionTargetMet)
            {
                SetInstruction("Now compress the chest again.");
                ShowCompressionPrompt();
            }
        }
    }

    private void HandleBreathPhaseStarted()
    {
        // Switch the prompt to breaths mid-cycle
        SetInstruction("30 compressions done! Now give 2 breaths.");
        ShowBreathPrompt();
        UpdateProgressText();
    }

    private void HandleCycleCompleted(int current, int total)
    {
        if (current < total)
        {
            // More cycles remaining
            string text = currentPrompt == CPRAction.compression ? "compressions" : "breath";
            SetInstruction($"Cycle {current}/{total} complete! Keep going — start {text} again.");
            UpdateProgressText();
        }
        // If current == total, OnLevelCompleted fires immediately after so no need to handle here
    }

    private void HandleLevelCompleted()
    {
        ShowCompletionPanel();
    }

    private void OnContinueClicked()
    {
        if (gameFlowManager != null)
            gameFlowManager.NextLevel();
    }

    // ─── Prompts ────────────────────────────────────────────────────
    private void ShowCompressionPrompt()
    {
        currentPrompt = CPRAction.compression;
        if (chestPromptIndicator != null) chestPromptIndicator.SetActive(true);
        if (headPromptIndicator != null) headPromptIndicator.SetActive(false);
    }

    private void ShowBreathPrompt()
    {
        currentPrompt = CPRAction.breath;
        if (chestPromptIndicator != null) chestPromptIndicator.SetActive(false);
        if (headPromptIndicator != null) headPromptIndicator.SetActive(true);
    }


    // ─── Mistake Feedback ────────────────────────────────────────────
    private string GetMistakeMessage(CPRAction wrongAction)
    {
        if (currentPhase == GamePhase.TutorialBreath)
            return "Wrong! Click the patient's head to give a breath.";

        if (currentPhase == GamePhase.TutorialCompression)
            return "Wrong! Click the patient's chest to compress.";

        // Full CPR
        bool shouldBeCompressing = gameManager.CompressionCount < gameManager.TargetCompressionCount;

        if (shouldBeCompressing && wrongAction == CPRAction.breath)
            return "Not yet! Finish your compressions first.";

        if (!shouldBeCompressing && wrongAction == CPRAction.compression)
            return "Stop! Now give breaths.";

        return "Wrong timing! Maintain a steady rhythm.";
    }

    private void ShowMistakeFeedback(string message)
    {
        if (mistakeFeedbackCoroutine != null) StopCoroutine(mistakeFeedbackCoroutine);
        mistakeFeedbackCoroutine = StartCoroutine(MistakeFeedbackRoutine(message));
    }

    private IEnumerator MistakeFeedbackRoutine(string message)
    {
        if (txtMistakeFeedback != null) txtMistakeFeedback.text = message;
        /*if (mistakeFeedbackCoroutine != null)        {
            // Update mistake count display
            
        }*/

        if (txtMistakesCont != null)
            txtMistakesCont.text = $"Mistakes: {gameManager.MistakeCount} / {(currentPhase == GamePhase.FullCPR ? gameManager.FullCPRMistakeLimit : gameManager.TutorialMistakeLimit)}";

        if (panelMistake != null) panelMistake.SetActive(true);
        yield return new WaitForSeconds(mistakeFeedbackDuration);
        if (panelMistake != null) panelMistake.SetActive(false);
    }

    // ─── Instruction & Progress ──────────────────────────────────────
    private void SetInstruction(string text)
    {
        if (txtInstruction != null) txtInstruction.text = text;
    }

    private void UpdateProgressText()
    {
        if (txtProgress == null || gameManager == null) return;

        bool compressionTargetMet = gameManager.CompressionCount >= gameManager.TargetCompressionCount;

        if (!compressionTargetMet)
            txtProgress.text = $"Compressions: {gameManager.CompressionCount} / {gameManager.TargetCompressionCount}";
        else
            txtProgress.text = $"Breaths: {gameManager.BreathCount} / {gameManager.TargetBreathCount}";
    }

    // ─── Phase Transition Panel ──────────────────────────────────────
    private void ShowPhaseTransitionPanel(string title, string subtitle)
    {
        if (phaseTransitionCoroutine != null) StopCoroutine(phaseTransitionCoroutine);
        phaseTransitionCoroutine = StartCoroutine(PhaseTransitionRoutine(title, subtitle));
    }

    private IEnumerator PhaseTransitionRoutine(string title, string subtitle)
    {
        if (txtPhaseTitle != null) txtPhaseTitle.text = title;
        if (txtPhaseSubtitle != null) txtPhaseSubtitle.text = subtitle;
        if (panelPhaseTransition != null) panelPhaseTransition.SetActive(true);
        yield return new WaitForSeconds(transitionDisplayDuration);
        if (panelPhaseTransition != null) panelPhaseTransition.SetActive(false);
    }

    // ─── Completion Panel ────────────────────────────────────────────
    private void ShowCompletionPanel()
    {
        string message = currentPhase switch
        {
            GamePhase.TutorialBreath => "Great job! You've learned how to give breaths.\nReady for the next challenge?",
            GamePhase.TutorialCompression => "Excellent! You've mastered chest compressions.\nNow let's put it all together!",
            GamePhase.FullCPR => "You saved the patient!\nOutstanding work!",
            _ => "Level Complete!"
        };

        if (txtCompletionMessage != null) txtCompletionMessage.text = message;
        if (panelCompletion != null) panelCompletion.SetActive(true);
    }

    private void HandleGameOver()
    {
        HideAllUI();
        if (txtGameOverMessage != null)
            txtGameOverMessage.text = "The patient didn't make it.\nTry again!";
        if (panelGameOver != null)
            panelGameOver.SetActive(true);
    }

    private void OnRetryClicked()
    {
        if (panelGameOver != null)
            panelGameOver.SetActive(false);
        if (gameFlowManager != null)
            gameFlowManager.RetryFullCPR();
    }

    // ─── Helpers ─────────────────────────────────────────────────────
    private void HideAllUI()
    {
        if (chestPromptIndicator != null) chestPromptIndicator.SetActive(false);
        if (headPromptIndicator != null) headPromptIndicator.SetActive(false);
        if (panelMistake != null) panelMistake.SetActive(false);
        if (panelCompletion != null) panelCompletion.SetActive(false);
        if (panelPhaseTransition != null) panelPhaseTransition.SetActive(false);
        if (txtProgress != null) txtProgress.text = "";
        if (txtInstruction != null) txtInstruction.text = "";
        if (panelGameOver != null) panelGameOver.SetActive(false);
    }
}