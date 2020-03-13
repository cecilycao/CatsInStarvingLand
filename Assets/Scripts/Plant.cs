using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class Plant : PickedUpItems
{
	public GameObject fruityPlant;
	public GameObject initialPlant;
    public GameObject fruit;
	//public GameObject plant;


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

            photonView.RPC("Rpcfertilize", RpcTarget.AllBuffered);
            m_player.useItemInHand();

		}
		

	}




    private void pickUpFruit(PlayerComponent m_player)
    {


        GameObject newFruit = Instantiate(fruit.gameObject);
        m_player.PickedUp(newFruit.GetComponent<PickedUpItems>());
        photonView.RPC("RpcPickUpFruit", RpcTarget.AllBuffered);


    }

    [PunRPC]
    public void RpcPickUpFruit()
    {
        //disable 图标
        fruityPlant.SetActive(false);
        initialPlant.SetActive(true);

        f_status = FruitStatus.noFruit;
        
    }

    
	    private void pickUpPlant(PlayerComponent m_player)
    {
	    

	    //GameObject newPlant = Instantiate(gameObject);
	    m_player.PickedUp(this);
        photonView.RPC("RpcPickUpPlant", RpcTarget.AllBuffered);



    }

    [PunRPC]
    public void RpcPickUpPlant()
    {
        //disable 图标
        initialPlant.SetActive(false);

    }

    [PunRPC]
    public void Rpcfertilize()
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

	


