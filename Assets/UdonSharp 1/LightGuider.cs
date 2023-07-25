
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LightGuider : UdonSharpBehaviour
{
    public GameObject[] GuideLights;
    public GameObject ReferenceObject;
    public float MaxIntensity;
    public float MinIntensity;

    [UdonSynced]
    private float _lightIntensity = 0f;
    [UdonSynced]
    private int _ownerID = -10;
    private Light[] _guideLights;
    private const int NO_OWNER = -1;
    private const float MAX_DISTANCE = 20.5f;
    private const float MIN_DISTANCE = 7f;

    void Start()
    {
        _guideLights = new Light[GuideLights.Length];
        for (int index = 0; index < GuideLights.Length; ++index)
        {
            var guideLight = GuideLights[index] ? GuideLights[index].GetComponent<Light>() : null;
            if (guideLight)
            {
                _lightIntensity = (_lightIntensity == 0f) ? MinIntensity : _lightIntensity;
                _guideLights[index] = guideLight;
            }
        }
        SetLightIntensity();

        if (_ownerID == -10)
        {
            _ownerID = NO_OWNER;
        }
    }

    public override void OnDeserialization()
    {
        if (_ownerID != NO_OWNER)
        {
            SetLightIntensity();
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (_ownerID == NO_OWNER)
        {
            _ownerID = player.playerId;
            Networking.SetOwner(player, this.gameObject);
        }
    }

    public override void OnPlayerTriggerStay(VRCPlayerApi player)
    {
        if (!Networking.IsOwner(player, this.gameObject) && _ownerID == NO_OWNER)
        {
            _ownerID = player.playerId;
            Networking.SetOwner(player, this.gameObject);
        }

        if (_ownerID == player.playerId)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdateLightIntensity");
        }
    }
    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (_ownerID == player.playerId)
        {
            _ownerID = NO_OWNER;
        }
    }

    public void UpdateLightIntensity()
    {
        var currentDistance = CalculateDistance();
        var varRange = (MaxIntensity - MinIntensity);
        var currentPoint = varRange * currentDistance;
        _lightIntensity = currentPoint + MinIntensity;
        _lightIntensity = Mathf.Max(_lightIntensity, MinIntensity);
        _lightIntensity = Mathf.Min(_lightIntensity, MaxIntensity);
        
        SetLightIntensity();
    }

    private void SetLightIntensity()
    {
        for (int index = 0; index < _guideLights.Length; ++index)
        {
            if (_guideLights[index])
            {
                _guideLights[index].intensity = _lightIntensity;
            }   
        }
    }

    private float CalculateDistance()
    {
        var owner = Networking.GetOwner(this.gameObject);
        float distance = Vector3.Distance(owner.GetPosition(), ReferenceObject.transform.position);
        distance = Mathf.Min(distance, MAX_DISTANCE);
        distance = Mathf.Max(distance, MIN_DISTANCE);

        float relativeDistance = Mathf.InverseLerp(MIN_DISTANCE, MAX_DISTANCE, distance);

        return 1f - relativeDistance;
    }
}
