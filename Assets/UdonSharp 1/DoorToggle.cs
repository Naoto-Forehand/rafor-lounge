
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
    }

    public override void OnDeserialization()
    {
        if (!IsLocked)
        {
            UpdateDoor();
        }
    }

    public override void Interact()
    {
        if (!Networking.LocalPlayer.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        if (!IsLocked)
        {
            IsOpen = (IsOpen) ? false : true;
            RequestSerialization();
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
    }

    public void UnlockDoor()
    {
        if (IsLockable)
        {
            IsLocked = false;
            RequestSerialization();
        }
    }

    public void ToggleLock()
    {
        if (IsLockable)
        {
            IsLocked = (IsLocked) ? false : true;
            RequestSerialization();
        }
    }

    private void UpdateDoor()
    {
        if ((!IsLocked) || (IsLocked && !IsOpen))
        {
            if (_doorAnimator.GetBool(DOOR_TOGGLE) != IsOpen)
            {
                _doorAnimator.SetBool(DOOR_TOGGLE, IsOpen);
            }
        }
    }
}
