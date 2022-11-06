using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    public MeshRenderer mrR;
    public MeshRenderer mrL;

    public Color normalColor;
    public Color detachedColor;
    public AnimationCurve signalDetachCurve;
    public float SignalDetachedDuration;

    public bool bSignal;
    bool isLeftAttached;
    bool isRightAttached;

    IEnumerator rHandCo, lHandCo;
    void Start()
    {
        mrL.material.color = normalColor;
        mrR.material.color = normalColor;
    }

    void Update()
    {
        if(bSignal)
        {
            HandDetached(true);
            bSignal = false;
        }
    }

    public void HandDetached(bool isLeft)
    {
        if (isLeft) isLeftAttached = false;
        else isRightAttached = false;

        // signal both white
        StopAllCoroutines();
        StartCoroutine(signalDetach());
        // keep the related side half opacity
        MeshRenderer mr = isLeft ? mrL : mrR;

    }

    private void MarkDetachedWhite()
    {
        if (isRightAttached) mrR.material.color = detachedColor;
        if (isLeftAttached) mrL.material.color = detachedColor;
    }

    public void HandReAttached(bool isLeft)
    {
        if (isLeft) isLeftAttached = true;
        else isRightAttached = true;
        // fade back to black
        MeshRenderer mr = isLeft ? mrL : mrR;
        
    }

    IEnumerator signalDetach()
    {
        float lerpTime = 0;

        while (lerpTime < SignalDetachedDuration)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / SignalDetachedDuration;
            t = signalDetachCurve.Evaluate(t);

            mrL.material.color = Color.Lerp(normalColor, detachedColor, t);
            mrR.material.color = Color.Lerp(normalColor, detachedColor, t);

            yield return null;
        }
    }

    IEnumerator changeColor(MeshRenderer mr, float duration, Color targetColor, AnimationCurve curve)
    {
        Color startColor = mr.material.color;
        float lerpTime = 0;

        while(lerpTime<duration)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / duration;
            t = curve.Evaluate(t);

            mr.material.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }
    }
}
