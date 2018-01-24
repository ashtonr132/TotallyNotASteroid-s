using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletBehavoir : MonoBehaviour
{
    void Start()
    {
        transform.Rotate(new Vector3(0, 180, 0), Space.Self);
    }
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "PowerUp")
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehavoir>().PowerUpEffect();
            List<GameObject> newlist = GameObject.FindGameObjectWithTag("Player").GetComponent<AsteroidBehavoir>().GetList("Asteroid");
            newlist.Remove(col.gameObject);
            GameObject.FindGameObjectWithTag("Player").GetComponent<AsteroidBehavoir>().SetList("Asteroid", newlist);
            Destroy(col.gameObject, 0);
        }
        if (col.gameObject.tag == "Shield" || col.gameObject.tag == "Barrel")
        {
            Physics.IgnoreCollision(col.gameObject.GetComponent<Collider>(), GetComponent<Collider>());
        }
        if ((col.gameObject.tag == "Player") && (col.gameObject.tag != "OuterRing"))
        {
            Destroy(gameObject, 0);
        }
    }
}