using UnityEngine;

public class BusTemperatureSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BusAirConditioner airConditioner;
    [SerializeField] private WindowInteractable window;

    [Header("Temperature Threshold")]
    [SerializeField] private float temperatureThreshold = 3f;

    private float currentTimer = 0f;
    private bool temperatureEventTriggered = false;

    private void Update()
    {
        CheckTemperature();
    }

    private void CheckTemperature()
    {
        bool freezingCondition =
            window.IsOpen &&
            airConditioner.IsWorking;

        bool overheatingCondition =
            !window.IsOpen &&
            !airConditioner.IsWorking;

        if (freezingCondition || overheatingCondition)
        {
            currentTimer += Time.deltaTime;

            if (currentTimer >= temperatureThreshold &&
                !temperatureEventTriggered)
            {
                temperatureEventTriggered = true;

                if (freezingCondition)
                {
                    PlayerFreezes();
                }
                else if (overheatingCondition)
                {
                    PlayerOverheats();
                }
            }
        }
        else
        {
            currentTimer = 0f;
            temperatureEventTriggered = false;
        }
    }

    private void PlayerFreezes()
    {
        Debug.Log("🥶 PLAYER SE CONGELA");
    }

    private void PlayerOverheats()
    {
        Debug.Log("🥵 PLAYER SE ASA");
    }
}