using System;
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
        gameManager = FindFirstObjectByType<GameManager>();

        gameManager.OnCPRExecute += HandleCPRExecute;
    }

    private void OnDestroy()
    {
        gameManager.OnCPRExecute -= HandleCPRExecute;
    }

    private void HandleCPRExecute((CPRAction, bool) tuple)
    {
        Debug.Log($"PlayerAnimation received CPR action: {tuple.Item1}");
        if (tuple.Item1 == CPRAction.compression)
        {
            animator.SetTrigger("Compression");
        }
        else if (tuple.Item1 == CPRAction.breath)
        {
            animator.SetTrigger("Breath");
        }
    }
}
