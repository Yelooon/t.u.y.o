using UnityEngine;

public class BusEventTester : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private BusBrakeEvent brakeEvent;
    [SerializeField] private BusAirConditioner airConditioner;
    [SerializeField] private MotorcycleRobberyEvent bikeRobbery;
    [SerializeField] private PickpocketEvent pickpocket;

    public void TestBrake()
    {
        if (brakeEvent != null)
        {
            brakeEvent.TriggerBrake();
        }
    }

    public void TestACFailure()
    {
        if (airConditioner != null)
        {
            airConditioner.FailAC();
        }
    }

    public void TestACRestore()
    {
        if (airConditioner != null)
        {
            airConditioner.RestoreAC();
        }
    }

    public void TestBikeRobbery()
    {
        if (bikeRobbery != null)
        {
            bikeRobbery.TriggerRobbery();
        }
    }

    public void TestPickpocket()
    {
        if (pickpocket != null)
        {
            pickpocket.StartPickpocket();
        }
    }
}