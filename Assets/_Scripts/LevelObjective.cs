using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelObjective", menuName = "CPR Simulation/Level Objective")]
public class LevelObjective : ScriptableObject
{
    [SerializeField] private CPRSecuence[] secuence;
    [SerializeField] private int repetition = 1;
    [SerializeField] private float waitTimeBetweenCycles = 1.0f;

    public CPRSecuence[] Secuence => secuence;
    public int Repetition => repetition;
    public float waitTime => waitTimeBetweenCycles;
}
