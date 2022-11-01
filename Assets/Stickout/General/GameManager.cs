using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public SpawnInstructions[] AllSpawnInstructions;
    public int InstructionsIndex = -1;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void NextSpawnInstructions()
    {
        if (InstructionsIndex >= AllSpawnInstructions.Length - 1) return;

        if (InstructionsIndex == 0) ScoreManager.Instance.ResetScore();

        InstructionsIndex++;

       /* ClearSticksList();
        sticksSpawnedOcclusion = 0;
        sticksSpawnedTotal = 0;
        SticksSpawnedNormal = 0;

        totalSticksToBeSpawned = spawnInstructions[spawnInstructionsIndex].NormalSticksAmount + spawnInstructions[spawnInstructionsIndex].OcclusionSticksAmount;
        timeFromLastSpawn = 0;

        StartCoroutine(waitBeforeNextSpawnInstructions());*/
    }
}
