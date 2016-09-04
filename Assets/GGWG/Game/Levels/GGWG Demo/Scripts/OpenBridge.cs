using UnityEngine;
using System.Collections;

public class OpenBridge : MonoBehaviour
{

    private Player player;
    public GameObject door;
    public float moveDistance = 5f;
    public float moveVelocity = 5f;
    private bool move;
    private float originalX;

    // Use this for initialization
    void Start()
    {
        player = FindObjectOfType<Player>();
        originalX = door.transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        if(move)
        door.transform.position = Vector2.MoveTowards(door.transform.position, new Vector2(originalX + moveDistance,door.transform.position.y), moveVelocity * Time.deltaTime);
        if (door.transform.position.x == originalX + moveDistance)
            move = false;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Player")
        {
                move = true;
       
        }
    }
}
