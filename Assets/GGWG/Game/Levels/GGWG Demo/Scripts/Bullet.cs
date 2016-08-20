using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	public float bulletSpeed = 100;
	private bool active = true;
	private Animator anim;

	// Use this for initialization
	void Start ()
	{
		anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(active)
		{
			transform.position += Time.deltaTime * bulletSpeed * transform.right;
		}
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		if(collider.gameObject.layer == 9 || collider.gameObject.layer == 8 || collider.gameObject.layer == 13)
		{
			active = false;
			int rng = Random.Range (0, 2);
			if(rng == 0)
				anim.Play ("BulletRicochet");
			else
				anim.Play ("BulletRicochet2");
		}
	}

	void DeleteBullet()
	{
		Destroy (gameObject);
	}
}
