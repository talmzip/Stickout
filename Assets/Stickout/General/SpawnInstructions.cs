using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnInstructions", menuName = "ScriptableObjects/SpawnInstructions", order = 1)]
public class SpawnInstructions : ScriptableObject
{
    public float WaitBeforeStarting;

    public int NormalSticksAmount;
    public int OcclusionSticksAmount;

    public float SpawnFrequency;
    public int MaxConcurrentSticks;
    public int SticksLifeTime = 15;
}
