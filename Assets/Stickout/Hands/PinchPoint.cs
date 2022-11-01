using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PinchPoint : MonoBehaviour
{
    public bool IsLeft;
    public bool IsPinching;
    public bool IsGhost = false;
    int thumbTipIndex = 19;
    int indexTipIndex = 8;

    OVRSkeleton ovrSkeleton;
    OVRHand ovrHand;

    public UnityAction<PinchPoint> HandPinchEnter;
    public UnityAction<PinchPoint> HandPinchExit;

    public float pinchThreshold;
    Vector3 indexTipPosition;
    Vector3 thumbTipPosition;

    void Start()
    {
        ovrSkeleton = IsLeft ? Player.Instance.handSkeletonL : Player.Instance.handSkeletonR;
        ovrHand = ovrSkeleton.transform.GetComponent<OVRHand>();

    }

    void Update()
    {
        if (ovrSkeleton.IsDataHighConfidence)
        {
            UpdatePosition();
            DetectPinching();
        }
        else
            transform.position = new Vector3(0, 0, -10000);


    }

    void UpdatePosition()
    {
        transform.position = Vector3.Lerp(ovrSkeleton.Bones[thumbTipIndex].Transform.position, ovrSkeleton.Bones[indexTipIndex].Transform.position, .5f);
    }

    void DetectPinching()
    {
        indexTipPosition = ovrSkeleton.Bones[indexTipIndex].Transform.position;
        thumbTipPosition = ovrSkeleton.Bones[thumbTipIndex].Transform.position;

        float thumbToIndexDistance = (indexTipPosition - thumbTipPosition).sqrMagnitude;
        if (thumbToIndexDistance < pinchThreshold)
        {
            if (!IsPinching)
            {
                IsPinching = true;
                HandPinchEnter?.Invoke(this);
                print("PINCH ON");
            }
        }
        else
        {
            if(IsPinching)
            {
                IsPinching = false;
                HandPinchExit?.Invoke(this);
            }
        }

            /*
            if (ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Index) || ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Middle))
                HandPinchEnter?.Invoke(this);*/
    }
}


