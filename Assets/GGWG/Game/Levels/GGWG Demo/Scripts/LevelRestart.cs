using UnityEngine;
using System.Collections;

public class LevelRestart : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		if(collider.tag == "Player")
		{
			Application.LoadLevel (Application.loadedLevelName);
		}
	}
}
