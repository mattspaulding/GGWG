using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

	public int health = 10;
	public int gun = 0;
	public Transform pointA;
	public Transform pointB;
	public float moveSpeed = 2;
	public string moveAnim = "walk2";

	private bool dead = false;
	private bool hitAnim = false;
	private bool reverse = false;
	private float hitMultiplier;
	private Quaternion flippedRotation = Quaternion.Euler(0, 180, 0);
	private Rigidbody2D body;
	private Spine.Unity.SkeletonAnimation animation;

	// Use this for initialization
	void Start ()
	{
		body = GetComponent<Rigidbody2D>();
		animation = GetComponent<Spine.Unity.SkeletonAnimation>();
		/*
		switch(gun)
		{
		case 0:
			animation.state.SetAnimation(1, "meleeIdle", true);
			break;
		case 1:
			animation.state.SetAnimation(1, "pistolNearIdle", true);
			break;
		case 2:
			animation.state.SetAnimation(1, "gunIdle", true);
			break;
		case 3:
			animation.state.SetAnimation(1, "machineGunIdle", true);
			break;
		}
*/
	}

	// Update is called once per frame
	void Update () {
		if(!dead)
		{
			if(reverse)
			{
				body.velocity = new Vector2(-moveSpeed, 0);
				if(transform.position.x < pointA.position.x)
					reverse = false;
			}
			else
			{
				body.velocity = new Vector2(moveSpeed, 0);
				if(transform.position.x > pointB.position.x)
					reverse = true;
			}
			if(hitAnim)
			{
				body.velocity = new Vector2(moveSpeed * -hitMultiplier, 0);
			}

			Flip ();
		}
	}

	void Flip()
	{
		if(!hitAnim)
		{
			if (body.velocity.x > 0)
				transform.localRotation = Quaternion.identity;
			else if (body.velocity.x < 0)
				transform.localRotation = flippedRotation;
		}
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		
		if(collider.tag == "Bullet" || collider.tag == "Sword")
		{
			if(health <= 0)
			{
				animation.state.SetAnimation(0, "hitBig", false);
				GetComponent<BoxCollider2D>().enabled = false;
				GetComponent<CircleCollider2D>().enabled = false;
				animation.state.SetAnimation(1, "reset", true);
				dead = true;
				body.velocity = new Vector2(4, 0);
				StartCoroutine(ZeroVelocity());
			}
			else
			{
				if(collider.transform.position.x > transform.position.x)
				{
					hitMultiplier = 0.25f;
				}
				else
				{
					hitMultiplier = -0.25f;
				}
				hitAnim = true;
				animation.state.SetAnimation(0, "hit1", false).Complete+= delegate {animation.state.SetAnimation(0, moveAnim, true); hitAnim = false;};
				body.velocity = new Vector2(body.velocity.x *-1, 0);
				health--;
			}
		}

	}

	IEnumerator ZeroVelocity()
	{
		yield return new WaitForSeconds(0.2f);
		body.velocity = Vector2.zero;
	}
}
