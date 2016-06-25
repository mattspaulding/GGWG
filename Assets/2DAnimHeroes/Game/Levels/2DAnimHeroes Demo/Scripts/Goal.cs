using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {

	private Player player;
	private bool hit = false;
	public Color color;

	// Use this for initialization
	void Start ()
	{
		player = FindObjectOfType<Player>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		if(!hit)
		{
			if(collider.tag == "Player")
			{
				player.SetCurrentState(Player.PlayerStates.celebration);
				GetComponent<SpriteRenderer>().color = color;
				hit = true;
			}
		}
	}
}
