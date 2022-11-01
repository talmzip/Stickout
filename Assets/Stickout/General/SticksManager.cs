using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1. Spawns sticks according to SpawnInstructions ScriptableObject
/// 2. Tracks current sticks state and count.
/// 3. Handle mass stick control events (i.e collapse)
/// </summary>
public class SticksManager : MonoBehaviour
{

    #region Singelton Decleration

    private static SticksManager _instance;

    public static SticksManager Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    #endregion

    public SpawnInstructions[] spawnInstructions;
    public int spawnInstructionsIndex = -1;
    public GameObject NormalStickPrefab;
    public GameObject OcclusionStickPrefab;

    public SpawnPoint[] SpawnPoints;         // all spawn points.
    public List<Stick> SpawnedSticks;         // the stick ref and the spawn point index its spawned at.

    float timeFromLastSpawn = 0;
    int totalSticksToBeSpawned;               // how many sticks should be spawned in those instructions?
    
    int sticksSpawnedTotal;
    int SticksSpawnedNormal;
    int sticksSpawnedOcclusion;

    bool shouldSpawn = false;

    void Start()
    {
        SpawnedSticks = new List<Stick>();
        SpawnPoints = GetComponentsInChildren<SpawnPoint>();

        NextSpawnInstructions();
    }

    void ClearSticksList()
    {
        foreach (Stick s in SpawnedSticks)
            Destroy(s.gameObject);
        SpawnedSticks.Clear();
    }

    public void NextSpawnInstructions()
    {
        if (spawnInstructionsIndex >= spawnInstructions.Length - 1) return;
        if (spawnInstructionsIndex == 0) ScoreManager.Instance.ResetScore();

        spawnInstructionsIndex++;
        
        ClearSticksList();
        sticksSpawnedOcclusion = 0;
        sticksSpawnedTotal = 0;
        SticksSpawnedNormal = 0;
        
        totalSticksToBeSpawned = spawnInstructions[spawnInstructionsIndex].NormalSticksAmount + spawnInstructions[spawnInstructionsIndex].OcclusionSticksAmount;
        timeFromLastSpawn = 0;

        StartCoroutine(waitBeforeNextSpawnInstructions());
    }

    public void SetNewSpawnInstructions(SpawnInstructions inst)
    {

    }

    IEnumerator waitBeforeNextSpawnInstructions()
    {
        yield return new WaitForSecondsRealtime(spawnInstructions[spawnInstructionsIndex].WaitBeforeStarting);
        shouldSpawn = true;
    }


    void Update()
    {
        if (!shouldSpawn) return;

        // should we spawn more sticks?
        if (sticksSpawnedTotal < totalSticksToBeSpawned)
        {
            // can spawn more sticks now?
            if (SpawnedSticks.Count < spawnInstructions[spawnInstructionsIndex].MaxConcurrentSticks)
            {
                timeFromLastSpawn += Time.deltaTime;

                // is it time to spawn?
                if (timeFromLastSpawn >= spawnInstructions[spawnInstructionsIndex].SpawnFrequency)
                {
                    timeFromLastSpawn = 0;
                    SpawnStick();
                }
            }
        }
        else
        {
            // all sticks were spawned. wait for them to get picked up
            shouldSpawn = false;
            StartCoroutine(WaitForAllSticksToGetPicked());
        }

    }

    IEnumerator WaitForAllSticksToGetPicked()
    {
        while (SpawnedSticks.Count > 0)
            yield return null;
        NextSpawnInstructions();
    }

    #region Stick Spawning
    public void SpawnStick()
    {
        // get stick type to spawn
        StickType newStickType = GetStickTypeToSpawn();

        int spawnIndex = GetEmptySpawnPointIndex(newStickType);
        if (spawnIndex == -1)
        {
            Debug.Log("No more empty spawn points. Stick won't be summoned");
            return;
        }

        // spawn stick
        Stick newStick = Instantiate(GetStickPrefab(newStickType), SpawnPoints[spawnIndex].Position, Quaternion.Euler(-90,0,0), transform).GetComponent<Stick>();
        
        newStick.LifeTime = spawnInstructions[spawnInstructionsIndex].SticksLifeTime;
        
        // update spawn point data
        SpawnPoints[spawnIndex].StickSpawned(newStick);

        newStick.SpawnIndex = spawnIndex;
        
        // for occlusion sticks. Its ! because left points need to have right sticks (the crossing makes the trackloss)
        newStick.IsOcclusionLeft = !SpawnPoints[spawnIndex].isOcclusionLeft;

        // add to list
        SpawnedSticks.Add(newStick);

        // update totals
        sticksSpawnedTotal++;
    }

    // returns a random stick type according to their amounts in the SpawnInstructions
    private StickType GetStickTypeToSpawn()
    {
        // builds an array full of types by their current amount to be spawned and then randomlly choose one.

        StickType[] remainingTypes = new StickType[totalSticksToBeSpawned - SpawnedSticks.Count];
        int i = 0;

        int normalSticksLeft = spawnInstructions[spawnInstructionsIndex].NormalSticksAmount - GetStickAmountByType(StickType.Normal);
        while(i<normalSticksLeft)
        {
            remainingTypes[i] = StickType.Normal;
            i++;
        }

        int occlusionSticksLeft = spawnInstructions[spawnInstructionsIndex].OcclusionSticksAmount - GetStickAmountByType(StickType.Occlusion);
        while(i< normalSticksLeft + occlusionSticksLeft)
        {
            remainingTypes[i] = StickType.Occlusion;
            i++;
        }

        // this if should never be true
        if (remainingTypes.Length == 0)
        {
            Debug.LogWarning("Tried to spawn a stick despite totalSticksSpawned > totalStickAmount");
            return StickType.Normal;
        }

        return remainingTypes[Random.Range(0, remainingTypes.Length)];
    }

    int GetStickAmountByType(StickType type)
    {
        int sum = 0;
        foreach (Stick s in SpawnedSticks)
        {
            if (s.Type == type) sum++;
        }

        return sum;
    }
    GameObject GetStickPrefab(StickType type)
    {
        switch (type)
        {
            case StickType.Normal:
                SticksSpawnedNormal++;
                return NormalStickPrefab;
            case StickType.Occlusion:
                sticksSpawnedOcclusion++;
                return OcclusionStickPrefab;
            default:
                return NormalStickPrefab;
        }
    }

    // return an index of an empty spawn point of a selected type
    int GetEmptySpawnPointIndex(StickType spawnPointType)
    {
        // save all empty indexes
        List<int> emptyPoints = new List<int>();
        for (int i = 0; i < SpawnPoints.Length; i++)
            if (IsSpawnPointAvailable(SpawnPoints[i],spawnPointType))
                emptyPoints.Add(i);

        if (emptyPoints.Count == 0) return -1;

        // choose a random one
        int spawnIndex = Random.Range(0, emptyPoints.Count);
 
        return emptyPoints[spawnIndex];
    }

    // gets a spawn point and a desired type and returns true if its currently availabe.
    private bool IsSpawnPointAvailable(SpawnPoint sp ,StickType type)
    {
        // if it matches the type and is not taken
        if(sp.Type == type && !sp.IsTaken)
        {
            // if its occlusion make sure the picking hand is not ghost. if it is- return false so that a left occlustion point won't be summoned when player's left hand is ghost.
            if (type == StickType.Occlusion)
            {
                if (!sp.isOcclusionLeft && !Player.Instance.ppL.IsGhost)
                    return true;
                if (sp.isOcclusionLeft && !Player.Instance.ppR.IsGhost)
                    return true;
                return false;
            }
            else
                return true;
        }
        return false;
    }
    #endregion
    public void RemoveStick(Stick removedStick)
    {
        // get removed stick spawn index
        int removedIndex = removedStick.SpawnIndex;

        // update free position
        SpawnPoints[removedIndex].StickRemoved();

        // remove from list
        SpawnedSticks.Remove(removedStick);

        Destroy(removedStick.gameObject);
    }

    public void StickPickedUp(Stick pickedStick, bool isBonus)
    {
        ScoreManager.Instance.AddScore(pickedStick.Type,isBonus);
        RemoveStick(pickedStick);
    }

    public void StickCollapsed(Stick collapsedStick)
    {
        //stop spawning
        shouldSpawn = false;
        RemoveStick(collapsedStick);

        StopAllCoroutines();
        StartCoroutine(EndGame());
    }

    IEnumerator EndGame()
    {
        // stop all sticks
        foreach (Stick s in SpawnedSticks) s.StopAllCoroutines();

        yield return new WaitForSecondsRealtime(2);
        // fold all sticks
        foreach (Stick s in SpawnedSticks) s.Fold();

        // wait until them all folds
        if (SpawnedSticks.Count > 0) 
            yield return new WaitForSecondsRealtime(SpawnedSticks[0].CompleteFoldDuration + 5);

        // get hands back to normal state
        Player.Instance.HandManagerR.GetPhysicalBack();
        Player.Instance.HandManagerL.GetPhysicalBack();

        // restart
        spawnInstructionsIndex = -1;
        NextSpawnInstructions();
    }
}
