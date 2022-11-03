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

    public SpawnInstructions currentSpawnInstructions;
    public int spawnInstructionsIndex = -1;
    
    public List<Stick> SpawnedSticks;         // the stick ref and the spawn point index its spawned at.
    
    public GameObject NormalStickPrefab;
    public GameObject OcclusionStickPrefab;
    public GameObject JumperStickPrefab;

    public SpawnPoint[] SpawnPoints;         // all spawn points.
    public SpawnPoint[] AllButSingle;
    public SpawnPoint[] AllButTwin;

    int totalSticksToBeSpawned;               // how many sticks should be spawned in those instructions?
    int sticksSpawnedTotal;
    float timeFromLastSpawn = 0;

    bool shouldSpawn = false;

    void Start()
    {
        SpawnedSticks = new List<Stick>();
        SpawnPoints = GetComponentsInChildren<SpawnPoint>();

    }

    void ClearSticksList()
    {
        foreach (Stick s in SpawnedSticks)
            Destroy(s.gameObject);
        SpawnedSticks.Clear();
    }

    public void SetNewSpawnInstructions(SpawnInstructions inst)
    {
        currentSpawnInstructions = inst;

        ClearSticksList();
        sticksSpawnedTotal = 0;

        totalSticksToBeSpawned = currentSpawnInstructions.NormalSticksAmount + currentSpawnInstructions.OcclusionSticksAmount + currentSpawnInstructions.JumperSticksAmount;
        timeFromLastSpawn = 0;
        SetupSpawnGroup(inst.SpawnGroup);

        StartCoroutine(waitBeforeNextSpawnInstructions());
    }

    private void SetupSpawnGroup(SpawnGroup sg)
    {
        foreach (SpawnPoint sp in SpawnPoints) sp.IsTaken = false;
        switch (sg)
        {
            case SpawnGroup.All:
                return;
            case SpawnGroup.Single:
                foreach (SpawnPoint sp in AllButSingle) sp.IsTaken = true;
                return;
            case SpawnGroup.Twin:
                foreach (SpawnPoint sp in AllButTwin) sp.IsTaken = true;
                return;
        }
    }

    IEnumerator waitBeforeNextSpawnInstructions()
    {
        yield return new WaitForSecondsRealtime(currentSpawnInstructions.WaitBeforeStarting);
        shouldSpawn = true;
    }


    void Update()
    {
        if (!shouldSpawn) return;

        // should we spawn more sticks?
        if (sticksSpawnedTotal < totalSticksToBeSpawned)
        {
            // can spawn more sticks now?
            if (SpawnedSticks.Count < currentSpawnInstructions.MaxConcurrentSticks)
            {
                timeFromLastSpawn += Time.deltaTime;

                // is it time to spawn?
                if (timeFromLastSpawn >= currentSpawnInstructions.SpawnFrequency)
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

        GameManager.Instance.AllSticksGotPicked();
    }

    #region Stick Spawning
    public void SpawnStick()
    {
        // get stick type to spawn
        StickType newStickType = GetStickTypeToSpawn();

        int spawnIndex = GetEmptySpawnPointIndex(newStickType);
        if (spawnIndex == -1)
        {
            Debug.LogWarning("No more empty spawn points. Stick won't be summoned");
            return;
        }

        // spawn stick
        Stick newStick = Instantiate(GetStickPrefab(newStickType), SpawnPoints[spawnIndex].Position, Quaternion.Euler(-90, 0, 0), transform).GetComponent<Stick>();

        newStick.LifeTime = currentSpawnInstructions.SticksLifeTime;

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

        int normalSticksLeft = currentSpawnInstructions.NormalSticksAmount - GetStickAmountByType(StickType.Normal);
        while (i < normalSticksLeft)
        {
            remainingTypes[i] = StickType.Normal;
            i++;
        }

        int occlusionSticksLeft = currentSpawnInstructions.OcclusionSticksAmount - GetStickAmountByType(StickType.Occlusion);
        while (i < normalSticksLeft + occlusionSticksLeft)
        {
            remainingTypes[i] = StickType.Occlusion;
            i++;
        }

        int jumperSticksLeft = currentSpawnInstructions.JumperSticksAmount - GetStickAmountByType(StickType.Jumper);
        while (i < normalSticksLeft + occlusionSticksLeft + jumperSticksLeft)
        {
            remainingTypes[i] = StickType.Jumper;
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
                return NormalStickPrefab;
            case StickType.Occlusion:
                return OcclusionStickPrefab;
            case StickType.Jumper:
                return JumperStickPrefab;
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
            if (IsSpawnPointAvailable(SpawnPoints[i], spawnPointType))
                emptyPoints.Add(i);

        if (emptyPoints.Count == 0)
            return -1;

        // choose a random one
        int spawnIndex = Random.Range(0, emptyPoints.Count);

        return emptyPoints[spawnIndex];
    }

    // gets a spawn point and a desired type and returns true if its currently availabe.
    private bool IsSpawnPointAvailable(SpawnPoint sp, StickType type)
    {
        // if it matches the type and is not taken
        if (sp.Type == type && !sp.IsTaken)
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
        ScoreManager.Instance.AddScore();
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
        GameManager.Instance.PlayGameOverSound();

        // stop all sticks
        foreach (Stick s in SpawnedSticks) s.StopAllCoroutines(); // TODO: don't stop the pickup coroutine

        yield return new WaitForSecondsRealtime(2);
        // fold all sticks
        foreach (Stick s in SpawnedSticks) s.Fold();

        // wait until them all folds
        if (SpawnedSticks.Count > 0)
            yield return new WaitForSecondsRealtime(SpawnedSticks[0].CompleteFoldDuration + 1);

        GameManager.Instance.Restart();
    }
}
