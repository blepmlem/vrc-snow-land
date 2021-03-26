using UdonSharp;
using VRC.SDKBase;

public class SnowboardMovement_Station_Button : UdonSharpBehaviour
{

    public VRCStation station;
    public SnowboardMovement_Station snowboardMovement;

    public override void Interact()
    {
        station.UseStation(Networking.LocalPlayer);
        snowboardMovement.SnowboardEntered(Networking.LocalPlayer, station);
    }
}
