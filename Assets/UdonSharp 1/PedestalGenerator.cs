
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PedestalGenerator : UdonSharpBehaviour
{
    public GameObject PedestalPrefab;
    public GameObject PedestalSpawn;
    public GameObject PedestalUnderLight;
    public GameObject PedestalSpotLight;
    public GameObject Canvas;

    [UdonSynced, FieldChangeCallback(nameof(PedestalEnabled))]
    private bool _pedestalEnabled;
    [UdonSynced, FieldChangeCallback(nameof(ActivePlayerInTrigger))]
    private int _activePlayerInTrigger;

    public bool PedestalEnabled
    {
        get { return _pedestalEnabled; }
        set 
        { 
            _pedestalEnabled = value;
            TogglePedestal(_pedestalEnabled);
            TogglePedestalLamps(_pedestalEnabled);
            ToggleCanvas(_pedestalEnabled);
        }
    }

    public int ActivePlayerInTrigger
    {
        get { return _activePlayerInTrigger; }
        set { _activePlayerInTrigger = value; }
    }


    private GameObject _spawnedPedestal;
    private int _localId;
    private const int UNASSIGNED_ID = -1;

    void Start()
    {
        StartUpFlow();
        _localId = (Networking.LocalPlayer != null) ? Networking.LocalPlayer.playerId : UNASSIGNED_ID;

        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            PedestalEnabled = false;
            ActivePlayerInTrigger = UNASSIGNED_ID;
            RequestSerialization();
        }
    }

    private void StartUpFlow()
    {
        if (PedestalUnderLight && PedestalSpotLight)
        {
            TogglePedestalLamps(false);
        }

        if (Canvas && Canvas.activeInHierarchy)
        {
            Canvas.SetActive(false);
        }

        CreatePedestal();
        InjectSpawnPoint();
        _spawnedPedestal.SetActive(false);
    }

    private void CreatePedestal()
    {
        _spawnedPedestal = GameObject.Instantiate(PedestalPrefab, PedestalSpawn.transform, false);
        _spawnedPedestal.name = "active_pedestal";
    }

    private void InjectSpawnPoint()
    {
        var dinoSpawnRef = _spawnedPedestal.GetComponentInChildren<Transform>();
        var uiSystem = Canvas.GetComponent<UISystem>();
        uiSystem.SpawnPoint = dinoSpawnRef.gameObject;
    }

    private void TogglePedestalLamps(bool value)
    {
        PedestalUnderLight.SetActive(value);
        PedestalSpotLight.SetActive(value);
    }

    private void TogglePedestal(bool value)
    {
        _pedestalEnabled = value;
        _spawnedPedestal.SetActive(_pedestalEnabled);
        TogglePedestalLamps(_pedestalEnabled);
    }

    private void ToggleCanvas(bool value)
    {
        Canvas.SetActive(value);
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.IsValid() && ActivePlayerInTrigger == UNASSIGNED_ID && !PedestalEnabled)
        {
            Networking.SetOwner(player, gameObject);
            PedestalEnabled = true;
            ActivePlayerInTrigger = player.playerId;

            RequestSerialization();
        }
    }

    public override void OnPlayerTriggerStay(VRCPlayerApi player)
    {
        if ((ActivePlayerInTrigger == UNASSIGNED_ID) && (player.IsValid()) && (!PedestalEnabled))
        {
            Networking.SetOwner(player, gameObject);
            PedestalEnabled = true;
            ActivePlayerInTrigger = player.playerId;

            RequestSerialization();
        }
        else if (player.IsValid() && ActivePlayerInTrigger == UNASSIGNED_ID)
        {
            Networking.SetOwner(player, gameObject);
            ActivePlayerInTrigger = player.playerId;

            RequestSerialization();
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player.IsValid() && PedestalEnabled && ActivePlayerInTrigger == player.playerId)
        {
            PedestalEnabled = false;
            ActivePlayerInTrigger = UNASSIGNED_ID;

            RequestSerialization();
        }
    }

    //TODO Figure out better way to enable/disable
    public override void OnDeserialization()
    {
        Debug.Log($"[{Networking.LocalPlayer.playerId}] received deserialization on pedestal generator");
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (_localId == ActivePlayerInTrigger)
        {
            ActivePlayerInTrigger = UNASSIGNED_ID;
            if (PedestalEnabled)
            {
                PedestalEnabled = false;
            }
            RequestSerialization();
        }
    }
}
