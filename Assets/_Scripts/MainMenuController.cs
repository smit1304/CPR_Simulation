using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject startMenuUI;

    private void Start()
    {
        Time.timeScale = 0f;

        if (startMenuUI != null)
            startMenuUI.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void StartGame()
    {
        if (startMenuUI != null)
            startMenuUI.SetActive(false);

        Time.timeScale = 1f;

        Cursor.visible = true;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}