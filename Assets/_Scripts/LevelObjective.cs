using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelObjective", menuName = "CPR Simulation/Level Objective")]
public class LevelObjective : ScriptableObject
{
    [SerializeField] private CPRSecuence[] secuence;
    [SerializeField] private int repetition = 1;

    public CPRSecuence[] Secuence => secuence;
    public int Repetition => repetition;
}
