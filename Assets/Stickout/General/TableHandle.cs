using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HandleState
{
    Ready,
    Pinched,
    Inactive
}
public class TableHandle : MonoBehaviour
{
    HandleState State;
    List<PinchPoint> ppInRange;
    PinchPoint pinchingHand;
    public Transform TableGroup;
    Transform originalParent;
    MeshRenderer mr;
    float pinchedY;
    Vector3 ppPositionOnPinch;
    Vector3 tablePositionOnPinch;
    public AnimationCurve rebalanceCurve;
    void Start()
    {
        originalParent = TableGroup.parent;
        mr = GetComponent<MeshRenderer>();
        ppInRange = new List<PinchPoint>();
        mr.material.color = new Color(1, 1, 1, .2f);

    }

    void LateUpdate()
    {
        switch(State)
        {
            case HandleState.Ready:
                // look for a pinch in either hands TODO: Move to an event. Pass the pinch point on the OnPinchEnter event
                if (ppInRange.Count > 0)
                {
                    foreach (PinchPoint pp in ppInRange)
                        if (pp.IsPinching)
                            PinchDetected(pp);
                }
                break;
            case HandleState.Pinched:
                if (!pinchingHand.IsPinching)
                {
                    NotPinchedAnymore();
                    return;
                }
                else
                {
                    TableGroup.position = tablePositionOnPinch + (pinchingHand.transform.position - ppPositionOnPinch);
                    TableGroup.localEulerAngles = new Vector3(0, pinchingHand.transform.eulerAngles.y - pinchedY, 0);
                }
                break;
            case HandleState.Inactive:
                break;
        }

    }

    private void PinchDetected(PinchPoint pp)
    {
        State = HandleState.Pinched;

        pinchingHand = pp;
        ppPositionOnPinch = pp.transform.position;
        tablePositionOnPinch = TableGroup.position;
        pinchedY = pp.transform.eulerAngles.y;
    }

    private void NotPinchedAnymore()
    {
        TableGroup.parent = originalParent;

        State = HandleState.Ready;
        mr.material.color = new Color(1, 1, 1, .2f);

        pinchingHand = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            PinchPoint pp = other.GetComponent<PinchPoint>();

            if (ppInRange.Count == 0)
                mr.material.color = Color.white;

            ppInRange.Add(pp);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            PinchPoint pp = other.GetComponent<PinchPoint>();
            //if (pinchingHand == pp) NotPinchedAnymore();

            ppInRange.Remove(pp);

            if (ppInRange.Count == 0)
                mr.material.color = new Color(1, 1, 1, .2f);
        }
    }


}
