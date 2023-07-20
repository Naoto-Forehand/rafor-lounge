
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class ButtonToggle : UdonSharpBehaviour
{
    public GameObject Button;
    public GameObject Mirror;
    public GameObject MirrorSpotLight;
    public GameObject ButtonSpotLight;
    public Color ButtonOnColor;
    public Color ButtonOffColor;
    public Material ButtonOnMat;
    public Material ButtonOffMat;

    [UdonSynced]
    private bool _mirrorEnabled;
    private Light _buttonLight;
    private MeshRenderer _buttonRenderer;

    void Start()
    {
        if (Mirror && MirrorSpotLight)
        {
            Mirror.SetActive(false);
            MirrorSpotLight.SetActive(false);
        }

        if (Button)
        {
            _buttonRenderer = Button.GetComponent<MeshRenderer>();
        }

        if (ButtonSpotLight)
        {
            _buttonLight = ButtonSpotLight.GetComponent<Light>();
        }

        _mirrorEnabled = false;
    }

    public override void Interact()
    {
        UpdateOwner();
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Toggle");
    }

    public override void OnDeserialization()
    {
        ToggleMirror(_mirrorEnabled);
        ToggleButtonSpotLight(!_mirrorEnabled);
    }

    public void Toggle()
    {
        _mirrorEnabled = (_mirrorEnabled) ? false : true;
        ToggleMirror(_mirrorEnabled);
        ToggleButtonSpotLight(!_mirrorEnabled);
    }

    private void UpdateOwner()
    {
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
    }

    private void ToggleMirror(bool value)
    {
        Mirror.SetActive(value);
        MirrorSpotLight.SetActive(value);
    }

    private void ToggleButtonSpotLight(bool value)
    {
        _buttonLight.color = (value) ? ButtonOnColor : ButtonOffColor;
        _buttonRenderer.material = (value) ? ButtonOnMat : ButtonOffMat;
    }
}
