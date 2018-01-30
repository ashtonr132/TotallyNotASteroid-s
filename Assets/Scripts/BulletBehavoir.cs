using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletBehavoir : MonoBehaviour
{
    private GameObject player;
    void Start()
    {
        player = GameObject.Find("ship");
        transform.Rotate(new Vector3(-90, 0, 0), Space.Self);
    }
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "PowerUp")
        {
            player.GetComponent<PlayerBehavoir>().PowerUpEffect(transform.position);
            List<GameObject> newlist = player.GetComponent<AsteroidBehavoir>().GetList("Asteroid");
            newlist.Remove(col.gameObject);
            player.GetComponent<AsteroidBehavoir>().SetList("Asteroid", newlist);
            Destroy(col.gameObject, 0);
        }
        if (col.gameObject.tag == "Shield" || col.gameObject.name == "Barrel")
        {
            Physics.IgnoreCollision(col.gameObject.GetComponent<Collider>(), GetComponent<Collider>());
        }
        if ((col.gameObject == player) && (col.gameObject.name != "outer"))
        {
            Destroy(gameObject, 0);
        }
    }
}