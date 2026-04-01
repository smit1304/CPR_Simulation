using System;
using UnityEngine;

public enum CPRAction
{
    none,
    compression,
    breath,
}


public class GameManagerUpdated : MonoBehaviour
{
    [SerializeField] private LevelObjective currentLevel;

    [Header("Mistake Settings")]
    [SerializeField] private int tutorialMistakeLimit = 3;
    [SerializeField] private int fullCPRMistakeLimit = 5;

    private const float BREATH_TIME = 2f;
    private const float COMPRESSION_TIME = 0.5f;
    private const float TIMEOUT_MARGIN = 1.0f;
    private const float TIMING_MARGIN = 0.2f;

    private int breathCount = 0;
    private int compressionCount = 0;
    private int currentSequenceIndex = 0;
    private int currentRepetition = 0;
    private float time = 0f;
    private float expectedTimeLimit = 0f;
    private int mistakeCount = 0;
    private bool isFullCPR = false;

    private CPRSecuence currentTarget;

    // Public read access for UI
    public int CompressionCount => compressionCount;
    public int BreathCount => breathCount;
    public int TargetCompressionCount => currentTarget?.compressionCount ?? 0;
    public int TargetBreathCount => currentTarget?.breathCount ?? 0;
    public int MistakeCount => mistakeCount;
    public int CurrentRepetition => currentRepetition;
    public int TotalRepetitions => currentLevel?.Repetition ?? 0;

    public event Action<(CPRAction, bool)> OnCPRExecute;
    public event Action OnLevelCompleted;
    public event Action OnTutorialFailed;
    public event Action OnGameOver;

    // Fires every time compressions are done and breaths should begin
    public event Action OnBreathPhaseStarted;

    // Fires every time a cycle completes so UI can show cycle count
    public event Action<int, int> OnCycleCompleted; // current, total

    private float lastActionTime = 0f;

    private void Start()
    {
        ResetSimulation();
    }

    public void SetLevel(LevelObjective newLevel, bool fullCPR = false)
    {
        currentLevel = newLevel;
        isFullCPR = fullCPR;
        ResetSimulation();
        Debug.Log($"[GameManager] Level set to: {(newLevel != null ? newLevel.name : "null")}, isFullCPR: {fullCPR}");
    }

    public void ResetSimulation()
    {
        breathCount = 0;
        compressionCount = 0;
        currentSequenceIndex = 0;
        currentRepetition = 0;
        time = 0f;
        mistakeCount = 0;
        lastActionTime = Time.time;
        UpdateCurrentTarget();
        UpdateExpectedTimeLimit();
    }

    private void Update()
    {
        if (currentLevel == null || currentTarget == null) return;

        time += Time.deltaTime;

        if (Time.time - lastActionTime > expectedTimeLimit + TIMEOUT_MARGIN)
        {
            Debug.Log("[GameManager] Timeout — player took too long.");
            lastActionTime = Time.time;
            RegisterMistake(CPRAction.none);
            //ResetSimulation();
        }
    }

    public void Compression()
    {
        if (currentLevel == null || currentTarget == null) return;
        compressionCount++;
        EvaluateAction(CPRAction.compression);
    }

    public void Breath()
    {
        if (currentLevel == null || currentTarget == null) return;
        breathCount++;
        EvaluateAction(CPRAction.breath);
    }

    private void EvaluateAction(CPRAction phase)
    {
        float timeSinceLastAction = Time.time - lastActionTime;
        lastActionTime = Time.time;

        if (currentLevel == null || currentTarget == null) return;

        // --- Determine what is expected right now ---
        bool stillNeedsCompressions = compressionCount < currentTarget.compressionCount;
        bool stillNeedsBreaths = !stillNeedsCompressions && breathCount < currentTarget.breathCount;

        bool makeMistake = false;

        if (stillNeedsCompressions)
        {
            bool tooFast = timeSinceLastAction < COMPRESSION_TIME - TIMING_MARGIN;
            if (tooFast)
            {
                makeMistake = true;
                Debug.Log($"[GameManager] Mistake: compression too fast ({timeSinceLastAction:F2}s).");
            }else if (phase != CPRAction.compression)
            {
                makeMistake = true;
                Debug.Log("[GameManager] Mistake: breath when compression expected.");
            }
        }
        else if (stillNeedsBreaths)
        {
            bool tooFast = timeSinceLastAction < COMPRESSION_TIME - TIMING_MARGIN;
            if (tooFast)
            {
                makeMistake = true;
                Debug.Log($"[GameManager] Mistake: compression too fast ({timeSinceLastAction:F2}s).");
            }else if (phase != CPRAction.breath)
            {
                makeMistake = true;
                Debug.Log("[GameManager] Mistake: compression when breath expected.");
            }
        }

        if (makeMistake)
        {
            RegisterMistake(phase);
            return;
        }

        Debug.Log($"[GameManager] Correct -> phase: {phase}, C: {compressionCount}/{currentTarget.compressionCount}, B: {breathCount}/{currentTarget.breathCount}");
        OnCPRExecute?.Invoke((phase, false));

        // --- Check if compressions just finished, breaths should start ---
        bool compressionsJustFinished = compressionCount >= currentTarget.compressionCount
                                        && breathCount == 0
                                        && currentTarget.breathCount > 0;

        if (compressionsJustFinished && phase == CPRAction.compression)
        {
            Debug.Log("[GameManager] Compressions complete — breath phase starting.");
            OnBreathPhaseStarted?.Invoke();
        }

        // --- Check if the full sequence step is complete ---
        bool stepComplete = compressionCount >= currentTarget.compressionCount
                            && breathCount >= currentTarget.breathCount;

        if (stepComplete)
        {
            currentSequenceIndex++;

            // Check if we've finished all steps in the sequence
            if (currentSequenceIndex >= currentLevel.Secuence.Length)
            {
                // One full repetition done — check if all repetitions are complete
                currentSequenceIndex = 0;
                currentRepetition++;

                Debug.Log($"[GameManager] Cycle {currentRepetition}/{currentLevel.Repetition} complete.");
                OnCycleCompleted?.Invoke(currentRepetition, currentLevel.Repetition);

                if (currentRepetition >= currentLevel.Repetition)
                {
                    Debug.Log("[GameManager] All cycles complete — Level completed!");
                    OnLevelCompleted?.Invoke();
                    ResetSimulation();
                    return;
                }
            }

            // Reset counts for next step or next repetition
            breathCount = 0;
            compressionCount = 0;
        }

        UpdateCurrentTarget();
        UpdateExpectedTimeLimit();
    }

    private void RegisterMistake(CPRAction phase)
    {
        mistakeCount++;
        Debug.Log($"[GameManager] Mistake #{mistakeCount}");

        OnCPRExecute?.Invoke((phase, true));

        int limit = isFullCPR ? fullCPRMistakeLimit : tutorialMistakeLimit;

        if (mistakeCount >= limit)
        {
            if (isFullCPR)
            {
                Debug.Log("[GameManager] Game Over.");
                OnGameOver?.Invoke();
            }
            else
            {
                Debug.Log("[GameManager] Tutorial failed — resetting.");
                OnTutorialFailed?.Invoke();
            }

            ResetSimulation();
        }
    }

    private void UpdateCurrentTarget()
    {
        if (currentLevel != null
            && currentLevel.Secuence.Length > 0
            && currentSequenceIndex < currentLevel.Secuence.Length)
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
            expectedTimeLimit = (compressionCount < currentTarget.compressionCount)
                ? COMPRESSION_TIME
                : BREATH_TIME;
        }
    }
}

[Serializable]
public class CPRSecuence
{
    public int breathCount = 0;
    public int compressionCount = 0;
}
