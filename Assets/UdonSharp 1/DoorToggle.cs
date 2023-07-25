
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DoorToggle : UdonSharpBehaviour
{
    public GameObject DoorObject;
    public bool IsLockable;

    [UdonSynced]
    private bool _isOpen;

    private bool _isLocked;
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
        if (!_isLocked)
        {
            UpdateDoor();
        }
    }

    public override void Interact()
    {
        Debug.Log($"[DOOR] This door is {this.IsLockable} and it is locked ? {_isLocked}");
        if (!this._isLocked)
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

    public  void LockDoor()
    {
        //_isLocked = true;
        CloseDoor();
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ToggleLock");
    }

    public void UnlockDoor()
    {
        //this._isLocked = false;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ToggleLock");
        OpenDoor();
    }

    public void ToggleLock()
    {
        if (IsLockable)
        {
            this._isLocked = (this._isLocked) ? false : true;
            Debug.Log($"[DOOR] {this.gameObject.name} Toggled isLocked to {_isLocked}");
        }
    }

    private void UpdateDoor()
    {
        if ((!this._isLocked) || (this._isLocked && !_isOpen))
        {
            Debug.Log($"[DOOR] isLocked ? {_isLocked} isOpen ? {_isOpen}");
            _doorAnimator.SetBool(DOOR_TOGGLE, _isOpen);
        }
    }
}
