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

    public bool bStartFromLevel = false;
    public int StartFromLevel = 0;
    public SpawnInstructions[] AllSpawnInstructions;
    public int SpawnInstructionsIndex = -1;

    public Transform TableGroup;
    public Transform TableHolder;
    public Vector3 spawnTableOffset;

    public AudioSource gameoversound;
    public HandManager LHand, RHand;
    void Start()
    {
        StartCoroutine(StartGameCoroutine());
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
            if (bStartFromLevel) SpawnInstructionsIndex = StartFromLevel;
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
        /* no need to, will recover
         * Player.Instance.HandManagerR.GetPhysicalBack();
        Player.Instance.HandManagerL.GetPhysicalBack();
        */
        ScoreManager.Instance.ResetScore();

        // restart
        SpawnInstructionsIndex = -1;
        NextSpawnInstructions();
    }
    public void PlayGameOverSound()
    {
        gameoversound.Play();

    }
    IEnumerator GameWon()
    {
        ScoreManager.Instance.GameWon();
        yield return new WaitForSecondsRealtime(5);
        Restart();
    }

    IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSecondsRealtime(2);

        CalibrateTablePos();

        while (!(LHand.State == HandState.Physical && RHand.State == HandState.Physical))
            yield return null;

        yield return new WaitForSecondsRealtime(1);

        NextSpawnInstructions();

        // spawn detached hands & Table in offset from players' head
        // wait for both hands to get attached
        // spawn first level
    }

    public void CalibrateTablePos()
    {
        Vector3 offset = Camera.main.transform.position + Camera.main.transform.forward * spawnTableOffset.z;
        offset.y = Camera.main.transform.position.y + spawnTableOffset.y;
        TableGroup.position = offset;
        TableGroup.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
    }

}
