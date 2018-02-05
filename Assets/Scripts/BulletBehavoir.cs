using UnityEngine;
using System.Collections.Generic;

public class BulletBehavoir : MonoBehaviour
{
    private GameObject player;
    void Start()
    {
        player = GameObject.Find("ship"); //player ref
        transform.Rotate(new Vector3(-90, 0, 0), Space.Self); //rotate bullet object so that the collider is facing the needed way 
    }
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "PowerUp")
        {
            player.GetComponent<PlayerBehavoir>().PowerUpEffect(col.transform.position, col.gameObject.GetComponent<Renderer>().material.color); //do a powerup effect
            List<GameObject> newlist = player.GetComponent<AsteroidBehavoir>().GetList("Asteroid");
            newlist.Remove(col.gameObject); //remove powerup from list
            player.GetComponent<AsteroidBehavoir>().SetList("Asteroid", newlist); //this stop iterative processes from throwing errors because of editing lists during iterations
            Destroy(col.gameObject, 0);
        }
        if (col.gameObject.tag == "Shield" || col.gameObject.name == "Barrel")
        {
            Physics.IgnoreCollision(col.gameObject.GetComponent<Collider>(), GetComponent<Collider>()); //these two things shouldnt collide
        }
    }
}