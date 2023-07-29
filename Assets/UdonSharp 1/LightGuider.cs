
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
    private int _ownerID;

    public float LightIntensity
    {
        get { return _lightIntensity; }
        set 
        {
            _lightIntensity = value;
            SetLightIntensity();
        }
    }

    public int OwnerID
    {
        get { return _ownerID; }
        set { _ownerID = value; }
    }

    private Light[] _guideLights;
    private const int UNASSIGNED_ID = -1;
    private const float MAX_DISTANCE = 20.5f;
    private const float MIN_DISTANCE = 7f;

    void Start()
    {
        StartUpFlow();
        //_guideLights = new Light[GuideLights.Length];
        //for (int index = 0; index < GuideLights.Length; ++index)
        //{
        //    var guideLight = GuideLights[index] ? GuideLights[index].GetComponent<Light>() : null;
        //    if (guideLight)
        //    {
        //        _lightIntensity = (_lightIntensity == 0f) ? MinIntensity : _lightIntensity;
        //        _guideLights[index] = guideLight;
        //    }
        //}
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            LightIntensity = (LightIntensity == 0f) ? MinIntensity : LightIntensity;
            OwnerID = UNASSIGNED_ID;
            SetLightIntensity();
            RequestSerialization();
        }
        else
        {
            _lightIntensity = (_lightIntensity == 0f) ? MinIntensity : _lightIntensity;
            SetLightIntensity();
        }
    }

    public override void OnDeserialization()
    {
        if (_ownerID != UNASSIGNED_ID)
        {
            SetLightIntensity();
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (OwnerID == UNASSIGNED_ID)
        {
            OwnerID = player.playerId;
            Networking.SetOwner(player, gameObject);
            RequestSerialization();
        }
    }

    public override void OnPlayerTriggerStay(VRCPlayerApi player)
    {
        if (!Networking.IsOwner(player, gameObject) && OwnerID == UNASSIGNED_ID)
        {
            OwnerID = player.playerId;
            Networking.SetOwner(player, gameObject);
        }

        if (OwnerID == player.playerId)
        {
            UpdateLightIntensity();
            RequestSerialization();
        }
    }
    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (OwnerID == player.playerId)
        {
            OwnerID = UNASSIGNED_ID;
        }
    }

    public void UpdateLightIntensity()
    {
        var currentDistance = CalculateDistance();
        var varRange = (MaxIntensity - MinIntensity);
        var currentPoint = varRange * currentDistance;
        var newIntensity  = currentPoint + MinIntensity;
        newIntensity = Mathf.Max(newIntensity, MinIntensity);
        newIntensity = Mathf.Min(newIntensity, MaxIntensity);

        LightIntensity = newIntensity;
    }

    private void SetLightIntensity()
    {
        for (int index = 0; index < _guideLights.Length; ++index)
        {
            if (_guideLights[index])
            {
                _guideLights[index].intensity = LightIntensity;
            }   
        }
    }

    private float CalculateDistance()
    {
        var owner = Networking.GetOwner(gameObject);
        float distance = Vector3.Distance(owner.GetPosition(), ReferenceObject.transform.position);
        distance = Mathf.Min(distance, MAX_DISTANCE);
        distance = Mathf.Max(distance, MIN_DISTANCE);

        float relativeDistance = Mathf.InverseLerp(MIN_DISTANCE, MAX_DISTANCE, distance);

        return 1f - relativeDistance;
    }

    private void StartUpFlow()
    {
        _guideLights = new Light[GuideLights.Length];
        for (int index = 0; index < GuideLights.Length; ++index)
        {
            var guideLight = GuideLights[index] ? GuideLights[index].GetComponent<Light>() : null;
            if (guideLight)
            {
                _guideLights[index] = guideLight;
            }
        }
    }
}
