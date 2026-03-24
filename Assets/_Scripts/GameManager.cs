using System;
using UnityEngine;

public enum CPRAction
{
    none,
    compression,
    breath,
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private LevelObjective currentLevel;

    private const float BREATH_TIME = 2f; // Time in seconds for a breath action
    private const float COMPRESSION_TIME = 0.5f; // Time in seconds for a compression action
    private const float TIMEOUT_MARGIN = 1.0f; // Maximum extra time allowed before triggering an error

    private int breathCount = 0;
    private int compressionCount = 0;
    private int currentRepetition = 0;
    private int currentSequenceIndex = 0;
    private float time = 0f;
    private float expectedTimeLimit = 0f;
    private CPRSecuence currentTarget;
    
    public event Action<(CPRAction, bool)> OnCPRExecute;
    public event Action OnLevelCompleted;
    // Timer for evaluating CPR pace (e.g. 100-120 compressions per minute)
    private float lastActionTime = 0f;

    private void Start()
    {
        ResetSimulation();
    }

    public void ResetSimulation()
    {
        breathCount = 0;
        compressionCount = 0;
        currentRepetition = 0;
        currentSequenceIndex = 0;
        time = 0f;
        lastActionTime = Time.time;
        UpdateCurrentTarget();
        UpdateExpectedTimeLimit();
    }

    private void Update()
    {
        if (currentLevel == null || currentTarget == null) return;

        time += Time.deltaTime;

        // Check if the player is taking too long to perform the next action
        if (Time.time - lastActionTime > expectedTimeLimit + TIMEOUT_MARGIN)
        {
            // Player took too long, send an error
            OnCPRExecute?.Invoke((CPRAction.none, true));
            Debug.Log("Player took too long to perform the next action. Triggering error.");
            // Reset the timer so we don't spam the error event every frame
            lastActionTime = Time.time; 
        }
    }

    public void Compression()
    {
        if (currentLevel == null) return;
        
        compressionCount++;
        EvaluateAction(CPRAction.compression);
    }

    public void Breath()
    {
        if (currentLevel == null) return;

        breathCount++;
        EvaluateAction(CPRAction.breath);
    }

    public void EvaluateAction(CPRAction phase)
    {
        float timeSinceLastAction = Time.time - lastActionTime;
        lastActionTime = Time.time;
        Debug.Log($"Evaluating action: {phase}, Time since last action: {timeSinceLastAction:F2} seconds");

        if (currentLevel != null && currentTarget != null)
        {
            bool makeMistake = false;
            
            if(currentTarget.compressionCount >= compressionCount && currentTarget.breathCount >= breathCount)
            {
                currentSequenceIndex++;
                if (currentSequenceIndex >= currentLevel.Secuence.Length)
                {
                    OnLevelCompleted?.Invoke();
                    ResetSimulation();
                    return; // ResetSimulation already updates target and time limit
                }
                else {
                    breathCount = 0;
                    compressionCount = 0;
                    currentRepetition++;
                }
            }
            else if (currentTarget.compressionCount < compressionCount)
            {
                compressionCount++;

                if (phase != CPRAction.compression && timeSinceLastAction < COMPRESSION_TIME)
                {
                    makeMistake = Mathf.Abs(timeSinceLastAction - COMPRESSION_TIME) > 0.2f; // Allow a small margin of error
                }
                else
                    makeMistake = true;

            }
            else if (currentTarget.breathCount < breathCount)
            {
                breathCount++;

                if (phase != CPRAction.compression && timeSinceLastAction < COMPRESSION_TIME)
                {
                    makeMistake = Mathf.Abs(timeSinceLastAction - COMPRESSION_TIME) > 0.2f; // Allow a small margin of error
                }
                else
                    makeMistake = true;

            }

            Debug.Log($"OnCPRExecute  {phase}");
            OnCPRExecute?.Invoke((phase, makeMistake));

            // Recalculate target and time limit based on the upcoming expected action
            UpdateCurrentTarget();
            UpdateExpectedTimeLimit();
        }
    }

    private void UpdateCurrentTarget()
    {
        if (currentLevel != null && currentLevel.Secuence.Length > 0 && currentSequenceIndex < currentLevel.Secuence.Length)
        {
            currentTarget = currentLevel.Secuence[currentSequenceIndex];
        }
        else
        {
            currentTarget = null;
        }
    }

    private void UpdateExpectedTimeLimit()
    {
        if (currentTarget != null)
        {
            expectedTimeLimit = (compressionCount < currentTarget.compressionCount) ? COMPRESSION_TIME : BREATH_TIME;
        }
    }
}

[Serializable]
public class CPRSecuence 
{
    public int breathCount = 0;
    public int compressionCount = 0;
}
