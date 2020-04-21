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

    private int currentHealthLevel = 4;
    private int currentHungerLevel = 4;
    private int currentTempLevel = 1;


    public void UpdateHealth(int healthValue)
    {
        Text HealthText = Health.GetComponentInChildren<Text>();
        HealthText.text = healthValue + "/100";
        int newHealthLevel = healthValue / 20;
        if (newHealthLevel != currentHealthLevel)
        {
            currentHealthLevel = newHealthLevel;
            ChangeHealthImg(currentHealthLevel);
        }
    }
    public void UpdateHunger(int hungerValue)
    {
        Text HungerText = Hunger.GetComponentInChildren<Text>();
        HungerText.text = hungerValue + "/100";

        int newHungerLevel = hungerValue / 20;
        if(newHungerLevel != currentHungerLevel)
        {
            currentHungerLevel = newHungerLevel;
            ChangeHungryImg(currentHungerLevel);
        }
    }
    public void UpdateTemperature(double temperatureValue)
    {
        Text TemperatureText = Temperature.GetComponentInChildren<Text>();
        int i = (int)(temperatureValue * 100);
        temperatureValue = (float)(i * 1.0) / 100;//保留小数后2位
        TemperatureText.text = temperatureValue + "/38";

        int newTempLevel = 1;
        if(temperatureValue < 37)
        {
            newTempLevel = 0;
        } else if (temperatureValue > 39)
        {
            newTempLevel = 2;
        }
        if(newTempLevel != currentTempLevel)
        {
            currentTempLevel = newTempLevel;
            ChangeTempImg(currentTempLevel);
        }
    }
    public void UpdateTiredness(int tirednessValue)
    {
        Text TirednessText = Tiredness.GetComponentInChildren<Text>();
        TirednessText.text = "Tiredness: " + tirednessValue;
    }

    //level: 0 - 4
    public void ChangeHealthImg(int level)
    {
        if (level > 4)
            level = 4;
        if (level < 0)
            level = 0;
        Debug.Log("Change Health Image: " + level);
        Image HealthImg = Health.GetComponent<Image>();
        HealthImg.sprite = Resources.Load<Sprite>("UI/MainGame/HeadUI/Health/" + level);
    }

    //level: 0 - 4
    public void ChangeHungryImg(int level)
    {
        if (level > 4)
            level = 4;
        if (level < 0)
            level = 0;
        Debug.Log("Change Hunger Image: " + level);
        Image HungerImg = Hunger.GetComponent<Image>();
        HungerImg.sprite = Resources.Load<Sprite>("UI/MainGame/HeadUI/Hungry/" + level);
    }

    //level: 0 - 2
    public void ChangeTempImg(int level)
    {
        Debug.Log("Change Temperature Image: " + level);
        Image TempImg = Temperature.GetComponent<Image>();
        TempImg.sprite = Resources.Load<Sprite>("UI/MainGame/HeadUI/Temperature/" + level);
    }
}