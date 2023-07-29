
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MirrorTrigger : UdonSharpBehaviour
{
    public GameObject MirrorCanvas;

    [UdonSynced, FieldChangeCallback(nameof(IsEnabled))]
    private bool _isEnabled;

    [UdonSynced, FieldChangeCallback(nameof(OwnerID))]
    private int _ownerID;
    public int OwnerID
    {
        get { return _ownerID; }
        set { _ownerID = value; }
    }

    public bool IsEnabled
    {
        get { return _isEnabled; }
        set 
        {
            _isEnabled = value;
            MirrorCanvas.SetActive(_isEnabled);
        }
    }

    private const int UNASSIGNED_ID = -10;
    private int _localID;

    void Start()
    {
        _localID = Networking.LocalPlayer.playerId;
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            IsEnabled = false;
            OwnerID = UNASSIGNED_ID;
            RequestSerialization();
        }
        else
        {
            Debug.Log($"[{Networking.LocalPlayer.playerId}] MIRROR CANVAS ? {_isEnabled}");
            MirrorCanvas.SetActive(_isEnabled);
        }
        Debug.Log($"[{Networking.LocalPlayer.playerId}] MirrorCanvas on ? {(MirrorCanvas.activeInHierarchy)}");
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.IsValid() && !IsEnabled && OwnerID == UNASSIGNED_ID)
        {
            Networking.SetOwner(player, gameObject);
            IsEnabled = true;
            OwnerID = player.playerId;

            RequestSerialization();
        }
    }

    public override void OnPlayerTriggerStay(VRCPlayerApi player)
    {
        if (player.IsValid() && OwnerID == UNASSIGNED_ID && !IsEnabled)
        {
            Networking.SetOwner(player, gameObject);
            IsEnabled = true;
            OwnerID = player.playerId;

            RequestSerialization();
        }
        else if (player.IsValid() && OwnerID == UNASSIGNED_ID)
        {
            Networking.SetOwner(player, gameObject);
            OwnerID = player.playerId;

            RequestSerialization();
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        Debug.Log($"[{player.playerId}] exit trigger {OwnerID} {IsEnabled}");
        if (player.IsValid() && IsEnabled && OwnerID == player.playerId)
        {
            IsEnabled = false;
            OwnerID = UNASSIGNED_ID;

            RequestSerialization();
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (_localID == OwnerID)
        {
            OwnerID = UNASSIGNED_ID;
            if (IsEnabled)
            {
                IsEnabled = false;
            }
            
            RequestSerialization();
        }
    }

    public override void OnDeserialization()
    {
        Debug.Log($"DESERIALIZED {Networking.LocalPlayer.playerId} isEnabled ? {_isEnabled}"); 
    }
}
