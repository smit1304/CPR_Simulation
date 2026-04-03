using System;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject startMenuUI;
    [SerializeField] private GameFlowManager gameFlowManager;

    private void Start()
    {
        Time.timeScale = 0f;

        if (startMenuUI != null)
            startMenuUI.SetActive(true);

        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        gameFlowManager.OnPhaseChanged += HandlePhaseChanged;
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        mainMenuUI.SetActive(phase == GamePhase.MainMenu);
    }

    public void StartGame()
    {
        if (startMenuUI != null)
            startMenuUI.SetActive(false);

        Time.timeScale = 1f;

        Cursor.visible = true;

        if (gameFlowManager != null)
            gameFlowManager.StartGame();
        else
            Debug.LogError("[MainMenuController] GameFlowManager not found. Can't start game.");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}

