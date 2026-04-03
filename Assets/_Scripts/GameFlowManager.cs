using System;
using UnityEngine;

public enum GamePhase
{
    MainMenu,
    TutorialBreath,
    TutorialCompression,
    FullCPR
}


public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("Level Objectives")]
    [SerializeField] private LevelObjective tutorialBreathObjective;
    [SerializeField] private LevelObjective tutorialCompressionObjective;
    [SerializeField] private LevelObjective fullCPRObjective;

    [Header("References")]
    [SerializeField] private GameManagerUpdated gameManager;

    private GamePhase currentPhase = GamePhase.MainMenu;
    public GamePhase CurrentPhase => currentPhase;

    public event Action<GamePhase> OnPhaseChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManagerUpdated>();

        if (gameManager != null)
        {
            gameManager.OnTutorialFailed += HandleTutorialFailed;
            gameManager.OnGameOver += HandleGameOver;
            gameManager.OnLevelCompleted += HandleLevelComplete;
        }
    }

    private void HandleLevelComplete()
    {
        Time.timeScale = 0f; // Pause the game until NextLevel is pressed
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnTutorialFailed -= HandleTutorialFailed;
            gameManager.OnGameOver -= HandleGameOver;
        }
    }

    // Debug skip — press N in editor to advance
    private void Update()
    {
#if UNITY_EDITOR
        if (UnityEngine.InputSystem.Keyboard.current.nKey.wasPressedThisFrame)
        {
            Debug.Log("[GameFlowManager] DEBUG: Manual NextLevel triggered.");
            NextLevel();
        }
#endif
    }

    public void StartGame()
    {
        SetPhase(GamePhase.TutorialBreath);
    }

    public void NextLevel()
    {
        switch (currentPhase)
        {
            case GamePhase.MainMenu:
                SetPhase(GamePhase.TutorialBreath);
                break;
            case GamePhase.TutorialBreath:
                SetPhase(GamePhase.TutorialCompression);
                break;
            case GamePhase.TutorialCompression:
                SetPhase(GamePhase.FullCPR);
                break;
            case GamePhase.FullCPR:
                SetPhase(GamePhase.MainMenu);
                Time.timeScale = 0f;
                break;
        }
    }

    // Restart the current tutorial phase from the beginning
    private void HandleTutorialFailed()
    {
        Debug.Log($"[GameFlowManager] Tutorial failed — restarting phase: {currentPhase}");
        SetPhase(currentPhase); // Re-set the same phase — reloads level objective and resets everything
        Time.timeScale = 0f;
    }

    // Game Over — UI handles showing the panel, we just pause
    private void HandleGameOver()
    {
        Debug.Log("[GameFlowManager] Game Over.");
        Time.timeScale = 0f; // Freeze the game until Retry is pressed
    }

    public void RetryFullCPR()
    {
        Time.timeScale = 1f;
        SetPhase(currentPhase);
    }

    private void SetPhase(GamePhase newPhase)
    {
        currentPhase = newPhase;
        Debug.Log($"[GameFlowManager] Phase changed to: {newPhase}");
        ApplyPhaseToGameManager(newPhase);
        OnPhaseChanged?.Invoke(newPhase);
        Time.timeScale = 1f;
    }

    private void ApplyPhaseToGameManager(GamePhase phase)
    {
        if (gameManager == null) return;

        switch (phase)
        {
            case GamePhase.TutorialBreath:
                gameManager.SetLevel(tutorialBreathObjective, fullCPR: false);
                break;
            case GamePhase.TutorialCompression:
                gameManager.SetLevel(tutorialCompressionObjective, fullCPR: false);
                break;
            case GamePhase.FullCPR:
                gameManager.SetLevel(fullCPRObjective, fullCPR: true);
                break;
            case GamePhase.MainMenu:
                gameManager.SetLevel(null, fullCPR: false);
                break;
        }
    }
}
