using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerBehavoir : MonoBehaviour
{
    private int asteroidsondeath = 999999999, PlayerHealth = 3, HighScore = 0, FireReps = 0, RunScore;
    private GameObject EndUI, LifeUI, Score, Barrel, SplashText, AsteroidHit;
    public GameObject BulletPrefab; // assign in inspector
    private float BulletSize = 1, ShotSpeed = 0.275f, BulletVelocityModifier = 1, ShotCDTimer = 0 ,repeats = 0;
    private bool InvincibilityFrames = false, TripleShot = false;
    private string SplashString = string.Empty;

    void Start()
    {
        LifeUI = GameObject.FindGameObjectWithTag("LifeUI"); //finding game objects and assigning their references
        EndUI = GameObject.FindGameObjectWithTag("EndUI");
        Score = GameObject.FindGameObjectWithTag("Score");
        Barrel = GameObject.FindGameObjectWithTag("Barrel");
        SplashText = GameObject.FindGameObjectWithTag("SplashText");
        EndUI.GetComponent<Text>().text = string.Empty; //initialised as blank to get rid of the filler text
        HighScore = PlayerPrefs.GetInt("highscore"); //playerprefs persist through scene reloads
        }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))  Application.Quit();
        ShotCDTimer += Time.deltaTime; //shotspeed incrememnt
        LifeUI.GetComponent<Text>().text = "Health : " + PlayerHealth; //displays
        SplashText.GetComponent<Text>().text = SplashString;
        if (IsPlayerDead() == true) //do the end game ui's
        {
            NewHighScore();
            PlayEndScreen();
        }
        else
        {
            if (Input.GetKey(KeyCode.Space)) Fire();
            if (Input.GetKey(KeyCode.LeftArrow)) Barrel.transform.RotateAround(GameObject.FindGameObjectWithTag("OuterRing").GetComponent<Collider>().bounds.center, Vector3.forward, 300 * Time.deltaTime); //move LR
            else if (Input.GetKey(KeyCode.RightArrow)) Barrel.transform.RotateAround(GameObject.FindGameObjectWithTag("OuterRing").GetComponent<Collider>().bounds.center, -Vector3.forward, 300 * Time.deltaTime);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        col.gameObject.GetComponent<Rigidbody>().velocity = -col.gameObject.GetComponent<Rigidbody>().velocity * 1.35f; //Bounce away with force, makes GO getting stuck less likely
        if (col.gameObject.tag == "Asteroid" && InvincibilityFrames == false)
        {
            PlayerHealth--;
            AsteroidHit = col.gameObject;
            InvokeRepeating("DoneDamage", 0, 1);
            StartCoroutine(WaitFor(0.5f, "InvincibilityFrames"));
            foreach (GameObject Asteroid in GetComponent<AsteroidBehavoir>().GetList("Asteroid"))
            {
               if (Asteroid != null) //stops npe's when GO is deleted from list (shouldn't be needed as lists are edited correctly, but still trips)
                {
                    if (GetComponent<AsteroidBehavoir>().DistanceBetween(GetComponent<Rigidbody>().transform.position, Asteroid.GetComponent<Rigidbody>().transform.position) >= 1.5)
                        col.gameObject.GetComponent<Rigidbody>().velocity = transform.position - Vector3.forward * Time.deltaTime; //an asteroid is within outering collider so force it out
                }
            }
        }
    }

    public void PowerUpEffect()
    {
        var x = UnityEngine.Random.Range(0, 9);
        switch (x)
        {
            case 0:
                GetComponent<AsteroidBehavoir>().setScore((int)(Mathf.Abs(GetComponent<AsteroidBehavoir>().getScore() * 0.05f)));
                SplashString = "More Points!";
                break;
            case 1:
                if (PlayerHealth < 5)
                {
                    PlayerHealth++;
                    SplashString = "Health Up!";
                }
                else PowerUpEffect();
                break;
            case 2:
                for (var i = 0; i < 36; i++)
                {
                    Vector3 pos = CircleSpawn(Vector3.zero, 3, i * 10); //shoots bullets in a circle around the player
                    Quaternion rot = Quaternion.FromToRotation(-Vector3.right, Vector3.zero - pos);
                    GameObject Bullet = (GameObject)Instantiate(BulletPrefab, pos, rot);
                    BulletCharacteristics(Bullet);
                    Destroy(Bullet, 2);
                }
                SplashString = "Bullet Blitz";
                break;
            case 3:
                InvokeRepeating("BulletRing", 0, 0.01f); //shoots bullets in a timed rotation
                SplashString = "Bullet Wave";
                break;
            case 4:
                StartCoroutine(WaitFor(3, "BigBullet"));
                SplashString = "Pack a Punch";
                break;
            case 5:
                StartCoroutine(WaitFor(5, "TripleShot"));
                SplashString = "TripleShot";
                break;
            case 6:
                StartCoroutine(WaitFor(5, "ShotSpeedUp"));
                SplashString = "ShotSpeedUp";
                break;
            case 7:
                foreach (GameObject Asteroid in GetComponent<AsteroidBehavoir>().GetList("Asteroid"))
                {
                    if (Asteroid != null) Asteroid.GetComponent<Rigidbody>().velocity = Vector3.zero;
                }
                SplashString = "Freeze";
                break;
            case 8:
                GetComponent<AsteroidBehavoir>().setShieldBool(true);
                SplashString = "Shield Activated";
                break;
            default:
                SplashString = "error in powerupeffect";
                break;
        }
    }

    Vector3 CircleSpawn(Vector3 center, float radius, float ang) //returns a v3 point encircling an object in 2dspace
    {
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.z = center.z;
        return pos;
    }

    void BulletRing()
    {
        var pos = CircleSpawn(Vector3.zero, 3, FireReps * 10);//position of the bullet
        var rot = Quaternion.FromToRotation(-Vector3.right, Vector3.zero - pos); //rotation of pos
        var Bullet = (GameObject)Instantiate(BulletPrefab, pos, rot);
        BulletCharacteristics(Bullet);
        FireReps++;
        Bullet.GetComponent<Rigidbody>().velocity = Bullet.GetComponent<Rigidbody>().velocity / 5; //bullet ring fires slower bullets
        Destroy(Bullet, 2);
        if (FireReps > 36)
        {
            CancelInvoke("BulletRing"); //stop calling the next rotation step change and fire at max firerepetitions
            FireReps = 0;
        }
    }

    public bool IsPlayerDead()
    {
        if (PlayerHealth <= 0)
        {
            RunScore = gameObject.GetComponent<AsteroidBehavoir>().getScore(); //get score on death, stops update mistakes, afterdeath
            return true;
        }
        return false;
    }

    private void NewHighScore()
    {
        if (RunScore > HighScore)
        {
            HighScore = RunScore;
            PlayerPrefs.SetInt("highscore", HighScore);
        }
    }

    void PlayEndScreen() //display end info and start reset scene countdown
    {
        if (asteroidsondeath == 999999999)
        {
            asteroidsondeath = gameObject.GetComponent<AsteroidBehavoir>().GetEvadedAsteroids();
        }
        EndUI.GetComponent<Text>().text = "Score : " + RunScore + System.Environment.NewLine + "Session HighScore : " +
         HighScore + System.Environment.NewLine + "Asteroids Cleared : " + asteroidsondeath;
        LifeUI.SetActive(false); Score.SetActive(false);
        StartCoroutine(WaitFor(10, "ResetScene"));
    }

    public IEnumerator WaitFor(float WaitTime, string callref) //varios timed function calls
    {
        GetComponent<AsteroidBehavoir>().setScore((int)(Mathf.Abs(GetComponent<AsteroidBehavoir>().getScore() * 0.01f)));
        switch (callref)
        {
            case "ResetScene":
                yield return new WaitForSeconds(WaitTime);
                SceneManager.LoadScene("scene");
                break;
            case ("InvincibilityFrames"):
                InvincibilityFrames = true;
                yield return new WaitForSeconds(WaitTime);
                InvincibilityFrames = false;
                break;
            case ("BigBullet"):
                BulletVelocityModifier = 0.5f;
                BulletSize = 3;
                yield return new WaitForSeconds(WaitTime);
                BulletVelocityModifier = 1;
                BulletSize = 1;
                break;
            case ("TripleShot"):
                TripleShot = true;
                yield return new WaitForSeconds(WaitTime);
                TripleShot = false;
                break;
            case ("ShotSpeedUp"):
                ShotSpeed -= 0.1f;
                yield return new WaitForSeconds(WaitTime);
                ShotSpeed += 0.1f;
                break;
        }
    }

    void Fire() //bullet physics, properties, spawn points
    {
        if (ShotCDTimer > ShotSpeed)
        {
            Vector3 FirePoint3 = Barrel.transform.position + Barrel.transform.rotation * new Vector3(0.55f, 0, 0);
            var Bullet = (GameObject)Instantiate(BulletPrefab, FirePoint3, Barrel.transform.rotation);
            Physics.IgnoreCollision(Bullet.GetComponent<Collider>(), Barrel.GetComponent<Collider>(), true);
            BulletCharacteristics(Bullet);
            if (TripleShot == true)
            {
                Vector3 FirePoint = Barrel.transform.position + Barrel.transform.rotation * new Vector3(0.35f, 1, 0), FirePoint2 = Barrel.transform.position + Barrel.transform.rotation * new Vector3(0.35f, -1, 0); //pos L&R of normal shooting, world space to local space
                GameObject BulletL = (GameObject)Instantiate(BulletPrefab, FirePoint, Barrel.transform.rotation), BulletR = (GameObject)Instantiate(BulletPrefab, FirePoint2, Barrel.transform.rotation); //inst L&R bullets
                BulletCharacteristics(BulletL);
                BulletCharacteristics(BulletR);
            }
            ShotCDTimer = 0;
        }
    }

    private void BulletCharacteristics(GameObject bullet) //physics that applies to all bullets, in this class for useability of variables
    {
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.right * 30 * BulletVelocityModifier;
        bullet.transform.localScale = Vector3.one * BulletSize / 4;
        Destroy(bullet, 7.5f);
    }
    public bool GetIsInvincible()
    {
        return InvincibilityFrames;
    }
    private void DoneDamage()
    {
        repeats++;
        if (repeats < 6)
        {
            AsteroidHit.GetComponent<Rigidbody>().isKinematic = true;
            AsteroidHit.GetComponent<Rigidbody>().velocity = Vector3.zero;
            AsteroidHit.GetComponent<Collider>().enabled = false;
            AsteroidHit.gameObject.SetActive(AsteroidHit.gameObject.activeSelf == true ? false : true);
        }
        else{
            repeats = 0;
            CancelInvoke("DoneDamage");
            List<GameObject> newlist = GetComponent<AsteroidBehavoir>().GetList("Asteroid");
            GetComponent<AsteroidBehavoir>().addEvadedAsteroids();
            newlist.Remove(AsteroidHit);
            GetComponent<AsteroidBehavoir>().SetList("Asteroid", newlist);
            Destroy(AsteroidHit, 0);
        }
    }
}