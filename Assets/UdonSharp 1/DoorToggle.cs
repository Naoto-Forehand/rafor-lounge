
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DoorToggle : UdonSharpBehaviour
{
    public GameObject DoorObject;

    [UdonSynced]
    private bool _isOpen;
    private Animator _doorAnimator;
    private const string DOOR_TOGGLE = "is_open";

    void Start()
    {
        if (DoorObject)
        {
            _doorAnimator = DoorObject.GetComponent<Animator>();
        }
        UpdateDoor();
    }

    public override void OnDeserialization()
    {
        UpdateDoor();
    }

    public override void Interact()
    {
        if (_isOpen)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CloseDoor");
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OpenDoor");
        }
    }

    public void OpenDoor()
    {
        _isOpen = true;
        UpdateDoor();
    }

    public void CloseDoor()
    {
        _isOpen = false;
        UpdateDoor();
    }

    private void UpdateDoor()
    {
        _doorAnimator.SetBool(DOOR_TOGGLE, _isOpen);
    }
}
