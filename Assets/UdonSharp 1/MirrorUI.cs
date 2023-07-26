
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class MirrorUI : UdonSharpBehaviour
{
    public GameObject[] MirrorGroup;
    public GameObject[] ToggleGroup;
    public Color[] ColorStates;
    public Color[] SpotColorStates;

    [UdonSynced, FieldChangeCallback(nameof(MirrorIsOn))]
    private bool _mirrorIsOn;

    public bool MirrorIsOn
    {
        get { return _mirrorIsOn; }
        set 
        { 
            _mirrorIsOn = value;
            UpdateMirror();
        }
    }

    private Light _toggleLight;
    private Text _text;

    private const int TOGGLE_INDEX = 0;
    private const int TOGGLE_SPOT_INDEX = 1;

    private const int ON_STATE = 1;
    private const int OFF_STATE = 0;

    void Start()
    {
        if ((ToggleGroup.Length > TOGGLE_INDEX) && (ToggleGroup[TOGGLE_INDEX]))
        {
            var foundLabel = ToggleGroup[TOGGLE_INDEX].GetComponentInChildren<Text>();
            if (foundLabel)
            {
                _text = foundLabel;
            }
        }
        if ((ToggleGroup.Length > TOGGLE_SPOT_INDEX) && (ToggleGroup[TOGGLE_SPOT_INDEX]))
        {
            _toggleLight = ToggleGroup[TOGGLE_SPOT_INDEX].GetComponent<Light>();
        }
        
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            MirrorIsOn = false;
            Debug.Log($"[{Networking.LocalPlayer.playerId}] owns the mirror ui");
            RequestSerialization();
        }
        else
        {
            Debug.Log($"[{Networking.LocalPlayer.playerId}] doesn't own the mirror ui");
            _mirrorIsOn = false;
            UpdateMirror();
        }
        
        //UpdateMirror();
    }

    public override void OnDeserialization()
    {
        Debug.Log($"[{Networking.LocalPlayer.playerId}] MIRROR DESERIALIZE, UPDATE MIRROR {MirrorIsOn}");
        
        UpdateMirror();
    }

    public void ToggleMirror()
    {
        Debug.Log($"[MIRROR] {Networking.LocalPlayer.playerId} TOGGLEMIRROR INVOKED");
        if (!Networking.LocalPlayer.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer,gameObject);
        }
        //_mirrorIsOn = (_mirrorIsOn) ? false : true;
        MirrorIsOn = (MirrorIsOn) ? false : true;
        RequestSerialization();
        //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdateMirror");
    }

    public void UpdateMirror()
    {
        Debug.Log($"[MIRROR] {Networking.LocalPlayer.playerId} UPDATEMIRROR INVOKED");
        for (int index = 0; index < MirrorGroup.Length; ++index)
        {
            if (MirrorGroup[index])
            {
                MirrorGroup[index].SetActive(MirrorIsOn);
            }
        }
        UpdateToggleScreen();
    }

    public void UpdateToggleScreen()
    {
        _toggleLight.color = (MirrorIsOn) ? SpotColorStates[ON_STATE] : SpotColorStates[OFF_STATE];
        if (_text)
        {
            _text.color = (MirrorIsOn) ? ColorStates[ON_STATE] : ColorStates[OFF_STATE];
        }
    }
}
