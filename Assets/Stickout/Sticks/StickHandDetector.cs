using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickHandDetector : MonoBehaviour
{
    Stick stick;
    public List<PinchPoint> HandsInRange;
    public Transform StickTipPosition;
    public Transform StickTipMesh;

    MeshRenderer tipMR;
    Color tipStartColor;
    float pinchingTime = 0;

    Vector3 offsetFromTip;

    void Start()
    {
        stick = GetComponentInParent<Stick>();
        if (StickTipMesh.gameObject.activeSelf)
        {
            tipMR = StickTipMesh.GetComponent<MeshRenderer>();
            tipStartColor = tipMR.material.color;
        }
        tipStartColor = stick.IsOcclusionLeft ? Player.Instance.leftHandColor : Player.Instance.rightHandColor;

    }


    private void Update()
    {
        transform.position = stick.transform.position + new Vector3(0, stick.StickMesh.transform.localScale.y * stick.StickPivot.transform.localScale.z + .1f, 0);

        if (HandsInRange.Count > 1)
            stick.LeanTowards(GetCloserHand().transform.position);
        else
        {
            if (HandsInRange.Count > 0)
                stick.LeanTowards(HandsInRange[0].transform.position);
            else
            {
                // if not hand in range- lean back.
                stick.LeanTowards(transform.position);
            }
        }

        if (stick.Type == StickType.Occlusion)
        {
            bool raisePinchtime = false;
            if (HandsInRange.Count > 0)
            {
                foreach (PinchPoint pp in HandsInRange)
                {
                    // if hand is pinching AND matches the R/L of the occlusion stick
                    if (pp.IsPinching && (pp.IsLeft == stick.IsOcclusionLeft))
                    {
                        raisePinchtime = true;
                        // count pinching time

                        if (pinchingTime >= stick.pinchHoldTime)
                        {
                            stick.ValidPinchDetected();
                            pinchingTime = 0;
                        }
                    }
                }
            }

            if (raisePinchtime)
                pinchingTime += Time.deltaTime;
            else
            {
                if (pinchingTime > 0)
                    pinchingTime -= Time.deltaTime;
                else
                    pinchingTime = 0;
            }

            float t = pinchingTime / stick.pinchHoldTime;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            tipMR.material.color = Color.Lerp(tipStartColor, Color.cyan, t);

            stick.isBeingPinched = pinchingTime > 0;

        }
    }

    void HandPinched(PinchPoint pp)
    {
        if (pp.IsGhost) return;

        if (stick.Type == StickType.Normal)
        {
            if (stick.isPickable)
            {
                if (HandsInRange.Count < 2)
                    stick.ValidPinchDetected();
                else
                {
                    // only count the pinch if it was the closer hand's one
                    if (pp == GetCloserHand())
                        stick.ValidPinchDetected();
                }
            }
        }
    }

    private PinchPoint GetCloserHand()
    {
        PinchPoint closerHand = HandsInRange[0];
        for (int i = 1; i < HandsInRange.Count; i++)
        {
            if ((HandsInRange[i].transform.position - StickTipPosition.position).sqrMagnitude < (closerHand.transform.position - StickTipPosition.position).sqrMagnitude)
                closerHand = HandsInRange[i];
        }

        return closerHand;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            PinchPoint pp = other.GetComponent<PinchPoint>();
            HandsInRange.Add(pp);
            pp.HandPinchEnter += HandPinched;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            PinchPoint pp = other.GetComponent<PinchPoint>();
            HandsInRange.Remove(pp);
            pp.HandPinchEnter -= HandPinched;
        }
    }
}
