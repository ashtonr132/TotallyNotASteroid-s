using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasToResolution : MonoBehaviour {

	// Use this for initialization
	void Start () {
        float currentAspect = ((float)Screen.width / (float)Screen.height) / 5;
        Camera.main.orthographicSize = (Screen.currentResolution.height / currentAspect) / 200;
    }
}
