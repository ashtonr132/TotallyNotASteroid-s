using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
public class AsteroidBehavoir : MonoBehaviour
{
    [SerializeField]
    private Collider OuterCol;
    [SerializeField]
    private GameObject AsteroidPrefab, PowerUpPrefab, ScoreName; //set in inspector
    [SerializeField]
    private GameObject InnerRing, OuterRing, scoreUI;
    private bool ShieldActive = false;
    private Vector3 OuterCenter;
    private float AsteroidSpawnCDTimer = 0, VeloInc = 1.25f, PowerUpSpawnRate = 5.0f, Counter = 0, ShieldRotationSpeed = 0, x = 0;
    internal static float AsteroidSpawnRate = 3f, SpawnRateCap = 1.5f;
    internal static int Score;
    internal static List<GameObject> AsteroidList = new List<GameObject>(), ShieldList = new List<GameObject>();
    
    void Start()
    {
        InvokeRepeating("DifficultyAdd", 5.0f, 5.0f);
        ShieldList.Clear();
        foreach (Transform Shield in transform)
        {
            if (Shield.tag == "Shield")
            {
                ShieldList.Add(Shield.gameObject);
            }
        }
        OuterCenter = OuterCol.bounds.center;
        ShieldList[0].transform.position = new Vector3(OuterCenter.x + 2, OuterCenter.y, OuterCenter.z); //setting starting pos
        ShieldList[1].transform.position = new Vector3(OuterCenter.x - 2, OuterCenter.y, OuterCenter.z);
        ShieldList[2].transform.position = new Vector3(OuterCenter.x, OuterCenter.y + 2, OuterCenter.z);
        ShieldList[3].transform.position = new Vector3(OuterCenter.x, OuterCenter.y - 2, OuterCenter.z);
        foreach (GameObject Shield in ShieldList)
        {
            Shield.SetActive(false);
        }
    }

    void Update()
    {
        if (PlayerBehavoir.GameStarted)
        {
            if (PlayerBehavoir.PlayerHealth > 0 && Time.timeScale != 0)
            {
                Score++;
            }
            scoreUI.GetComponent<Text>().text = "Score : " + Score; //display latest score val
            InnerRing.transform.RotateAround(InnerRing.GetComponent<Renderer>().bounds.center, Vector3.forward, Time.deltaTime * 5);
            OuterRing.transform.RotateAround(OuterRing.GetComponent<Renderer>().bounds.center, -Vector3.forward, Time.deltaTime * 5);
            if (ShieldActive == true)
            {
                ShieldFunction();
            }
            SpawnAsteroid();
        }
    }
    private void ShieldFunction()
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
            Transform ShieldTrans = Shield.transform;
            if (Counter < 2)
            {
                ShieldRotationSpeed += 2; //Acceleration
                ShieldTrans.LookAt(OuterCenter);
                ShieldTrans.RotateAround(OuterCenter, new Vector3(0, 0, 2.5f), ShieldRotationSpeed * Time.deltaTime);
                if (Vector3.Distance(Shield.transform.position, OuterCenter) < 5)
                {
                    ShieldTrans.Translate(-Vector3.forward * Time.deltaTime); //move away from player OT if within 5 units
                }
            }
            else if (Counter < 5)
            {
                ShieldTrans.LookAt(OuterCenter);
                ShieldTrans.RotateAround(OuterCenter, new Vector3(0, 0, 2.5f), ShieldRotationSpeed * Time.deltaTime);
            }
            else if (Counter < 7)
            {
                if (!(ShieldRotationSpeed <= 0))
                {
                    ShieldRotationSpeed -= 2; //Decelleration
                }
                ShieldTrans.LookAt(OuterCenter);
                ShieldTrans.RotateAround(OuterCenter, new Vector3(0, 0, 2.5f), ShieldRotationSpeed * Time.deltaTime);
                if (Vector3.Distance(ShieldTrans.position, OuterCenter) > 2)
                {
                    ShieldTrans.Translate(Vector3.forward * Time.deltaTime); //move away from player OT if within 5 units
                }
            }
        }
        if (Counter > 6)
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
    private void SpawnAsteroid()
    {
        if (AsteroidSpawnCDTimer > AsteroidSpawnRate)
        {
            bool SpawnFailed = false;
            float a = Mathf.Sqrt(Random.Range(0.5f, 6.125f)), b = Random.Range(0.0f, 80.0f); //a velocity and size scale, b bytes, c spawn random, d spawn side pos/min variance
            Vector3 SpawnPosition = transform.position + (Vector3)Random.insideUnitCircle.normalized * 32;
            GameObject[] SnapShotArr = AsteroidList.ToArray();
            foreach (GameObject Astor in SnapShotArr)
            {
                if (Astor != null)
                {
                    if (Vector3.Distance(Astor.transform.position, SpawnPosition) < a / 100 + Astor.transform.localScale.magnitude)
                    {
                        SpawnFailed = true; //if any new asteroid will spawn inside a prexisting asteroid failspawn
                        SpawnAsteroid();
                    }
                }
            }
            if (SpawnFailed == false)
            {
                GameObject Prefab = (Random.Range(0.0f, 100.0f) <= PowerUpSpawnRate) ? PowerUpPrefab : AsteroidPrefab; //if rand is less than powerupspawn rate prefab is powerupprefab else its asteroid prefab
                GameObject Asteroid = (GameObject)Instantiate(Prefab, SpawnPosition, Quaternion.identity);
                if (!ScoreName.activeSelf)
                {
                    PlayerBehavoir.AsteroidsEvaded++;
                }
                Rigidbody AstRB = Asteroid.GetComponent<Rigidbody>(); //this asteroids RB
                AsteroidList.Add(Asteroid);
                Asteroid.name = Prefab.name + " " + AsteroidList.Count;
                Asteroid.transform.localScale = Vector3.one * a / 100; //size scaled betw a rand
                AstRB.mass = 115 * a;
                AstRB.velocity = (((transform.position + (Vector3)Random.insideUnitCircle.normalized * Random.Range(0, 18)) - SpawnPosition) * VeloInc / (Mathf.Sqrt(a) * AstRB.mass)) * (VeloInc * (Random.Range(70, 100)/10));
                byte[] mybyte = System.BitConverter.GetBytes(b); //color variance via bits from rand float
                while (mybyte[1] < 20 || mybyte[1] > 50) //is too dark or too light? reroll
                {
                    b = Random.Range(0.0f, 80.0f);
                    mybyte = System.BitConverter.GetBytes(b);
                }
                Material AMat = Asteroid.GetComponent<Renderer>().material;
                if (Asteroid.gameObject.tag == "Asteroid")
                {
                    Color32 colorVar = new Color32(mybyte[1], mybyte[1], mybyte[1], mybyte[1]); //fiddy shades of kreygasm
                    PowerUpSpawnRate += 0.1f;
                    AMat.color = colorVar; //set renderer colour
                    AMat.shader = Shader.Find("Legacy Shaders/Decal"); //set shader type
                    Material BMat = Asteroid.transform.GetChild(0).GetComponent<Renderer>().material;
                    BMat.color = colorVar; //set renderer colour
                    BMat.shader = Shader.Find("Legacy Shaders/Decal"); //set shader type
                }
                else if (Asteroid.gameObject.tag == "PowerUp")
                {
                    PowerUpSpawnRate = 6.0f;
                    a = Random.Range(65.0f, 85.0f); //redeclar a for powerup physics
                    Asteroid.transform.localScale = Vector3.one * a / 80;
                    Color32 colorVar = new Color32(mybyte[0], mybyte[1], mybyte[2], mybyte[3]); //any colour
                    AMat.color = colorVar; //set renderer colour
                    AMat.shader = Shader.Find("Standard"); //set shader type
                }
                AsteroidSpawnCDTimer = 0;
            }
        }
        else
        {
            AsteroidSpawnCDTimer += (Random.Range(0.0f, 3.0f) * Time.deltaTime); //asteroids spawn timer variance
        }
    }
    private void DifficultyAdd()
    {
        if (AsteroidSpawnRate > SpawnRateCap)
        {
            AsteroidSpawnRate -= 0.05f; //shorter spawn gap
            VeloInc += 0.02f; //spawn higher velo
        }
    }
    private void Count()
    {
        if(Counter == 0 && ShieldList[0].activeSelf == true)
        {
            ShieldList[0].transform.position = new Vector3(OuterCenter.x + 2, OuterCenter.y, OuterCenter.z); //setting starting pos
            ShieldList[1].transform.position = new Vector3(OuterCenter.x - 2, OuterCenter.y, OuterCenter.z);
            ShieldList[2].transform.position = new Vector3(OuterCenter.x, OuterCenter.y + 2, OuterCenter.z);
            ShieldList[3].transform.position = new Vector3(OuterCenter.x, OuterCenter.y - 2, OuterCenter.z);
        }
        Counter++;
    }
    public void SetShieldBool(bool tf)
    {
        ShieldActive = tf;
    }
}