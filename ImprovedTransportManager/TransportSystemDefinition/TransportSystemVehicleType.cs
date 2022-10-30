extern alias UUI;

namespace ImprovedTransportManager.TransportSystems
{
    public enum TransportSystemVehicleType : uint
    {
        None = ~0u & TransportSystemTypeExtensions.BITMASK_VEHICLE_TYPE_NTH_BIT,
        Car = 0,
        Metro,
        Train,
        Ship,
        Plane,
        Bicycle,
        Tram,
        Helicopter,
        Meteor,
        Vortex,
        Ferry,
        Monorail,
        CableCar,
        Blimp,
        Balloon,
        Rocket,
        Trolleybus,
        TrolleybusLeftPole,
        TrolleybusRightPole,
    }
}
