
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SeatController : UdonSharpBehaviour
{
    public override void Interact()
    {
        Networking.LocalPlayer.UseAttachedStation();
    }
}
