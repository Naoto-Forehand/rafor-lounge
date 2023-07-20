
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

    [UdonSynced]
    private bool _pedestalEnabled;
    [UdonSynced]
    private int _activePlayersInTrigger;

    private bool _amIInTrigger;
    private GameObject _spawnedPedestal;
    private int _localId;

    void Start()
    {
        if (PedestalUnderLight && PedestalSpotLight)
        {
            TogglePedestalLamps(false);
        }

        CreatePedestal();
        _spawnedPedestal.SetActive(false);
        _pedestalEnabled = false;
        _activePlayersInTrigger = 0;
        _amIInTrigger = false;
        _localId = Networking.LocalPlayer.playerId;
    }

    void LateUpdate()
    {
        if (_activePlayersInTrigger <= 0 && _pedestalEnabled)
        {
            Debug.Log($"[PedestalGenerator] Turning Off Pedestal {_activePlayersInTrigger}");
            //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "TogglePedestal");
        }
    }

    private void CreatePedestal()
    {
        _spawnedPedestal = GameObject.Instantiate(PedestalPrefab, PedestalSpawn.transform, false);
        _spawnedPedestal.name = "active_pedestal";
    }

    private void TogglePedestalLamps(bool value)
    {
        PedestalUnderLight.SetActive(value);
        PedestalSpotLight.SetActive(value);
    }

    private void UpdateOwner()
    {
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
    }

    private void PedestalManagement(bool value)
    {
        _pedestalEnabled = value;
        _spawnedPedestal.SetActive(_pedestalEnabled);
        TogglePedestalLamps(_pedestalEnabled);
    }

    public void EnablePedestal()
    {
        ++_activePlayersInTrigger;
        if (!_pedestalEnabled)
        {
            PedestalManagement(true);   
        }
    }

    public void DisablePedestal()
    {
        --_activePlayersInTrigger;
        if (_pedestalEnabled && _activePlayersInTrigger < 1)
        {
            PedestalManagement(false);
        }
    }

    public void TogglePedestal()
    {
        Debug.Log($"[PEDESTAL] TOGGLING {_pedestalEnabled}");
        _pedestalEnabled = (_pedestalEnabled) ? false : true;
        _spawnedPedestal.SetActive(_pedestalEnabled);
        TogglePedestalLamps(_pedestalEnabled);
    }

    public void PlayerLeft()
    {
        Debug.Log("[PedestalGenerator] PlayerLeft");
    }

    public void IncrementPlayers()
    {
        ++_activePlayersInTrigger;
    }

    public void DecrementPlayers()
    {
        --_activePlayersInTrigger;
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        Debug.Log($"[ENTER] {player.playerId} send increment");
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "EnablePedestal");
        //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "IncrementPlayers");
        //_amIInTrigger = true;
        //if (_activePlayersInTrigger > 0 && !_pedestalEnabled)
        //{
        //    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "TogglePedestal");
        //}
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DisablePedestal");
        //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DecrementPlayers");
        //_amIInTrigger = false;
    }

    public override void OnDeserialization()
    {
        if (_pedestalEnabled && _activePlayersInTrigger > 0)
        {
            _spawnedPedestal.SetActive(_pedestalEnabled);
            TogglePedestalLamps(_pedestalEnabled);
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        Debug.Log($"[PEDESTAL] player left {(player != null)}");
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PlayerLeft");
        //if (player.isLocal && _amIInTrigger)
        //{
        //    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DecrementPlayers");
        //}
    }
}
