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

    public void UpdateHealth(int healthValue)
    {
        Text HealthText = Health.GetComponentInChildren<Text>();
        HealthText.text = "Health: " + healthValue;
    }
    public void UpdateHunger(int hungerValue)
    {
        Text HungerText = Hunger.GetComponentInChildren<Text>();
        HungerText.text = "Hunger: " + hungerValue;
    }
    public void UpdateTemperature(double temperatureValue)
    {
        Text TemperatureText = Temperature.GetComponentInChildren<Text>();
        TemperatureText.text = "Temperature: " + temperatureValue;
    }
    public void UpdateTiredness(int tirednessValue)
    {
        Text TirednessText = Tiredness.GetComponentInChildren<Text>();
        TirednessText.text = "Tiredness: " + TirednessText;
    }
}