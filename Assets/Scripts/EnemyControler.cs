using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is for animal behaviour
public class EnemyControler : MonoBehaviourPun, IPunObservable
{
    public int shitDuration = 30;
    public float speed = 3 ;
    public float changeDirectionTime =2f;
    public bool isVertical;
    private float changeTimer;
    private Rigidbody2D rbody;
    private Vector2 moveDirection;
    
    private int lastPopo=0;
    public PlayerComponent m_player;

    public GameObject Fish;
    public GameObject wuping;
    public GameObject shit;
    private int CreateShitTime;

    public int animalHealth = 1;
    public int aniCurHealth;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>(); 
        moveDirection = isVertical? Vector2.up: Vector2.right;
        changeTimer = changeDirectionTime;
        aniCurHealth = animalHealth;
        CreateShitTime = 0;

    }

    // Update is called once per frame
    void Update()
    {
        changeTimer -= Time.deltaTime;
        if(changeTimer<0){
            moveDirection *=-1;
            changeTimer = changeDirectionTime; 
        }
        Vector2 position = rbody.position;
        position.x += moveDirection.x *  speed * Time.deltaTime;
        position.y += moveDirection.y *  speed * Time.deltaTime;
        rbody.MovePosition(position);
        //Debug.Log(WorldManager.Instance.getCurrentDay());
        if(WorldManager.Instance.getCurrentSecond()- lastPopo >= shitDuration)
        {
            lastPopo = (int)WorldManager.Instance.getCurrentSecond();
            GameObject newShit = Instantiate(shit, transform.position, transform.rotation);
            //Debug.Log("sssssss");
            //CreateShitTime = WorldManager.Instance.getCurrentDay();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            if (aniCurHealth == 0)
            {
                print("I am master, destroy this animal now.....");
                photonView.RPC("RpcAnimalDrop", RpcTarget.AllBuffered);
                PhotonNetwork.Destroy(this.gameObject);
                
            }
        }

    }

    public int ShitTime()
    {
        return CreateShitTime;
    }


    void OnCollisionEnter2D(Collision2D other){
        
        if (other.gameObject.tag == "Player")
        {
            PlayerComponent pc = other.gameObject.GetComponent<PlayerComponent>();

            
                pc.ChangeHealth(-1);
                //Destroy(this);
                Debug.Log("扣血");
            
        }
    }

    public void animalChangeHealth(int amount)
    {
        if (amount < 0)
        {
            Debug.Log("animal" + aniCurHealth + "/" + animalHealth);
            aniCurHealth = Mathf.Clamp(aniCurHealth + amount, 0, animalHealth);
            //UiManager.instance.UpdateHealthbar(currentHealth, maxHealth);
            Debug.Log("animal" +aniCurHealth + "/" + animalHealth);
        }
    }

    [PunRPC]
    public void RpcAnimalDrop()
    {
        Instantiate(wuping, transform.position, transform.rotation);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(aniCurHealth);
        }
        else
        {
            aniCurHealth = (int)stream.ReceiveNext();
        }
    }
}
