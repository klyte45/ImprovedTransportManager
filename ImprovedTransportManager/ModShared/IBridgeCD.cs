using Kwytto.Interfaces;
using UnityEngine;

namespace ImprovedTransportManager.ModShared
{
    public interface IBridgeCD : IBridgePrioritizable
    {
        bool CustomDataAvailable { get; }
        bool GetAddressStreetAndNumber(Vector3 sidewalk, Vector3 midPosBuilding, out int number, out string streetName);
        Texture2D GetLineIcon(ushort lineId);
        void SetLineIcon(ushort lineId, Texture2D newIcon);
        string GetVehicleIdentifier(ushort vehicleId);
    }
}
