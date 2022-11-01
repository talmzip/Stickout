using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public StickType Type;
    public bool IsTaken;        // true when a stick is summoned on it
    public Stick Stick;
    public Vector3 Position => transform.position;

    public bool isOcclusionLeft;
    void Start()
    {
        foreach (Transform child in transform) child.gameObject.SetActive(false);
    }

    void Update()
    {
        
    }

    public void StickSpawned(Stick s)
    {
        IsTaken = true;
        Stick = s;
    }

    public void StickRemoved()
    {
        IsTaken = false;
        Stick = null;
    }
}
