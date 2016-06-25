using UnityEngine;
using System.Collections;

public class LedgeHang : MonoBehaviour {

	public Transform playerPosition;
	public CameraFollowPlayer mainCamera;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		Player player = collider.GetComponent<Player>();
		player.SetCurrentState(Player.PlayerStates.edgeIdle);
		mainCamera.follow = transform;
		player.animation.state.SetAnimation(0, "edgeIdle", true);
		player.transform.position = playerPosition.position;

	}
}
