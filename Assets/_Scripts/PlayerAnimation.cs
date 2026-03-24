using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager != null)
        {
            gameManager.OnCPRExecute += HandleCPRExecute;
        }
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnCPRExecute -= HandleCPRExecute;
        }
    }

    private void HandleCPRExecute(CPRAction action)
    {
        if (action == CPRAction.compression)
        {
            animator.SetTrigger("Compression");
        }
        else if (action == CPRAction.breath)
        {
            animator.SetTrigger("Breath");
        }
    }
}
