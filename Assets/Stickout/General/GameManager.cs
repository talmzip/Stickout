using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singelton Decleration

    private static GameManager _instance;

    public static GameManager Instance { get { return _instance; } }


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

    public int StartFromLevel = 0;
    public SpawnInstructions[] AllSpawnInstructions;
    public int SpawnInstructionsIndex = -1;

    void Start()
    {
        NextSpawnInstructions();
    }


    public void NextSpawnInstructions()
    {
        // stop when completing all spawn instructions
        if (SpawnInstructionsIndex >= AllSpawnInstructions.Length - 1)
        {
            StartCoroutine(GameWon());
            return;
        }

        SpawnInstructionsIndex++;

        // Reset score after first level (the single stick level)
        if (SpawnInstructionsIndex == 1)
        {
            ScoreManager.Instance.ReCount();

            // for testing
            if (StartFromLevel != 0) SpawnInstructionsIndex = StartFromLevel;
        }
        

        SticksManager.Instance.SetNewSpawnInstructions(AllSpawnInstructions[SpawnInstructionsIndex]);
    }

    public void AllSticksGotPicked()
    {
        NextSpawnInstructions();
    }

    // called from SticksManager after a stick collapsed event.
    public void Restart()
    {
        // get hands back to normal state
        Player.Instance.HandManagerR.GetPhysicalBack();
        Player.Instance.HandManagerL.GetPhysicalBack();

        ScoreManager.Instance.ResetScore();

        // restart
        SpawnInstructionsIndex = -1;
        NextSpawnInstructions();
    }

    IEnumerator GameWon()
    {
        ScoreManager.Instance.GameWon();
        yield return new WaitForSecondsRealtime(5);
        Restart();
    }
}
