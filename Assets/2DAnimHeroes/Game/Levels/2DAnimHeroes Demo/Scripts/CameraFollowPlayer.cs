using UnityEngine;
using System.Collections;

public class CameraFollowPlayer : MonoBehaviour {
	public Transform follow;
	public float smoothing = 5;
	public float offsetX = 0;
	public float offsetY = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = Vector3.Lerp (transform.position, new Vector3(follow.position.x + offsetX, follow.position.y + offsetY, -10), smoothing * Time.deltaTime);
	}
}
