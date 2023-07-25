
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class DoorUI : UdonSharpBehaviour
{
    public GameObject[] Lights;
    public GameObject[] Buttons;
    public Material[] Materials;
    public Color[] Colors;
    public DoorToggle LockableDoor;

    [UdonSynced]
    private bool _isLocked;

    private Light[] _lights;
    private MeshRenderer[] _renderers;
    private Button[] _buttons;

    private const int SWAP_MAT_INDEX = 1;
    private const int ENABLED = 1;
    private const int DISABLED = 0;

    void Start()
    {
        _lights = new Light[Lights.Length];
        _renderers = new MeshRenderer[Lights.Length];
        _buttons = new Button[Buttons.Length];
        CacheLights();
        CacheButtons();
        LockableDoor.IsLockable = true;
        UpdateLockState();
    }

    private void CacheLights()
    {
        for (int index = 0; index < Lights.Length; ++index)
        {
            var light_fixture = Lights[index];
            var renderer = light_fixture.GetComponent<MeshRenderer>();
            var light = light_fixture.GetComponentInChildren<Light>();
            _lights[index] = light;
            _renderers[index] = renderer;
        }
    }

    private void CacheButtons()
    {
        for (int index = 0; index < Buttons.Length; ++index)
        {
            var button = Buttons[index].GetComponent<Button>();
            _buttons[index] = button;
        }
    }

    public void EnableLock()
    {
        Debug.Log("[BEDROOM] LOCKDOWN");
        _isLocked = true;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdateLockState");
        LockableDoor.LockDoor();
    }

    public void DisableLock()
    {
        Debug.Log("[BEDROOM] OPEN SESAME");
        _isLocked = false;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdateLockState");
        LockableDoor.UnlockDoor();
    }

    public void UpdateLockState()
    {
        int state = (_isLocked) ? ENABLED : DISABLED;
        for (int index = 0; index < _lights.Length; ++index)
        {
            _lights[index].color = Colors[state];
        }
        for (int index = 0; index < _renderers.Length; ++index)
        {
            Material[] newMaterials = new Material[_renderers[index].materials.Length];
            newMaterials[0] = _renderers[index].materials[0];
            newMaterials[SWAP_MAT_INDEX] = Materials[state];
            _renderers[index].materials = newMaterials;
        }
        Debug.Log($"[BEDROOM] Lock State ? {_isLocked}");
        _buttons[ENABLED].interactable = !_isLocked;
        _buttons[DISABLED].interactable = _isLocked;
    }

    public override void OnDeserialization()
    {
        UpdateLockState();
    }
}
