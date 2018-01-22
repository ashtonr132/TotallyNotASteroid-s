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
        if (transform.position.x >= -92)
        {
            transform.position = startPos;
        }
	    transform.position = startPos + (-Vector3.right * Mathf.Repeat(Time.time * bckScrollSpeed, tileSizex));
	}
}

