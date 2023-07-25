
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AnimationManipulator : UdonSharpBehaviour
{
    public Animator DinoToAnimate;
    public int DinoIndex;

    public int ActiveSide
    {
        get { return _activeSide + 1; }
    }

    [UdonSynced]
    private int _activeSide;
    [UdonSynced]
    private bool _isMoving;

    private Rigidbody _cubeBody;
    private Transform _cubeTransform;
    

    void Start()
    {
        _activeSide = 0;
        _isMoving = false;
        _cubeBody = gameObject.GetComponent<Rigidbody>();
        _cubeTransform = gameObject.transform;
    }

    public void UpdateAnimation()
    {
        Debug.Log($"[ANIMATOR MANIPULATOR] ACTIVE SIDE IS {_activeSide}");
        if (DinoToAnimate != null)
        {
            Debug.Log($"[ANIMATOR MANIPULATOR] ACTIVE SIDE IS {_activeSide}");
            //DinoToAnimate.Play()
        }
    }

    public override void OnDrop()
    {
        Debug.Log("[ANIMATOR MANIPULATOR] Has been dropped");
        _isMoving = true;
    }

    void LateUpdate()
    {
        if (_isMoving)
        {
            if (_cubeBody.velocity.magnitude <= 0.2f)
            {
                _cubeBody.velocity = Vector3.zero;
                DetectSideUp();
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdateAnimation");
                _isMoving = false;
            }
        }
    }

    public void DetectSideUp()
    {
        float[] sides = new float[6];
        sides[0] = Vector3.Angle(_cubeTransform.up, Vector3.up);
        sides[1] = Vector3.Angle(-_cubeTransform.up, Vector3.up);
        sides[2] = Vector3.Angle(_cubeTransform.right, Vector3.up);
        sides[3] = Vector3.Angle(-_cubeTransform.right, Vector3.up);
        sides[4] = Vector3.Angle(_cubeTransform.forward, Vector3.up);
        sides[5] = Vector3.Angle(-_cubeTransform.forward, Vector3.up);
        var smallestAngle = Mathf.Min(sides);
        var index = System.Array.IndexOf(sides, smallestAngle);
        Debug.Log($"[ANIMATION MANIPULATOR] index {index}");
        _activeSide = index;
    }
}
