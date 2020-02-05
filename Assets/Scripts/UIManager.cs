using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//A client script
public class UIManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Health;
    public GameObject Hunger;
    public GameObject Temperature;
    public GameObject Tiredness;

    void UpdateHealth(int healthValue)
    {
        Text HealthText = Health.GetComponentInChildren<Text>();
        HealthText.text = "Health: " + healthValue;
    }
    void UpdateHunger(int hungerValue)
    {
        Text HungerText = Hunger.GetComponentInChildren<Text>();
        HungerText.text = "Hunger: " + hungerValue;
    }
    void UpdateTemperature(int temperatureValue)
    {
        Text TemperatureText = Temperature.GetComponentInChildren<Text>();
        TemperatureText.text = "Temperature: " + temperatureValue;
    }
    void UpdateTiredness(int tirednessValue)
    {
        Text TirednessText = Tiredness.GetComponentInChildren<Text>();
        TirednessText.text = "Tiredness: " + TirednessText;
    }
}
