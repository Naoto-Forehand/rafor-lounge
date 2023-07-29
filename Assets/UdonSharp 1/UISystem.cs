
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

    [UdonSynced, FieldChangeCallback(nameof(SelectedDino))]
    private int _selectedDino;
    [UdonSynced, FieldChangeCallback(nameof(DinoVisible))]
    private bool _dinoVisible;

    public int SelectedDino
    {
        get { return _selectedDino; }
        set 
        { 
            _selectedDino = value;
            if (DinoVisible)
            {
                LoadDino();
            }
            //UpdateUICanvas();
        }
    }

    public bool DinoVisible
    {
        get { return _dinoVisible; }
        set 
        { 
            _dinoVisible = value;
            ToggleDino();
            //UpdateUICanvas();
        }
    }


    private GameObject _loadedDino;
    private Dropdown _dropDown;
    private Toggle _toggle;
    private Text _activeSide;

    void Start()
    {
        StartUpFlow();

        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            DinoVisible = false;
            SelectedDino = 0;

            RequestSerialization();
        }
        else
        {
            _selectedDino = 0;
            _dinoVisible = false;
        }

        Debug.Log($"[{Networking.LocalPlayer.playerId}] ui {SelectedDino} enabled {DinoVisible}");
    }

    private void StartUpFlow()
    {
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

    private void ToggleDino()
    {
        if (DinoVisible)
        {
            LoadDino();
        }
        else
        {
            RemoveDino();
        }
    }

    private void UpdateUICanvas()
    {
        _dropDown.value = SelectedDino;
        _toggle.isOn = DinoVisible;
        if (_activeSide.text != $"{Die.ActiveSide}")
        {
            _activeSide.text = $"{Die.ActiveSide}";
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
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            SelectedDino = value;
            RequestSerialization();
        }

        //if (_dinoVisible)
        //{
        //    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "LoadDino");
        //}
    }

    public override void OnDeserialization()
    {
        UpdateUICanvas();
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
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        DinoVisible = value;
        RequestSerialization();
        //if (value)
        //{
        //    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ShowDino");
        //}
        //else
        //{
        //    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "HideDino");
        //}
    }

    public void LoadDino()
    {
        if (_loadedDino != null)
        {
            GameObject.Destroy(_loadedDino);
        }

        //if (_dropDown.value != _selectedDino)
        //{
        //    _dropDown.value = _selectedDino;
        //}

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

    //public void ShowDino()
    //{
    //    _dinoVisible = true;
    //    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "LoadDino");
    //}

    //public void HideDino()
    //{
    //    _dinoVisible = false;
    //    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RemoveDino");
    //}

    public void RetrieveDie()
    {
        Debug.Log($"[{Networking.LocalPlayer.playerId}] is requesting to retrieve the Die");
        Networking.SetOwner(Networking.LocalPlayer, Die.gameObject);
        //Die.transform.position = DieRespawn.position;
        Die.SnapTo(DieRespawn);
        RequestSerialization();
    }
}
