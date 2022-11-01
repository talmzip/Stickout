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

    void Start()
    {
        TrackingLossSound = GetComponent<AudioSource>();
        Physical.Init(this,skeleton);

        Ghost.ChangeColor(Color.clear);

        StartCoroutine(waitForTrackingInit());
    }

    void Update()
    {
        if(skeleton.IsDataHighConfidence)
        {
            if(State == HandState.Physical)
                Physical.TrackHandMovements();
        }
        else
            if(State !=HandState.Transition)
                if(Time.time > 5)    
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

        State = HandState.Transition;

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

        State = HandState.Ghost;
    }

    public void GetPhysicalBack()
    {
        if (State == HandState.Ghost)
        {
            Physical.ReAttach();
            pinchPoint.IsGhost = false;
        }
    }
}
