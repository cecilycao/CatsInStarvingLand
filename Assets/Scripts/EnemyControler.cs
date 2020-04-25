using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is for animal behaviour
public class EnemyControler : MonoBehaviourPun, IPunObservable
{
    public float speed = 3 ;
    public float changeDirectionTime =2f;
    public bool isVertical;
    private float changeTimer;
    private Rigidbody2D rbody;
    private Vector2 moveDirection;
    
    private int lastPopo=-1;
    public PlayerComponent m_player;

    public GameObject Fish;
    public GameObject wuping;
    public GameObject fur;
    public GameObject shit;

    //Duration between shitting
    public int shitDuration;
    //first time shit
    public int CreateShitTime = -1;

    public int animalHealth = 1;
    public int aniCurHealth;
    public int Damage = 1;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>(); 
        moveDirection = isVertical? Vector2.up: Vector2.right;
        changeTimer = changeDirectionTime;
        aniCurHealth = animalHealth;
        if (PhotonNetwork.IsMasterClient)
        {
            CreateShitTime = Random.Range(15, 50);
            shitDuration = Random.Range(25, 40);
            photonView.RPC("RpcSetTime", RpcTarget.OthersBuffered, CreateShitTime, shitDuration);
        }
        
    }

    [PunRPC]
    public void RpcSetTime(int createTime, int duration)
    {
        this.CreateShitTime = createTime;
        this.shitDuration = duration;
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
        //Have not set up initial times yet
        if(CreateShitTime == -1)
        {
            return;
        }
        if (lastPopo == -1)
        {
            lastPopo = CreateShitTime;
        }
        if(WorldManager.Instance.getCurrentSecond()- lastPopo >= shitDuration)
        {
            lastPopo = (int)WorldManager.Instance.getCurrentSecond()+ shitDuration; 
            GameObject newShit = Instantiate(shit, transform.position, transform.rotation);
            //Debug.Log("Shit now");
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

            if (pc.m_status == PlayerComponent.PlayerStatus.ATTACK)
            {
                pc.changeHealth(Damage);
                //Destroy(this);
                Debug.Log("扣血");
            }
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
        int randInt = Random.Range(0, 2);
        if (randInt == 0)
        {
            Instantiate(fur, transform.position, transform.rotation);
        }
        else
        {
            Instantiate(wuping, transform.position, transform.rotation);
        }
        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(aniCurHealth);
            //stream.SendNext(lastPopo);
        }
        else
        {
            aniCurHealth = (int)stream.ReceiveNext();
            //lastPopo = (int)stream.ReceiveNext();
        }
    }
}
