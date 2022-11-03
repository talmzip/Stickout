using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HandState
{
    Ghost,
    Transition,
    Physical
}
public class HandManager : MonoBehaviour
{
    public OVRSkeleton skeleton;
    public HandState State = HandState.Transition;
    public PhysicalHand Physical;

    public GhostHand Ghost;
    public PinchPoint pinchPoint;

    [Header("Ghost Hand")]
    public Color GhostColor;
    public float ClearToGhostDuration = 1f;
    public AnimationCurve ClearToGhostCurve;

    AudioSource TrackingLossSound;

    public float RecoveryTime = 5;
   
    void Start()
    {
        TrackingLossSound = GetComponent<AudioSource>();
        Physical.Init(this, skeleton);

        Ghost.ChangeColor(Color.clear);

        StartCoroutine(waitForTrackingInit());
    }

    void Update()
    {
        if (skeleton.IsDataHighConfidence)
        {
            if (State == HandState.Physical)
                Physical.TrackHandMovements();
        }
        else
            if (State != HandState.Transition)
            if (GameManager.Instance.SpawnInstructionsIndex > -1)
                OnTrackingLost();

    }

    // called on GameOver
    public void Detach()
    {
        if (State == HandState.Physical)
        {
            Physical.Detach();
            pinchPoint.IsGhost = true;
        }
    }

    private void OnTrackingLost()
    {
        if (State == HandState.Physical)
        {
            Physical.Detach();
            TrackingLossSound.Play();
            pinchPoint.IsGhost = true;

        }


        Ghost.ChangeColor(Color.clear);
        StartCoroutine(revealGhostAfterTrackingReturn());


    }

    // on start tracking takes a moment to init and first track hands
    private IEnumerator waitForTrackingInit()
    {
        while (!skeleton.IsDataHighConfidence) yield return null;
        State = HandState.Physical;
    }

    // fades in ghost hand (called when tracking is gained after was lost)
    private IEnumerator revealGhostAfterTrackingReturn()
    {
        bool wasHandPhysical = State == HandState.Physical;
        State = HandState.Transition;

        while (!skeleton.IsDataHighConfidence) yield return null;

        float lerpTime = 0;
        while (lerpTime < ClearToGhostDuration)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / ClearToGhostDuration;
            t = ClearToGhostCurve.Evaluate(t);

            Ghost.ChangeColor(Color.Lerp(Color.clear, GhostColor, t));

            yield return null;
        }
        
        // so it won't get called when tracking loss occured on ghost 
        if(wasHandPhysical)
            StartCoroutine(CountToHandRecovery());


        State = HandState.Ghost;

    }

    IEnumerator CountToHandRecovery()
    {
        yield return new WaitForSecondsRealtime(RecoveryTime);
        // make physical pinch point appear and detect recovery.
        pinchPoint.RecoveryOn();
    }

    public void GetPhysicalBack()
    {
        if (State != HandState.Physical)
        {
            Physical.ReAttach();
            pinchPoint.IsGhost = false;
        }
    }
}
