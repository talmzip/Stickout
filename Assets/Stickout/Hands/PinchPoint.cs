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
    int middleTipIndex = 11;

    OVRSkeleton ovrSkeleton;
    OVRHand ovrHand;
    public PhysicalHand physicalHand;
    public HandManager handManager;
    MeshRenderer mr;
    public Transform handRoot;

    public UnityAction<PinchPoint> OnPinchEnter;
    public UnityAction<PinchPoint> HandPinchExit;

    public float pinchThreshold = .0005f;
    
    Vector3 indexTipPosition;
    Vector3 middleTipPosition;
    Vector3 thumbTipPosition;

    public float RecoveryPinchDistanceThreshold = .001f;    // how close to PinchPoint should ghost hand pinch for it to recover the hand
    public AudioSource reattachsound;
    public AudioSource reattachAvailablesound;
    public AudioSource closeEnoughToReAttach;
    public AudioSource tooFarToReAttach;
    bool isHandCloseToPinch = false;

    void Start()
    {
        ovrSkeleton = IsLeft ? Player.Instance.handSkeletonL : Player.Instance.handSkeletonR;
        ovrHand = ovrSkeleton.transform.GetComponent<OVRHand>();
        mr = GetComponent<MeshRenderer>();
        mr.material.color = Color.clear;
    }

    void Update()
    {
        if (ovrSkeleton.IsDataHighConfidence)
        {
            UpdatePosition();
            DetectPinching();
        }
        else
            IsPinching = false;

    }

    void UpdatePosition()
    {
        transform.position = Vector3.Lerp(physicalHand.Bones[thumbTipIndex].position, physicalHand.Bones[indexTipIndex].position, .5f);
        transform.rotation = handRoot.rotation;
    }

    // TODO: Should be moved to Ghost Hand or Hand Manager. Pinchpoint should be part of physical hand
    void DetectPinching()
    {
        indexTipPosition = ovrSkeleton.Bones[indexTipIndex].Transform.position;
        middleTipPosition = ovrSkeleton.Bones[middleTipIndex].Transform.position;
        thumbTipPosition = ovrSkeleton.Bones[thumbTipIndex].Transform.position;

        float thumbToIndexDistance = (indexTipPosition - thumbTipPosition).sqrMagnitude;
        float thumbToMiddleDistance = (middleTipPosition- thumbTipPosition).sqrMagnitude;

        if (thumbToIndexDistance < pinchThreshold || thumbToMiddleDistance < pinchThreshold)
        {
            if (!IsPinching)
            {
                IsPinching = true;
                OnPinchEnter?.Invoke(this);
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

    }

    public void RecoveryOn()
    {
        StartCoroutine(RecoveryCoroutine());
    }

    IEnumerator RecoveryCoroutine()
    {
        isHandCloseToPinch = false;
        yield return StartCoroutine(Appear());
        yield return StartCoroutine(WaitForPinchRecovery());
        mr.material.color = Color.clear;
        handManager.GetPhysicalBack();
    }

    IEnumerator Appear()
    {
        reattachAvailablesound.Play();

        float lerpTime = 0;
        while(lerpTime < .5f)
        {
            lerpTime += Time.deltaTime;
            
            float t = lerpTime / .5f;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            mr.material.color = Color.Lerp(Color.clear, new Color(1f,1f,1f,.5f), t);
            yield return null;

        }

    }

    IEnumerator WaitForPinchRecovery()
    {
        bool didRecover = false;
        while(!didRecover)
        {
            Vector3 ghostPPPos = Vector3.Lerp(thumbTipPosition, indexTipPosition, .5f);

            // if hand is close enough
            if ((ghostPPPos - transform.position).sqrMagnitude <= RecoveryPinchDistanceThreshold)
            {
                // change color to full if hand is close enough to pinch
                if(!isHandCloseToPinch)
                {
                    mr.material.color = Color.white;
                    isHandCloseToPinch = true;

                    closeEnoughToReAttach.Stop();
                    tooFarToReAttach.Stop();
                    closeEnoughToReAttach.Play();
                }
                if (IsPinching)
                {
                    didRecover = true;
                }
            }
            else
            {
                // change color to faded if hand is not close enough to pinch
                if(isHandCloseToPinch)
                {
                    closeEnoughToReAttach.Stop();
                    tooFarToReAttach.Stop();
                    tooFarToReAttach.Play();
                    mr.material.color = new Color(1f, 1f, 1f, .5f);
                    isHandCloseToPinch = false;
                }
            }
            yield return null;
        }
        reattachsound.Play();
    }

}


