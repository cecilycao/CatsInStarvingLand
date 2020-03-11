using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slime : MonoBehaviour
{
    public float SlimeSpeed = 3;
    public float SlimeChangeDirectionTime = 1f;
    public bool isVertical;
    private float changeTimer;
    private Rigidbody2D rbody;
    private Vector2 moveDirection;

    public GameObject meiqiguan;
    public GameObject nextPoopoo;
   

    public int SlimeHealth = 3;
    public int SlimeCurHealth;
    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        moveDirection = isVertical ? Vector2.up : Vector2.right;
        changeTimer = SlimeChangeDirectionTime;
        SlimeCurHealth = SlimeHealth;
    }

    // Update is called once per frame
    void Update()
    {
        changeTimer -= Time.deltaTime;
        if (changeTimer < 0)
        {
            moveDirection *= -1;
            changeTimer = SlimeChangeDirectionTime;
        }
        Vector2 position = rbody.position;
        position.x += moveDirection.x * SlimeSpeed * Time.deltaTime;
        position.y += moveDirection.y * SlimeSpeed * Time.deltaTime;
        rbody.MovePosition(position);
    }

    void OnCollisionEnter2D(Collision2D other)
    {

        if (other.gameObject.tag == "Player")
        {
            PlayerComponent pc = other.gameObject.GetComponent<PlayerComponent>();

            pc.ChangeHealth(-2);
            //Destroy(this);
            Debug.Log("Slime扣血");
            
        }
    }

    public void slimeChangeHealth(int amount)
    {
        if (amount < 0)
        {
            Debug.Log("slime" + SlimeCurHealth + "/" + SlimeHealth);
            SlimeCurHealth = Mathf.Clamp(SlimeCurHealth + amount, 0, SlimeHealth);
            //UiManager.instance.UpdateHealthbar(currentHealth, maxHealth);
            Debug.Log("animal" + SlimeCurHealth + "/" + SlimeHealth);
            if (SlimeCurHealth == 0)
            {
                Destroy(this.gameObject);
                int a = Random.Range(1,100);
                if (a <= 5)
                {
                    GameObject meiqi = Instantiate(meiqiguan, transform.position, transform.rotation);
                }
                else
                {
                    GameObject nextpopo = Instantiate(nextPoopoo, transform.position, transform.rotation);
                }
               
            }
        }
    }
}
