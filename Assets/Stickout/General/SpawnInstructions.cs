using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnGroup
{
    All,
    Single,
    Twin,
}

[CreateAssetMenu(fileName = "SpawnInstructions", menuName = "ScriptableObjects/SpawnInstructions", order = 1)]
public class SpawnInstructions : ScriptableObject
{
    public float WaitBeforeStarting;
    public SpawnGroup SpawnGroup;

    public int NormalSticksAmount;
    public int OcclusionSticksAmount;
    public int JumperSticksAmount;

    public float SpawnFrequency;
    public int MaxConcurrentSticks;
    public int SticksLifeTime = 15;
}
