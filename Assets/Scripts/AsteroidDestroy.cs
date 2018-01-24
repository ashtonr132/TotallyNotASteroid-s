using System.Collections.Generic;
using UnityEngine;

public class AsteroidDestroy : MonoBehaviour
{
    private GameObject Player;
    private AsteroidBehavoir astbehav;
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        astbehav = Player.GetComponent<AsteroidBehavoir>();
    }

    void Update()
    {
        transform.Rotate(new Vector3(Random.Range(0, 359.9f), Random.Range(0, 359.9f), Random.Range(0, 359.9f)) * Random.Range(0, 0.1f) * Time.deltaTime);
        Vector3 pos = transform.position; //cannot assign directly to the position in this case
        pos.z = 0;
        transform.position = pos;
        if (Vector3.Distance(Player.transform.position, transform.position) > 22) //destroy if out of game range
        {
            List<GameObject> newlist = astbehav.GetList("Asteroid");
            astbehav.addEvadedAsteroids();
            newlist.Remove(gameObject);
            astbehav.SetList("Asteroid", newlist);
            Destroy(gameObject, 0);
        }
    }
}
