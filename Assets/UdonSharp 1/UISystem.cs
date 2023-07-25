
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
    public AnimationManipulator Die;
    public Transform DieRespawn;
    public GameObject ActiveSideObject;

    [UdonSynced]
    private int _selectedDino;
    [UdonSynced]
    private bool _dinoVisible;

    private GameObject _loadedDino;
    private Dropdown _dropDown;
    private Toggle _toggle;
    private Text _activeSide;

    void Start()
    {
        _selectedDino = 0;
        _dinoVisible = false;

        if (Dropdown)
        {
            _dropDown = Dropdown.GetComponent<Dropdown>();
        }

        if (Toggle)
        {
            _toggle = Toggle.GetComponent<Toggle>();
        }

        if (ActiveSideObject)
        {
            _activeSide = ActiveSideObject.GetComponent<Text>();
        }
    }

    void LateUpdate()
    {
        if ((_activeSide != null) && (Die))
        {
            _activeSide.text = $"{Die.ActiveSide}";
        }
    }

    public void OnDropDownChanged(int value)
    {
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
            if (Die)
            {
                Die.DinoToAnimate = _loadedDino.GetComponent<Animator>();
                Die.UpdateAnimation();
            }
        }
    }

    public void RemoveDino()
    {
        if (_loadedDino != null)
        {
            GameObject.Destroy(_loadedDino);
        }
    }

    public void ShowDino()
    {
        _dinoVisible = true;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "LoadDino");
    }

    public void HideDino()
    {
        _dinoVisible = false;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RemoveDino");
    }

    public void RetrieveDie()
    {
        Die.transform.position = DieRespawn.position;
    }
}
