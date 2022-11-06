using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableSide : MonoBehaviour
{
    MeshRenderer mr;
    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
    }

    public void MarkDetached()
    {

    }

    IEnumerator markDetachedCo(Color detachedColor, float duration, AnimationCurve curve)
    {
        float lerpTime = 0;
        Color normalColor = mr.material.color;

        while (lerpTime < duration)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / duration;
            t = curve.Evaluate(t);

            mr.material.color = Color.Lerp(normalColor, detachedColor, t);

            yield return null;
        }
    }
}
