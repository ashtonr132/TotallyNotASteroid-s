using UnityEngine;
using System.Collections;

public class BackgroundLoop : MonoBehaviour {

    internal float bckScrollSpeed = 1, tileSizex = 128.0f;
	private Vector3 startPos;

	void Start ()
    {
	    startPos = transform.position;
	}

    void Update()
    {
        if (PlayerBehavoir.GameStarted)
        {
            transform.position = startPos + (-Vector3.right * Mathf.Repeat(Time.time - PlayerBehavoir.LevelTime * bckScrollSpeed, tileSizex));
        }
    }
}

