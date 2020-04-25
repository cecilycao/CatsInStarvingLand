using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;
using UnityEngine.EventSystems;

public class Plant : PickedUpItems, IPointerClickHandler
{
	public GameObject fruityPlant;
	public GameObject initialPlant;
    public GameObject fruit;
    public Vector2Int index;


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

    public void setIndex(int x, int y)
    {
        photonView.RPC("RpcSetIndex", RpcTarget.AllBuffered, x, y);
    }

    [PunRPC]
    public void RpcSetIndex(int x, int y)
    {
        index.x = x;
        index.y = y;
    }

        public void setFruitStatus(bool hasFruit)
    {
        photonView.RPC("RpcSetFruitStatus", RpcTarget.AllBuffered, hasFruit);
    }

    [PunRPC]
    public void RpcSetFruitStatus(bool hasFruit)
    {
        if (hasFruit)
        {
            f_status = FruitStatus.withFruit;
            fruityPlant.SetActive(true);
            initialPlant.SetActive(false);
        }
        else
        {
            f_status = FruitStatus.noFruit;
            fruityPlant.SetActive(false);
            initialPlant.SetActive(true);
        }
    }


	public void OnPointerClick(PointerEventData eventData)
    {
        if(m_State != ItemState.DEFAULT)
        {
            return;
        }
		Debug.Log("click a plant");
        //find local player
        PlayerComponent m_player = GameManagerForNetwork.Instance.LocalPlayer.GetComponent<PlayerComponent>();

		PickedUpItems holdedItem = m_player.GetWhatsInHand();
        if (holdedItem != null)
		{
            if (holdedItem.getItemName() == GameResources.PickedUpItemName.POOPOO)
            {
                photonView.RPC("Rpcfertilize", RpcTarget.AllBuffered);
                m_player.useItemInHand();
                return;
            }

		} 

        if (f_status == FruitStatus.withFruit)
        {                                  //pick up fruit
            pickUpFruit(m_player);

        }
        else if (f_status == FruitStatus.noFruit)
        {
            pickUpPlant(m_player);

        }

            
        
		

	}




    private void pickUpFruit(PlayerComponent m_player)
    {
      
        photonView.RPC("RpcPickUpFruit", RpcTarget.AllBuffered, m_player.m_ID);


    }

    [PunRPC]
    public void RpcPickUpFruit(int playerID)
    {
        PlayerComponent m_player = WorldManager.Instance.getPlayer(playerID);
        GameObject newFruit = Instantiate(fruit.gameObject);
        m_player.PickedUp(newFruit.GetComponent<PickedUpItems>());

        //change plant sprite
        fruityPlant.SetActive(false);
        initialPlant.SetActive(true);

        f_status = FruitStatus.noFruit;
    }

    
	    private void pickUpPlant(PlayerComponent m_player)
    {
	    

	    //GameObject newPlant = Instantiate(gameObject);
	    //m_player.PickedUp(this);
        photonView.RPC("RpcPickUpPlant", RpcTarget.AllBuffered, m_player.m_ID);
      

    }

    [PunRPC]
    public void RpcPickUpPlant(int playerID)
    {
        PlayerComponent m_player = WorldManager.Instance.getPlayer(playerID);
        m_player.PickedUp(this);
        //disable 图标
        //initialPlant.SetActive(false);
        WorldManager.Instance.clearObjectAt(index.x, index.y);
        //gameObject.SetActive(false);
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

    public override GameObject getGameObj()
    {
        return gameObject;
    }
}

	


