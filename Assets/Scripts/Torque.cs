using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torque : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        GetComponent<Rigidbody>().AddTorque(new Vector3(Random.Range(-10000, 10000), Random.Range(-10000, 10000), Random.Range(-10000, 10000)));
	}
}
