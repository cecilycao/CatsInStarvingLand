using UnityEngine;
using System.Collections;
using static GameResources;

public class CameraFollow : MonoBehaviour
{

    public float velocityFactor = 5f;
    public float lerpStep = 0.25f;
    public float interpVelocity;
    public float minDistance;
    public float followDistance;
    public GameObject target;
    public Vector3 offset;
    Vector3 targetPos;
    public SpriteRenderer background;

    private static CameraFollow instance = null;
    public static CameraFollow Instance { get { return instance; } }

    void Awake()
    {
        // Singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    // Use this for initialization
    void Start()
    {
        targetPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        target = GameManagerForNetwork.Instance.LocalPlayer.gameObject;
        if (target)
        {
            Vector3 posNoZ = transform.position;
            posNoZ.z = target.transform.position.z;

            Vector3 targetDirection = (target.transform.position - posNoZ);

            interpVelocity = targetDirection.magnitude * velocityFactor;

            targetPos = transform.position + (targetDirection.normalized * interpVelocity * Time.deltaTime);

            transform.position = Vector3.Lerp(transform.position, targetPos + offset, lerpStep);

        }
    }

    public void changeBg(LandType land)
    {
        if(land == LandType.GREENLAND)
        {
            background.sprite = Resources.Load<Sprite>("Bg/greenland_bg");
            AudioManager.instance.changeBg("greenlandbg");
        } else if(land == LandType.RUINLAND)
        {
            background.sprite = Resources.Load<Sprite>("Bg/ruinland_bg");
            AudioManager.instance.changeBg("ruinlandbg");
        } else
        {
            background.sprite = Resources.Load<Sprite>("Bg/sandland_bg");
            AudioManager.instance.changeBg("sandlandbg");
        }
    }
}