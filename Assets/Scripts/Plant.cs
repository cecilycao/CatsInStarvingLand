using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : PickedUpItems
{
	public GameObject fruityPlant;
	public GameObject initialPlant;
	public PlayerComponent m_player;
	public GameResources.PickedUpItemName tplant;
	public GameResources.PickedUpItemName tfruit;


	public FruitStatus f_status;

	public enum FruitStatus
	{
		withFruit,
		noFruit
	}


	void Start()
	{
		GameObject playerGameObject = GameObject.FindWithTag("Player");
		m_player = playerGameObject.GetComponent<PlayerComponent>();
	}




	void OnMouseDown()
	{
		Debug.Log("click a plant");
		PickedUpItems holdedItem = m_player.GetWhatsInHand();
		if (holdedItem == null)
		{
			if (f_status == FruitStatus.withFruit)
			{                                  //pick up fruit
				pickUpFruit();

			}
			else if (f_status == FruitStatus.noFruit)
			{
				pickUpPlant();

			}

		}
		else if (holdedItem.getItemName() == GameResources.PickedUpItemName.POOPOO)
		{

			fertilize();
            

		}
		

	}




private void pickUpFruit()
{
	Debug.Log("hahahaah");

	//disable 图标
	fruityPlant.SetActive(false);
	initialPlant.SetActive(true);

	f_status = FruitStatus.noFruit;

	//if (m_player.myBackpack.AddNewItem(GameResources.PickedUpItemName.FRUIT))
	//{
	//	Destroy(fruityPlant);
	//}
	//else
	//{
	//	Debug.Log("Add item fail!");
	//}


}

private void pickUpPlant()
{
	//disable 图标
	initialPlant.SetActive(false);
	Debug.Log("wwwwww");

	//if (m_player.myBackpack.AddNewItem(GameResources.PickedUpItemName.PLANT))
	//{
	//	Destroy(initialPlant);
	//}
	//else
	//{
	//	Debug.Log("Add item fail!");

	//}

}

private void fertilize()
{
	fruityPlant.SetActive(true);
	initialPlant.SetActive(false);
	f_status = FruitStatus.withFruit;


}



}

	


