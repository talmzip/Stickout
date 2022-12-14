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
    public bool IsAttached = false;

    public Vector3[] BonesPositions;
    public Quaternion[] BonesRotations;
    public bool doSaveBones = false;
    public bool doStartFromSavedTransforms;

    void Start()
    {
        mr = GetComponentInChildren<SkinnedMeshRenderer>();
        mr.material.color = DetachedColor;

        

        if(doStartFromSavedTransforms)
        {
            for (int i = 0; i < Bones.Length; i++)
            {
               Bones[i].position = BonesPositions[i];
               Bones[i].rotation= BonesRotations[i];
            }
            Bones[0].localPosition = Vector3.zero;
            Bones[0].localRotation= Quaternion.identity;
        }
    }

    void Update()
    {
        if (doSaveBones)
        {
            BonesPositions = new Vector3[Bones.Length];
            BonesRotations = new Quaternion[Bones.Length];

            doSaveBones = false;
            for (int i = 0; i < Bones.Length; i++)
            {
                BonesPositions[i] = Bones[i].position;
                BonesRotations[i] = Bones[i].rotation;
            }
        }
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
        StopAllCoroutines();
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
        IsAttached = false;
    }

    IEnumerator reattachCoroutine()
    {
        handManager.State = HandState.Physical;

        Vector3[] bonesStartPositions = new Vector3[Bones.Length];
        Quaternion[] bonesStartRotations = new Quaternion[Bones.Length];

        for (int i = 0; i < Bones.Length; i++)
        {
            bonesStartPositions[i] = Bones[i].position;
            bonesStartRotations[i] = Bones[i].rotation;
        }

        float lerpTime = 0;
        while (lerpTime < .5f)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / .5f;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            for (int i = 0; i < Bones.Length; i++)
            {
                Bones[i].position = Vector3.Lerp(bonesStartPositions[i], skeleton.Bones[i].Transform.position,t);
                Bones[i].rotation = Quaternion.Lerp(bonesStartRotations[i], skeleton.Bones[i].Transform.rotation,t);
            }
           
            mr.material.color = (Color.Lerp(DetachedColor, TrackingColor, t));

            yield return null;
        }

        IsAttached = true;
    }
}
