
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class UISystem : UdonSharpBehaviour
{
    public GameObject Dropdown;
    public GameObject Toggle;
    public GameObject SpawnPoint;
    public GameObject[] Dinos;

    [UdonSynced]
    private int _selectedDino;
    [UdonSynced]
    private bool _dinoVisible;

    private GameObject _loadedDino;
    private Dropdown _dropDown;
    private Toggle _toggle;

    void Start()
    {
        _selectedDino = 0;
        _dinoVisible = false;

        if (Dropdown)
        {
            _dropDown = Dropdown.GetComponent<Dropdown>();
        //    _dropDown.onValueChanged.AddListener(OnDropDownChanged);
        }

        if (Toggle)
        {
            _toggle = Toggle.GetComponent<Toggle>();
        //    _toggle.onValueChanged.AddListener(OnToggleChange);
        }
        //Dropdown.onValueChanged.AddListener(OnDropDownChanged);
        //Toggle.onValueChanged.AddListener(OnToggleChange);
    }

    public void OnDropDownChanged(int value)
    {
        Debug.Log("[UI] OnDropDownChanged");
        if (value < Dinos.Length && value >= 0)
        {
            _selectedDino = value;
        }

        if (_dinoVisible)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "LoadDino");
        }
    }

    public void UpdateToggle()
    {
        OnToggleChange(_toggle.isOn);
    }

    public void UpdateDropDown()
    {
        OnDropDownChanged(_dropDown.value);
    }

    public void OnToggleChange(bool value)
    {
        Debug.Log("[UI] OnToggleChange");
        if (value)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ShowDino");
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "HideDino");
        }
    }

    public void LoadDino()
    {
        Debug.Log("[UI] LOAD DINO");
        if (_loadedDino != null)
        {
            GameObject.Destroy(_loadedDino);
        }

        if (SpawnPoint)
        {
            _loadedDino = GameObject.Instantiate(Dinos[_selectedDino], SpawnPoint.transform, false);
            var localPos = _loadedDino.transform.localPosition;
            localPos.y += 0.2f;
            _loadedDino.transform.localPosition = localPos;
        }
    }

    public void RemoveDino()
    {
        Debug.Log("[UI] REMOVE DINO");
        if (_loadedDino != null)
        {
            GameObject.Destroy(_loadedDino);
        }
    }

    public void ShowDino()
    {
        Debug.Log("[UI] SHOW DINO");
        _dinoVisible = true;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "LoadDino");
    }

    public void HideDino()
    {
        Debug.Log("[UI] HIDE DINO");
        _dinoVisible = false;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RemoveDino");
    }
}
