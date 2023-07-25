
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MirrorTrigger : UdonSharpBehaviour
{
    public GameObject MirrorCanvas;

    [UdonSynced]
    private bool _isEnabled;

    void Start()
    {
        _isEnabled = false;
        UpdateMirrorCanvas();
    }

    public void UpdateMirrorCanvas()
    {
        MirrorCanvas.SetActive(_isEnabled);
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.IsValid() && !_isEnabled)
        {
            _isEnabled = true;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdateMirrorCanvas");
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player.IsValid() && _isEnabled)
        {
            _isEnabled = false;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdateMirrorCanvas");
        }
    }
}
