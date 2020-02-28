using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControler : MonoBehaviour
{
    public float speed = 3 ;
    public float changeDirectionTime =2f;
    public bool isVertical;
    private float changeTimer;
    private Rigidbody2D rbody;
    private Vector2 moveDirection;

    public PlayerComponent m_player;

    public GameObject Fish;
    public GameObject Dryfish;

    public GameResources.PickedUpItemName tplant;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>(); 
        moveDirection = isVertical? Vector2.up: Vector2.right;
        changeTimer = changeDirectionTime;
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
    }

    void OnCollisionEnter2D(Collision2D other){
        PlayerComponent pc = other.gameObject.GetComponent<PlayerComponent>();
        if(pc!= null){
             pc.ChangeHealth(-1);
            //Instantiate(Dryfish, other.transform.position, transform.rotation);
            //Destroy(this);
            Debug.Log("扣血");
        }

    }
}
