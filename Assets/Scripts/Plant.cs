using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class Plant : PickedUpItems
{
	public GameObject fruityPlant;
	public GameObject initialPlant;
    public GameObject fruit;
	public GameObject plant;


	//public PlayerComponent m_player;
	//public GameResources.PickedUpItemName tplant;
	//public GameResources.PickedUpItemName tfruit;


	public FruitStatus f_status;

	public enum FruitStatus
	{
		withFruit,
		noFruit
	}


	void Start()
	{
		//GameObject playerGameObject = GameObject.FindWithTag("Player");
		//m_player = playerGameObject.GetComponent<PlayerComponent>();
	}




	void OnMouseDown()
	{
		Debug.Log("click a plant");
        //find local player
        PlayerComponent m_player = GameManagerForNetwork.Instance.LocalPlayer.GetComponent<PlayerComponent>();

		PickedUpItems holdedItem = m_player.GetWhatsInHand();
		if (holdedItem == null)
		{
			if (f_status == FruitStatus.withFruit)
			{                                  //pick up fruit
				pickUpFruit(m_player);

			}
			else if (f_status == FruitStatus.noFruit)
			{
				pickUpPlant(m_player);

			}

		}
		else if (holdedItem.getItemName() == GameResources.PickedUpItemName.POOPOO)
		{

			fertilize();
            m_player.useItemInHand();

		}
		

	}




    private void pickUpFruit(PlayerComponent m_player)
    {
	    Debug.Log("hahahaah");

	    //disable 图标
	    fruityPlant.SetActive(false);
	    initialPlant.SetActive(true);

	    f_status = FruitStatus.noFruit;
        GameObject newFruit = Instantiate(fruit.gameObject);

	    m_player.PickedUp(newFruit.GetComponent<PickedUpItems>());

            //m_player.PickedUp(newFruit.GetComponent<PickedUpItems>());
         //Destroy(newFruit);

        }

	    private void pickUpPlant(PlayerComponent m_player)
    {
	    //disable 图标
	    initialPlant.SetActive(false);
	    Debug.Log("wwwwww");
        //m_player.PickedUp(this);

	    GameObject newPlant = Instantiate(gameObject);

	    m_player.PickedUp(this);


	    //m_player.PickedUp(newPlant.GetComponent<PickedUpItems>());
	    //Debug.Log(newPlant.GetComponent<PickedUpItems>());
        //destroy(plant);


	    }

    public void fertilize()
    {
	    fruityPlant.SetActive(true);
	    initialPlant.SetActive(false);
	    f_status = FruitStatus.withFruit;


    }

    public override PickedUpItemName getItemName()
    {
        return PickedUpItemName.FRUIT_PLANT;
    }

}

	


