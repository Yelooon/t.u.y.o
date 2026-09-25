using UnityEngine;

public class BusLaterEvenTesting : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private GreedyVendorEvent vendorEvent;
    [SerializeField] private UFOConspiracyEvent ufoEvent;

    public void TestVendor()
    {
        if (vendorEvent != null)
        {
            vendorEvent.StartVendorEvent();
        }
    }

    public void TestUFO()
    {
        if (ufoEvent != null)
        {
            ufoEvent.StartConspiracyEvent();
        }
    }
    
}
