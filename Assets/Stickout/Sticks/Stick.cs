using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum StickType
{
    Normal,
    Occlusion,
    Jumper,
    Fast
}

public enum StickStage
{
    Bonus,              // stick just appeared and is in the peak of its height for a ~.3 seconds in the appearing animation
    High,               // stick regular height after appear
    Medium,             // stick fell down one time
    Low,                // stick fell down two times
    Folded              // folded - game over
}

public class Stick : MonoBehaviour
{
    [Header("For Testing")]
    public bool HandPinch;
    public Transform testHand;
    public bool loopAppearence;
    public float waitBeforeAnimLoop = 2;
    public bool loopPicked;
    public bool autoPick;

    public bool isPickable;                 // only when true can be picked with a pinch
    [Header("Publics")]
    public bool isHandInRange;              // true when player's pinch point is in range.
    public float LifeTime = 8;
    public float SignalingTime;             // after how many seconds start signaling?
    private float timeAlive = 0;
    Color startColor;
    public StickType Type;
    public StickStage Stage;
    public int SpawnIndex;                  // updated from StickManager on Spawn according to its spawn point.

    [Header("References")]
    public Transform StickMesh;
    MeshRenderer StickMR;
    public Transform StickPivot;
    public Transform HandDetector;
    public PinchPoint closerHand;       // the closest hand to stick (from all hands in range) 
    SticksManager stickManager;

    float leanAmount = 1;
    float leanToHandSpeed = 4.39f;
    float leanBackSpeed = 3.14f;
    Quaternion startRotation;

    [Header("Appearening Animation")]
    public float prePopDuration = .5f;
    public AnimationCurve prePopCurve;

    public float popDuration = .3f;
    public AnimationCurve popCurve;

    public Vector3 initialScale = new Vector3(5, 5, 0);
    public Vector3 prePopScale = new Vector3(7, 7, .05f);

    [Header("Picked Animation")]
    public float pickedAnimDuration = .3f;
    public AnimationCurve pickedAnimScaleCurve;
    public AnimationCurve pickedAnimColorCurve;
    public Vector3 pickedScale = new Vector3(.6f,.6f,1.8f);

    [Header("Stage Fold")]
    public float foldDuration;
    public AnimationCurve foldCurve;
    public float foldMidHeight;
    public float foldLowHeight;
    public float CompleteFoldDuration;

    [Header("Sounds")]
    public AudioSource pickupSwish;
    public AudioSource pickupScore;
    public AudioSource soundAppear;
    public AudioSource soundFold;

    [Header("Occlusion")]
    public bool IsOcclusionLeft;    // can be picked up by left or right
    public float pinchHoldTime = 1f;
    public bool isBeingPinched = false;

    void Start()
    {
        StickMR = StickMesh.GetComponent<MeshRenderer>();
        startColor = StickMR.material.color;
        isPickable = false;
        startRotation = StickPivot.rotation;

        stickManager = GetComponentInParent<SticksManager>();
        // run appearence animation    
        StartCoroutine(AppearCo());
    }

    void Update()
    {
        #region collapsing code
        /*if(isPickable)
        {
            // count to collapse
            timeAlive += Time.deltaTime;
            
            if (timeAlive > LifeTime / 2)
            {
                // signal collapsing
                float t = Mathf.Sin(Time.time * collapseSignalingSpeed);
                StickMR.material.color = Color.Lerp(startColor, collapseSignalingColor, t);

                if (timeAlive > LifeTime)
                    Collapse();
            }
        }
        */
        #endregion

        #region For Testing
        // pinch detection
        if (HandPinch)
        {
            HandPinch = false;
            ValidPinchDetected();
        }
        #endregion
    }


    // called from StickHandDetector upon a valid pinch detection.
    public void ValidPinchDetected()
    {
        if(isPickable)
            Pickup();
    }

    public void Collapse()
    {
        isPickable = false;

        // tell game manager that stick collapsed

        StartCoroutine(CollapseCo());


    }

    public void LeanTowards(Vector3 handPos)
    {
        if (isPickable)
        {
            // calc full lookAt rotation
            Quaternion lookAtRotation = Quaternion.LookRotation(handPos - StickPivot.position);

            // how much should stick lean towards hands
            Quaternion targetRotation = Quaternion.Lerp(startRotation, lookAtRotation, leanAmount);

            // actual leaning
            StickPivot.rotation = Quaternion.Lerp(StickPivot.rotation, targetRotation, leanToHandSpeed * Time.deltaTime);
        }
    }

    void Pickup()
    {
        isPickable = false;
        StopAllCoroutines();
        StartCoroutine(PickedCo());
    }

    // called from SticksManager when GameOver
    public void Fold()
    {
        isPickable = false;
        StopAllCoroutines();
        StartCoroutine(CompleteFold());
    }

   IEnumerator AppearCo()
    {
        Stage = StickStage.High;
        #region Pre Pop


        StickPivot.localScale = initialScale;
        //yield return new WaitForSecondsRealtime(Random.Range(3.0f, 10.0f));

        soundAppear.Play();
        float lerpTime = 0;
        while(lerpTime<prePopDuration)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / prePopDuration;
            t = prePopCurve.Evaluate(t);

            StickPivot.localScale = Vector3.LerpUnclamped(initialScale, prePopScale, t);
            yield return null;

        }
        #endregion

        #region Pop

        lerpTime = 0;
        while (lerpTime < popDuration)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / popDuration;
            t = popCurve.Evaluate(t);

            float x = Mathf.Lerp(prePopScale.x, 1, t);
            float y = x;
            float z = Mathf.LerpUnclamped(prePopScale.z, 1, t);
            //StickPivot.localScale = Vector3.LerpUnclamped(prePopScale, Vector3.one, t);
            StickPivot.localScale = new Vector3(x, y, z);
            if(!loopAppearence) // this if is for testing. otherwise, stick should become pickable when height enough, roughly after t>.5
            if (z>.7f) isPickable = true;

            yield return null;
           
        }

        #endregion
        Stage = StickStage.High;
        StartCoroutine(CollapseCo());
    }

   IEnumerator PickedCo()
    {
        Color oColor = StickMR.material.color;
        Color finalColor = new Color(oColor.r, oColor.g, oColor.b, 0);

        pickupSwish.pitch = Random.Range(1f,1.3f);
        pickupSwish.Play();
        pickupScore.Play();


        float lerpTime = 0;
        while(lerpTime < pickedAnimDuration)
        {
            lerpTime += Time.deltaTime;

            // change size
            float tScale = lerpTime / pickedAnimDuration;
            tScale = pickedAnimScaleCurve.Evaluate(tScale);
            StickPivot.localScale = Vector3.LerpUnclamped(Vector3.one, pickedScale, tScale);
            
            // change color
            float tColor = lerpTime / pickedAnimDuration;
            tColor = pickedAnimColorCurve.Evaluate(tColor);
            StickMR.material.color = Color.Lerp(oColor, finalColor, tColor);

            yield return null;

        }

        SticksManager.Instance.StickPickedUp(this,Stage==StickStage.Bonus);
    }
    
   IEnumerator CollapseCo()
    {
        yield return new WaitForSecondsRealtime(LifeTime / 3);

        yield return StartCoroutine(FoldToStage(StickStage.Medium));

        yield return new WaitForSecondsRealtime(LifeTime / 3);

        yield return StartCoroutine(FoldToStage(StickStage.Low));

        yield return new WaitForSecondsRealtime(LifeTime / 3);

        yield return StartCoroutine(FoldToStage(StickStage.Folded));

        stickManager.StickCollapsed(this);
    }

   IEnumerator FoldToStage(StickStage fallToStage)
    {
        // don't fold if while being pinched
        while (isBeingPinched) yield return null;

        Color oColor = StickMR.material.color;

        soundFold.Stop();
        soundFold.Play();

        float lerpTime = 0;
        while (lerpTime < foldDuration)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / foldDuration;
            t = foldCurve.Evaluate(t);

            StickPivot.localScale = Vector3.LerpUnclamped(GetStageScale(Stage), GetStageScale(fallToStage), t);
            
            if (fallToStage == StickStage.Low)
                StickMR.material.color = Color.Lerp(oColor, Color.red, t);
            
            yield return null;

        }

        Stage = fallToStage;
    }

    IEnumerator CompleteFold()
    {
        float lerpTime = 0;
        Vector3 CurrentScale = StickPivot.localScale;
        while (lerpTime < CompleteFoldDuration)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / CompleteFoldDuration;

            StickPivot.localScale = Vector3.Lerp(CurrentScale, GetStageScale(StickStage.Folded), t);

            yield return null;

        }
    }

   Vector3 GetStageScale(StickStage stage)
    {
        switch(stage)
        {
            case StickStage.High: return Vector3.one;
            case StickStage.Medium: return new Vector3(1, 1, foldMidHeight);
            case StickStage.Low: return new Vector3(1, 1, foldLowHeight);
            case StickStage.Folded: return new Vector3(1, 1, 0f);
            default: return Vector3.one;
        }
    }
}
