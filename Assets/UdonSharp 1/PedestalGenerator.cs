
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

    [UdonSynced]
    private bool _pedestalEnabled;
    [UdonSynced]
    private int _activePlayerInTrigger;

    private GameObject _spawnedPedestal;
    private int _localId;
    private const int UNASSIGNED_ID = -1;

    void Start()
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
        _pedestalEnabled = false;
        _activePlayerInTrigger = UNASSIGNED_ID;
        _localId = (Networking.LocalPlayer != null) ? Networking.LocalPlayer.playerId : UNASSIGNED_ID;
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

    public void EnablePedestal()
    {
        if (!_pedestalEnabled && _activePlayerInTrigger != UNASSIGNED_ID)
        {
            TogglePedestal(true);
            ToggleCanvas(true);
        }
    }

    public void DisablePedestal()
    {
        if (_pedestalEnabled && _activePlayerInTrigger == UNASSIGNED_ID)
        {
            TogglePedestal(false);
            ToggleCanvas(false);
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.playerId == _localId && _activePlayerInTrigger == UNASSIGNED_ID)
        {
            _activePlayerInTrigger = player.playerId;
        }
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "EnablePedestal");
    }

    public override void OnPlayerTriggerStay(VRCPlayerApi player)
    {
        if ((_activePlayerInTrigger == UNASSIGNED_ID) && (player.IsValid()))
        {
            _activePlayerInTrigger = player.playerId;
            if (!_pedestalEnabled)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "EnablePedestal");
            }
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player.playerId == _localId && _activePlayerInTrigger == player.playerId)
        {
            _activePlayerInTrigger = UNASSIGNED_ID;
        }
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DisablePedestal");
    }

    //TODO Figure out better way to enable/disable
    public override void OnDeserialization()
    {
        if (_pedestalEnabled)
        {
            _spawnedPedestal.SetActive(_pedestalEnabled);
            TogglePedestalLamps(_pedestalEnabled);
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (_localId == _activePlayerInTrigger)
        {
            _activePlayerInTrigger = UNASSIGNED_ID;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DisablePedestal");
        }
    }
}
