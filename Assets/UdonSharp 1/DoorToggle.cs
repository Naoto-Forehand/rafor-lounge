
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DoorToggle : UdonSharpBehaviour
{
    public GameObject DoorObject;
    public bool IsLockable;

    [UdonSynced, FieldChangeCallback(nameof(IsOpen))]
    private bool _isOpen;
    [UdonSynced, FieldChangeCallback(nameof(IsLocked))]
    private bool _isLocked;

    public bool IsOpen
    {
        get { return _isOpen; }
        set 
        { 
            _isOpen = value;
            UpdateDoor();
        }
    }

    public bool IsLocked
    {
        get { return _isLocked; }
        set 
        {
            if (value)
            {
                IsOpen = false;
            }
            _isLocked = value;
            UpdateDoor();
        }
    }

    private Animator _doorAnimator;
    private const string DOOR_TOGGLE = "is_open";

    void Start()
    {
        if (DoorObject)
        {
            _doorAnimator = DoorObject.GetComponent<Animator>();
        }

        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            IsOpen = false;
            IsLocked = false;
            RequestSerialization();
        }
        //UpdateDoor();
    }

    public override void OnDeserialization()
    {
        Debug.Log($"[DOOR] {Networking.LocalPlayer.playerId} This door is {IsLockable} and it is locked ? {IsLocked}");
        if (!IsLocked)
        {
            UpdateDoor();
        }
    }

    public override void Interact()
    {
        Debug.Log($"[DOOR] {Networking.LocalPlayer.playerId} This door is {IsLockable} and it is locked ? {IsLocked}");
        if (!Networking.LocalPlayer.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        if (!IsLocked)
        {
            IsOpen = (IsOpen) ? false : true;
            RequestSerialization();
            //if (_isOpen)
            //{
            //    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CloseDoor");
            //}
            //else
            //{
            //    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OpenDoor");
            //}
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

    public void LockDoor()
    {
        if (IsLockable)
        {
            IsLocked = true;
            RequestSerialization();
        }

        //_isLocked = true;
        //CloseDoor();
        //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ToggleLock");
    }

    public void UnlockDoor()
    {
        if (IsLockable)
        {
            IsLocked = false;
            RequestSerialization();
        }

        //this._isLocked = false;
        //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ToggleLock");
        //OpenDoor();
    }

    public void ToggleLock()
    {
        if (IsLockable)
        {
            IsLocked = (IsLocked) ? false : true;
            Debug.Log($"[DOOR] {Networking.LocalPlayer.playerId} {this.gameObject.name} Toggled isLocked to {IsLocked}");
            RequestSerialization();
        }
    }

    private void UpdateDoor()
    {
        if ((!IsLocked) || (IsLocked && !IsOpen))
        {
            Debug.Log($"[DOOR] {Networking.LocalPlayer.playerId} isLocked ? {IsLocked} isOpen ? {IsOpen} ANIMATOR BOOL SET");
            if (_doorAnimator.GetBool(DOOR_TOGGLE) != IsOpen)
            {
                _doorAnimator.SetBool(DOOR_TOGGLE, IsOpen);
            }
        }
    }
}
