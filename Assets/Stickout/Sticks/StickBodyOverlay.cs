using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickBodyOverlay : MonoBehaviour
{
    MeshRenderer mr;
    Vector3 startScale = new Vector3(1.001f, 0, 1.001f);
    Vector3 endScale = new Vector3(1.001f, .93f, 1.001f);
    private void Awake()
    {
        mr = GetComponentInChildren<MeshRenderer>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SetOverlay(float t, Color overlayColor)
    {
        mr.material.color = overlayColor;
        transform.localScale = Vector3.Lerp(startScale, endScale, t);
    }

    public void SetAlpha(float a)
    {
        Color c = mr.material.color;
        c.a = a;
        mr.material.color = c;
    }

}
