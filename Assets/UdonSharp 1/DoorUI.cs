
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

    [UdonSynced, FieldChangeCallback(nameof(IsLocked))]
    private bool _isLocked;

    public bool IsLocked
    {
        get { return _isLocked; }
        set 
        { 
            _isLocked = value;
            int setValue = _isLocked ? ENABLED : DISABLED;
            UpdateLights(setValue);
            UpdateButtons();
        }
    }

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
        //LockableDoor.IsLockable = true;
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            IsLocked = false;

            RequestSerialization();
        }
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
        if (!Networking.IsOwner(gameObject))
        {
            var local = Networking.LocalPlayer;
            Networking.SetOwner(local, gameObject);
            Networking.SetOwner(local, LockableDoor.gameObject);
        }

        //LockableDoor.IsLocked = true;
        LockableDoor.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "LockDoor");
        IsLocked = true;
        RequestSerialization();
        //LockableDoor.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "LockDoor");
        //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdateLockState");
    }

    public void DisableLock()
    {
        Debug.Log("[BEDROOM] OPEN SESAME");
        if (!Networking.IsOwner(gameObject))
        {
            var local = Networking.LocalPlayer;
            Networking.SetOwner(local, gameObject);
            Networking.SetOwner(local, LockableDoor.gameObject);
        }

        //LockableDoor.IsLocked = false;
        LockableDoor.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UnlockDoor");
        IsLocked = false;
        RequestSerialization();

        //LockableDoor.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UnlockDoor");
        //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdateLockState");
    }

    public void UpdateLockState()
    {
        int state = (_isLocked) ? ENABLED : DISABLED;
        string eventName = (_isLocked) ? "LockDoor" : "UnlockDoor";
        UpdateLights(state);
        //for (int index = 0; index < _lights.Length; ++index)
        //{
        //    _lights[index].color = Colors[state];
        //}
        //for (int index = 0; index < _renderers.Length; ++index)
        //{
        //    Material[] newMaterials = new Material[_renderers[index].materials.Length];
        //    newMaterials[0] = _renderers[index].materials[0];
        //    newMaterials[SWAP_MAT_INDEX] = Materials[state];
        //    _renderers[index].materials = newMaterials;
        //}
        Debug.Log($"[BEDROOM] {Networking.LocalPlayer.playerId} Lock State ? {_isLocked}");
        //_buttons[ENABLED].interactable = !_isLocked;
        //_buttons[DISABLED].interactable = _isLocked;
        UpdateButtons();
        //LockableDoor.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, eventName);
    }

    private void UpdateLights(int state)
    {
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
    }

    private void UpdateButtons()
    {
        _buttons[ENABLED].interactable = !IsLocked;
        _buttons[DISABLED].interactable = IsLocked;
    }

    public override void OnDeserialization()
    {
        int state = (_isLocked) ? ENABLED : DISABLED;
        UpdateLights(state);
        UpdateButtons();
    }
}
