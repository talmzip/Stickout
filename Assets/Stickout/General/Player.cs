using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region Singelton Decleration

    private static Player _instance;

    public static Player Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    #endregion

    public OVRSkeleton handSkeletonL;
    public OVRSkeleton handSkeletonR;

    public Transform ovrCenterEye;
    public Transform PlayerStartPosition;

    public Color rightHandColor;
    public Color leftHandColor;

    public PinchPoint ppL;
    public PinchPoint ppR;

    public HandManager HandManagerR;
    public HandManager HandManagerL;

    void Start()
    {
       // RecenterOVRCamera();
        
    }

    void Update()
    {
        
    }

    private void RecenterOVRCamera()
    {
        Vector3 posDiff = ovrCenterEye.position - PlayerStartPosition.position;
        transform.position += posDiff;

        Quaternion rotDiff = ovrCenterEye.rotation * PlayerStartPosition.rotation;
        transform.rotation = rotDiff * transform.rotation;
    }

    public bool IsHandGhost(bool isLeftHand)
    {
        if (isLeftHand) 
            return ppL.IsGhost;
        
        return ppR.IsGhost;
    }
}
