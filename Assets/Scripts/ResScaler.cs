using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResScaler : MonoBehaviour {
    [SerializeField]
    private GameObject canvas;
	// Use this for initialization
	void Start ()
    {
        for (float i = gameObject.GetComponent<RectTransform>().sizeDelta.y * gameObject.transform.localScale.y; i < canvas.GetComponent<RectTransform>().sizeDelta.y; i = gameObject.GetComponent<RectTransform>().sizeDelta.y * gameObject.transform.localScale.y)
        {
            gameObject.GetComponent<RectTransform>().sizeDelta += new Vector2(1.6f, 1);
        }
        for (float i = gameObject.GetComponent<RectTransform>().sizeDelta.x * gameObject.transform.localScale.x; i < canvas.GetComponent<RectTransform>().sizeDelta.x; i = gameObject.GetComponent<RectTransform>().sizeDelta.x * gameObject.transform.localScale.x)
        {
            gameObject.GetComponent<RectTransform>().sizeDelta += new Vector2(1.6f, 1);
        }
    }
}
