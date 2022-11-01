using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostHand : MonoBehaviour
{
    private SkinnedMeshRenderer mr;

    private void Awake()
    {
        mr = GetComponent<SkinnedMeshRenderer>();
    }

    public void ChangeColor(Color color)
    {
        mr.material.color = color;
    }
}
