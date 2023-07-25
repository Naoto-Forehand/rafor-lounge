
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

    [UdonSynced]
    private bool _mirrorIsOn;

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
            //var children = ToggleGroup[TOGGLE_INDEX].GetComponentsInChildren<Transform>();
            //for (int index = 0; index < children.Length; ++index)
            //{
            //    if (children[index].name.Contains("Label"))
            //    {
            //        Debug.Log($"[MIRROR] Contains Text ? {(children[index].GetComponent<Text>() != null)}");
            //    }
            //}
            var foundLabel = ToggleGroup[TOGGLE_INDEX].GetComponentInChildren<Text>();
            if (foundLabel)
            {
                _text = foundLabel;
            }
            Debug.Log($"[MIRROR] {foundLabel.gameObject.name}");
        }
        if ((ToggleGroup.Length > TOGGLE_SPOT_INDEX) && (ToggleGroup[TOGGLE_SPOT_INDEX]))
        {
            _toggleLight = ToggleGroup[TOGGLE_SPOT_INDEX].GetComponent<Light>();
        }
        UpdateMirror();
    }

    public override void OnDeserialization()
    {
        UpdateMirror();
    }

    public void ToggleMirror()
    {
        _mirrorIsOn = (_mirrorIsOn) ? false : true;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdateMirror");
    }

    public void UpdateMirror()
    {
        for (int index = 0; index < MirrorGroup.Length; ++index)
        {
            if (MirrorGroup[index])
            {
                MirrorGroup[index].SetActive(_mirrorIsOn);
            }
        }
        UpdateToggleScreen();
    }

    public void UpdateToggleScreen()
    {
        _toggleLight.color = (_mirrorIsOn) ? SpotColorStates[ON_STATE] : SpotColorStates[OFF_STATE];
        if (_text)
        {
            _text.color = (_mirrorIsOn) ? ColorStates[ON_STATE] : ColorStates[OFF_STATE];
        }
    }
}
