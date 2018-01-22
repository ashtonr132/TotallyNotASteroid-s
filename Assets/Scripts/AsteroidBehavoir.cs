using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
public class AsteroidBehavoir : MonoBehaviour
{
    private int Score = 0, AsteroidsEvaded = 0;
    private float AsteroidSpawnRate = 2.0f, AsteroidSpawnCDTimer = 0, VeloInc = 0, PowerUpSpawnRate = 5.0f, Counter = 0, ShieldRotationSpeed = 0, x = 0;
    public GameObject AsteroidPrefab, PowerUpPrefab; //set in inspector
    private bool ShieldActive = false;
    private GameObject InnerRing, OuterRing, ShipModel, scoreUI;
    private Vector3 AstroidVelo;
    private Collider OuterCol, InnerCol;
    private List<GameObject> AsteroidList = new List<GameObject>(), ShieldList = new List<GameObject>();

    void Start()
    {
        scoreUI = GameObject.FindGameObjectWithTag("Score");
        InnerRing = GameObject.FindGameObjectWithTag("InnerRing"); //of player ship
        OuterRing = GameObject.FindGameObjectWithTag("OuterRing");
        OuterCol = OuterRing.GetComponent<Collider>(); //colliders of ship
        InnerCol = InnerRing.GetComponent<Collider>();
        InvokeRepeating("DifficultyAdd", 5.0f, 5.0f);
        foreach (GameObject Shield in GameObject.FindGameObjectsWithTag("Shield"))
        {
            ShieldList.Add(Shield);
        }
        ShieldList[0].transform.position = new Vector3(OuterCol.bounds.center.x + 2, OuterCol.bounds.center.y, OuterCol.bounds.center.z); //setting starting pos
        ShieldList[1].transform.position = new Vector3(OuterCol.bounds.center.x - 2, OuterCol.bounds.center.y, OuterCol.bounds.center.z);
        ShieldList[2].transform.position = new Vector3(OuterCol.bounds.center.x, OuterCol.bounds.center.y + 2, OuterCol.bounds.center.z);
        ShieldList[3].transform.position = new Vector3(OuterCol.bounds.center.x, OuterCol.bounds.center.y - 2, OuterCol.bounds.center.z);
        foreach (GameObject Shield in ShieldList)
        {
            Shield.SetActive(false);
        }
    }

    void Update()
    {
        if (GetComponent<PlayerBehavoir>().IsPlayerDead() == false)
        {
            Score++;
        }
        scoreUI.GetComponent<Text>().text = "Score : " + Score; //display latest score val
        InnerRing.transform.RotateAround(Vector3.zero, Vector3.forward, Time.deltaTime * 5);
        OuterRing.transform.RotateAround(Vector3.zero, -Vector3.forward, Time.deltaTime * 5);
        if(ShieldActive == true)
        {
            foreach (GameObject Shield in ShieldList)
            {
                Shield.SetActive(true);
            }
            if (x == 0)
            {
                InvokeRepeating("Count", 0.0f, 1.0f);
                x++;
            }
            foreach (GameObject Shield in ShieldList)
            {
                if (Counter < 2)
                {
                    ShieldRotationSpeed += 2; //Acceleration
                    Shield.transform.LookAt(OuterCol.bounds.center);
                    Shield.transform.RotateAround(OuterCol.bounds.center, new Vector3(0, 0, 2.5f), ShieldRotationSpeed * Time.deltaTime);
                    if (DistanceBetween(Shield.transform.position, OuterCol.bounds.center) < 5)
                    {
                        Shield.transform.Translate(-Vector3.forward * Time.deltaTime);
                    } //move away from player OT if within 5 units
                }
                else if (Counter < 5)
                {
                    Shield.transform.LookAt(OuterCol.bounds.center);
                    Shield.transform.RotateAround(OuterCol.bounds.center, new Vector3(0, 0, 2.5f), ShieldRotationSpeed * Time.deltaTime);
                }
                else if (Counter < 7)
                {
                    if (!(ShieldRotationSpeed <= 0))
                    {
                        ShieldRotationSpeed -= 2;
                    } //Decelleration
                    Shield.transform.LookAt(OuterCol.bounds.center);
                    Shield.transform.RotateAround(OuterCol.bounds.center, new Vector3(0, 0, 2.5f), ShieldRotationSpeed * Time.deltaTime);
                    if (DistanceBetween(Shield.transform.position, OuterCol.bounds.center) > 2)
                    {
                        Shield.transform.Translate(Vector3.forward * Time.deltaTime);
                    } //move away from player OT if within 5 units
                }
            }
            if(Counter > 6)
            {
                CancelInvoke("Count");
                Counter = 0;
                ShieldActive = false;
                x = 0;
                foreach (GameObject Shield in ShieldList)
                {
                    Shield.SetActive(false);
                }
            }
           
        }
        if (AsteroidSpawnCDTimer > AsteroidSpawnRate)
        {
            bool SpawnFailed = false;
            float a = Mathf.Sqrt(Random.Range(0.25f, 9.0f)), b = Random.Range(0.0f, 80.0f), c = Random.Range(0.0f, 4.0f), d = Random.Range(-4.0f, 4.0f); //a velocity and size scale, b bytes, c spawn random, d spawn side pos/min variance
            Vector3 SpawnPosition; //assigning random spawn pos
            if (c <= 1)
            {
                SpawnPosition = new Vector3(transform.position.x + d, (transform.position.y + 20), 0);
            }
            else if (c <= 2)
            {
                SpawnPosition = new Vector3((transform.position.x + 20), transform.position.y + d, 0);
            }
            else if (c <= 3)
            {
                SpawnPosition = new Vector3(transform.position.x + d, (transform.position.y - 20), 0);
            }
            else
            {
                SpawnPosition = new Vector3(transform.position.x - 20, (transform.position.y + d), 0);
            }
            foreach (GameObject Astorpwrup in AsteroidList)
            {
                if (DistanceBetween((Astorpwrup.GetComponent<Collider>().bounds.center + Astorpwrup.GetComponent<Collider>().bounds.extents), SpawnPosition) <= (a * Mathf.Sqrt(a))) SpawnFailed = true; //if any new asteroid will spawn inside a prexisting asteroid failspawn
            }
            if (SpawnFailed == false)
            {
                GameObject Prefab = (Random.Range(0.0f, 100.0f) <= PowerUpSpawnRate) ? PowerUpPrefab : AsteroidPrefab; //if rand is less than powerupspawn rate prefab is powerupprefab else its asteroid prefab
                GameObject Asteroid = (GameObject)Instantiate(Prefab, SpawnPosition, new Quaternion(0, 0, 0, 0));
                AsteroidList.Add(Asteroid);
                Rigidbody AstRB = Asteroid.GetComponent<Rigidbody>(); //this asteroids RB
                Asteroid.transform.localScale = Vector3.one * a / 100; //size scaled betw /3 and 3*
                if (SpawnPosition.y == transform.position.y + 20)
                {
                    AstroidVelo = -transform.up * (VeloInc + 1) * 200 / Mathf.Sqrt(a);
                }//velocity maths, larger asteroids spawn with smaller velocities, velo dir relative to spawnp
                else if (SpawnPosition.y == transform.position.y - 20)
                {
                    AstroidVelo = transform.up * (VeloInc + 1) * 200 / Mathf.Sqrt(a);
                }
                else if (SpawnPosition.x == transform.position.x + 20)
                {
                    AstroidVelo = -transform.right * (VeloInc + 1) * 200 / Mathf.Sqrt(a);
                }
                else
                {
                    AstroidVelo = transform.right * (VeloInc + 1) * 200 / Mathf.Sqrt(a);
                }
                AstRB.mass = 80 * a;
                AstRB.velocity = AstroidVelo / AstRB.mass;
                byte[] mybyte = System.BitConverter.GetBytes(b); //color variance via bits from rand float
                while (mybyte[1] < 20 || mybyte[1] > 50) //is too dark or too light? reroll
                {
                    b = Random.Range(0.0f, 80.0f);
                    mybyte = System.BitConverter.GetBytes(b);
                }
                if (Asteroid.gameObject.tag == "Asteroid")
                {
                    Color32 colorVar = new Color32(mybyte[1], mybyte[1], mybyte[1], mybyte[1]); //fiddy shades of kreygasm
                    PowerUpSpawnRate += 0.1f;
                    Asteroid.GetComponent<Renderer>().material.color = colorVar; //set renderer colour
                    Asteroid.GetComponent<Renderer>().material.shader = Shader.Find("Legacy Shaders/Decal"); //set shader type
                    Asteroid.transform.GetChild(0).GetComponent<Renderer>().material.color = colorVar; //set renderer colour
                    Asteroid.transform.GetChild(0).GetComponent<Renderer>().material.shader = Shader.Find("Legacy Shaders/Decal"); //set shader type
                }
                else if (Asteroid.gameObject.tag == "PowerUp")
                {
                    PowerUpSpawnRate = 6.0f;
                    a = Random.Range(65.0f, 85.0f); //redeclar a for powerup physics
                    Asteroid.transform.localScale = Vector3.one * a / 80;
                    Color32 colorVar = new Color32(mybyte[0], mybyte[1], mybyte[2], mybyte[3]); //any colour
                    AstRB.velocity = AstroidVelo / a;
                    Asteroid.GetComponent<Renderer>().material.color = colorVar; //set renderer colour
                    Asteroid.GetComponent<Renderer>().material.shader = Shader.Find("Standard"); //set shader type
                }
                AsteroidSpawnCDTimer = 0;
            }
        }
        else
        {
            AsteroidSpawnCDTimer += (Random.Range(0.0f, 3.0f) * Time.deltaTime);
        }//asteroids spawn timer variance
    }

    private void DifficultyAdd()
    {
        if (AsteroidSpawnRate > 0.2f)
        {
            AsteroidSpawnRate -= 0.075f; //shorter spawn gap
            VeloInc = +0.02f; //spawn higher velo
        }
    }

    public float DistanceBetween(Vector3 a, Vector3 b)
    {
        float y1 = Mathf.Abs(a.y), y2 = Mathf.Abs(b.y), x1 = Mathf.Abs(a.x);var x2 = Mathf.Abs(b.x); //abs to ensure that negative locations donot fuck up calcs
        return Mathf.Sqrt(((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1))); //pythagorean formula for dist betw
    }

    public int getScore()
    {
        return Score;
    }

    public void setScore(int scorein)
    {
        if (gameObject.GetComponent<PlayerBehavoir>().IsPlayerDead() == false)
        {
            Score += scorein;
        }
    }

    public int GetEvadedAsteroids()
    {
        return AsteroidsEvaded;
    }

     public void addEvadedAsteroids()
    {
        AsteroidsEvaded++;
    }

    public List<GameObject> GetList(string list)
    {
        //if (list == "Asteroid")
            return AsteroidList;
    }
    public void SetList(string list, List<GameObject> list2)
    {
        if (list == "Asteroid")
        {
            AsteroidList = list2;
        }
    }
    private void Count()
    {
        if((Counter == 0) && ShieldList[0].activeSelf == true)
        {
            ShieldList[0].transform.position = new Vector3(OuterCol.bounds.center.x + 2, OuterCol.bounds.center.y, OuterCol.bounds.center.z); //setting starting pos
            ShieldList[1].transform.position = new Vector3(OuterCol.bounds.center.x - 2, OuterCol.bounds.center.y, OuterCol.bounds.center.z);
            ShieldList[2].transform.position = new Vector3(OuterCol.bounds.center.x, OuterCol.bounds.center.y + 2, OuterCol.bounds.center.z);
            ShieldList[3].transform.position = new Vector3(OuterCol.bounds.center.x, OuterCol.bounds.center.y - 2, OuterCol.bounds.center.z);
        }
        Counter++;
    }
    public void setShieldBool(bool tf)
    {
        ShieldActive = tf;
    }
}