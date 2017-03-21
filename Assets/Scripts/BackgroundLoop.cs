using UnityEngine;
using System.Collections;

public class BackgroundLoop : MonoBehaviour {

    public float bckScrollSpeed = 1, tileSizex = 128.0f;
	private Vector3 startPos;

	void Start ()
    {
	    startPos = transform.position;
	}

	void Update ()
    {
	    var newPosition = Mathf.Repeat(Time.time * bckScrollSpeed, tileSizex); //move over time between voundaries
	    transform.position = startPos + Vector3.right * newPosition;
	}
}

