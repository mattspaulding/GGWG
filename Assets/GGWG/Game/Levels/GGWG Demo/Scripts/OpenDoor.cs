using UnityEngine;
using System.Collections;

public class OpenDoor : MonoBehaviour
{

    private Player player;
    public GameObject door;
    public float moveDistance = 5f;
    public float moveVelocity = 5f;
    private bool move;
    private float originalHeight;

    // Use this for initialization
    void Start()
    {
        player = FindObjectOfType<Player>();
        originalHeight = door.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if(move)
        door.transform.position = Vector2.MoveTowards(door.transform.position, new Vector2(door.transform.position.x, originalHeight + moveDistance), moveVelocity * Time.deltaTime);
        if (door.transform.position.y == originalHeight + moveDistance)
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
