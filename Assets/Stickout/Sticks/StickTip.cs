using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickTip : MonoBehaviour
{

    public Transform tipPosition;
    void Start()
    {
        
    }

    void Update()
    {
        transform.position = tipPosition.position;
    }
}
