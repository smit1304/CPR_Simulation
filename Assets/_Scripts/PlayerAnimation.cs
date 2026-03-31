using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private GameManagerUpdated gameManager;
    private bool isSubscribed = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        gameManager = FindFirstObjectByType<GameManagerUpdated>();

        // Only subscribe once
        if (gameManager != null && !isSubscribed)
        {
            gameManager.OnCPRExecute += HandleCPRExecute;
            isSubscribed = true;
            Debug.Log("[PlayerAnimation] Subscribed to OnCPRExecute");
        }
    }

    private void OnDestroy()
    {
        if (gameManager != null && isSubscribed)
        {
            gameManager.OnCPRExecute -= HandleCPRExecute;
            isSubscribed = false;
        }
    }

    private void HandleCPRExecute((CPRAction, bool) tuple)
    {
        Debug.Log($"PlayerAnimation received CPR action: {tuple.Item1}, isMistake: {tuple.Item2}");
        
        if (tuple.Item1 == CPRAction.compression)
        {
            animator.SetTrigger("Compression");
            Debug.Log("[PlayerAnimation] Triggered Compression animation");
        }
        else if (tuple.Item1 == CPRAction.breath)
        {
            animator.SetTrigger("Breath");
            Debug.Log("[PlayerAnimation] Triggered Breath animation");
        }
    }
}
