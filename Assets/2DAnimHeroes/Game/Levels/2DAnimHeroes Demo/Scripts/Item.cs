using UnityEngine;
using System.Collections;
using System.Linq;

public class Item : MonoBehaviour {
    private Player player;

    public float rotateSpeed = 1.0f;
	public float maxSize = 2.0f;
	public float minSize = 0.1f;
	public float enlargeSpeed = 1.0f;
	public float minimizeSpeed = 1.0f;
    public string itemType = "unarmed";

	private bool enlarge = false;
	private bool minimize = false;
	private bool hit = false;

	// Use this for initialization
	void Start () {
        player = FindObjectsOfType<Player>().Where(x => x.name == "Player").Single();
        

    }

    // Update is called once per frame
    void Update () {
		transform.Rotate (0, 0, rotateSpeed);
		if(enlarge)
		{
            GetComponent<Rigidbody2D>().gravityScale = 0f;
            GetComponent<BoxCollider2D>().enabled = false;
            transform.localScale = Vector3.Lerp (transform.localScale, new Vector3(maxSize, maxSize, maxSize), enlargeSpeed * Time.deltaTime);
			if(transform.localScale.x > maxSize*0.9f)
			{
				minimize = true;
				enlarge = false;
			}
		}
		if(minimize)
		{

			transform.localScale = Vector3.Lerp (transform.localScale, new Vector3(minSize, minSize, minSize), minimizeSpeed * Time.deltaTime);
			if(transform.localScale.x < minSize*1.2f)
				Destroy(gameObject);
		}
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		if(!hit)
		{
			if(collider.name == "Player" )
			{
				enlarge = true;
				hit = true;
				GetComponent<CircleCollider2D>().enabled = false;
                switch (this.itemType)
                {
                    case "Key":
                        {
                            Destroy(GameObject.Find("InvisibleWall0").gameObject);
                            break;
                        }
                    case "Pistol":
                        {
                           player.animation.state.SetAnimation(1, "pistolNearIdle", true);
                            player.combatState = Player.CombatStates.pistol;
                            break;
                        }
                    case "DoubleJump":
                        {
                            player.isDoubleJump = true;
                            break;
                        }
                    case "WallJump":
                        {
                            player.isWallJump = true;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

        }
	}
}
