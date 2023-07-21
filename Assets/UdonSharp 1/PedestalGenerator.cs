
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
    private int _activePlayersInTrigger;

    private GameObject _spawnedPedestal;
    private int _localId;
    private bool _localInTrigger;

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
        _activePlayersInTrigger = 0;
        _localId = Networking.LocalPlayer.playerId;
        _localInTrigger = false;
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

    public void AddPlayer()
    {
        ++_activePlayersInTrigger;
    }

    public void RemovePlayer()
    {
        --_activePlayersInTrigger;
    }

    public void EnablePedestal()
    {
        //Debug.Log($"[PEDESTAL] {_localId} ENABLE players PRE in trigger {_activePlayersInTrigger}");
        //++_activePlayersInTrigger;
        //Debug.Log($"[PEDESTAL] {_localId} ENABLE players POST in trigger {_activePlayersInTrigger}");
        if (!_pedestalEnabled)
        {
            TogglePedestal(true);
            ToggleCanvas(true);
        }
    }

    public void DisablePedestal()
    {
        //Debug.Log($"[PEDESTAL] {_localId} DISABLE players PRE in trigger {_activePlayersInTrigger}");
        //--_activePlayersInTrigger;
        //Debug.Log($"[PEDESTAL] {_localId} DISABLE players POST in trigger {_activePlayersInTrigger}");
        if (_pedestalEnabled && _activePlayersInTrigger < 1)
        {
            TogglePedestal(false);
            ToggleCanvas(false);
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        Debug.Log($"[ENTER] {player.playerId} send increment");
        if (player.playerId == _localId)
        {
            Debug.Log($"[PEDESTAL] {_localId} entered trigger");
            _localInTrigger = true;
            ++_activePlayersInTrigger;
        }
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "EnablePedestal");
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player.playerId == _localId)
        {
            Debug.Log($"[PEDESTAL] {_localId} exited trigger");
            _localInTrigger = false;
            --_activePlayersInTrigger;
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

    //TODO Figure out better how to handle players leaving game
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        Debug.Log($"[PEDESTAL] player left {(player != null)}");
        Debug.Log($"[PEDESTAL] [{_localId}] {_localInTrigger}");
        if (_localInTrigger)
        {
            _localInTrigger = false;
            --_activePlayersInTrigger;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DisablePedestal");
        }
    }
}
