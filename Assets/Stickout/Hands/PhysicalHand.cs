using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalHand : MonoBehaviour
{
    private OVRSkeleton skeleton;
    private HandManager handManager;

    public Transform[] Bones;
    public Color TrackingColor, DetachedColor;

    public float ToDetachedDuration = .5f;
    public AnimationCurve ToDetachedCurve;

    private SkinnedMeshRenderer mr;
    public bool IsAttached = true;
    void Start()
    {
        mr = GetComponentInChildren<SkinnedMeshRenderer>();
        mr.material.color = TrackingColor;
    }

    void Update()
    {

    }

    public void Init(HandManager manager, OVRSkeleton ovrSkeleton)
    {
        handManager = manager;
        skeleton = ovrSkeleton;
    }

    public void TrackHandMovements()
    {
        for (int i = 0; i < Bones.Length; i++)
        {
            Bones[i].position = skeleton.Bones[i].Transform.position;
            Bones[i].rotation = skeleton.Bones[i].Transform.rotation;
        }
    }

    public void Detach()
    {
        StartCoroutine(detachCoroutine());
    }

    public void ReAttach()
    {
        IsAttached = false;
        StartCoroutine(reattachCoroutine());
    }

    IEnumerator detachCoroutine()
    {
        float lerpTime = 0;
        while (lerpTime < ToDetachedDuration)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / ToDetachedDuration;
            t = ToDetachedCurve.Evaluate(t);

            mr.material.color = (Color.Lerp(TrackingColor, DetachedColor, t));

            yield return null;
        }
    }

    IEnumerator reattachCoroutine()
    {
        Vector3[] bonesStartPositions = new Vector3[Bones.Length];
        Quaternion[] bonesStartRotations = new Quaternion[Bones.Length];

        for (int i = 0; i < Bones.Length; i++)
        {
            bonesStartPositions[i] = Bones[i].position;
            bonesStartRotations[i] = Bones[i].rotation;
        }

        float lerpTime = 0;
        while (lerpTime < 1f)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / 1f;
            t = ToDetachedCurve.Evaluate(t);

            for (int i = 0; i < Bones.Length; i++)
            {
                Bones[i].position = Vector3.Lerp(bonesStartPositions[i], skeleton.Bones[i].Transform.position,t);
                Bones[i].rotation = Quaternion.Lerp(bonesStartRotations[i], skeleton.Bones[i].Transform.rotation,t);
            }
           
            mr.material.color = (Color.Lerp(DetachedColor, TrackingColor, t));

            yield return null;
        }

        handManager.State = HandState.Physical;
        IsAttached = true;
    }
}
