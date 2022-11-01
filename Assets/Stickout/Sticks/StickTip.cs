using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickTip : MonoBehaviour
{
    MeshRenderer mr;

    public Transform tipPosition;

    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        mr.enabled = false;
    }

    void LateUpdate()
    {
        transform.position = tipPosition.position;
    }

    public void SetAppearence(bool state) => mr.enabled = state;
}
