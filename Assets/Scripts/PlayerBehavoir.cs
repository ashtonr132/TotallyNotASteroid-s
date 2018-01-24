using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerBehavoir : MonoBehaviour
{
    private int asteroidsondeath = 0, PlayerHealth = 3, HighScore = 0, FireReps = 0, RunScore;
    private GameObject EndUI, Score, Barrel, SplashText, AsteroidHit, InstructionsUI, GameStartText, AmmoBar, HealthBar;
    private AudioSource Music;
    public GameObject BulletPrefab; // assign in inspector
    private float BulletSize = 1, BulletMass = 1, ShotSpeed = 0.125f, BulletVelocityModifier = 0.75f, ShotCDTimer = 0, repeats = 0, UIY = 0, AmmoRegenTimer = 0, MaxAmmo = 30, AmmoCount, BarrelMoveSpeed = 150, AmmoWait = 0.5f;
    private bool InvincibilityFrames = false, TripleShot = false;
    internal static bool GameStarted = false;
    private string SplashString = string.Empty;

    void Start()
    {
        AmmoCount = MaxAmmo;
        Music = GameObject.Find("Music").GetComponent<AudioSource>();
        GameStartText = GameObject.Find("GameStartText");
        EndUI = GameObject.FindGameObjectWithTag("EndUI");
        InstructionsUI = GameObject.FindGameObjectWithTag("InstructionsUI");
        Score = GameObject.FindGameObjectWithTag("Score");
        Barrel = GameObject.FindGameObjectWithTag("Barrel");
        SplashText = GameObject.FindGameObjectWithTag("SplashText");
        EndUI.GetComponent<Text>().text = string.Empty; //initialised as blank to get rid of the filler text
        HighScore = PlayerPrefs.GetInt("highscore"); //playerprefs persist through scene reloads
        AmmoBar = GameObject.Find("GreenBackground");
        HealthBar = GameObject.Find("RedBackground");
        UIY = InstructionsUI.transform.position.y;
        StartCoroutine(StartAudio());
        UpdateBarUI();
    }

    void Update()
    {
        if ((!GameStarted) && Input.anyKey) //player has started game
        {
            GameStarted = (!GameStarted);
            GameStartText.SetActive(false);
        }
        if (GameStarted)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
            AmmoRegenTimer += Time.deltaTime;
            if (AmmoRegenTimer > AmmoWait && AmmoCount < MaxAmmo)
            {
                AmmoCount += 1f;
            }
            UpdateBarUI();
            ShotCDTimer += Time.deltaTime; //shotspeed incrememnt
            SplashText.GetComponent<Text>().text = SplashString;
            if (Time.time > 15)
            {
                InstructionsUI.transform.position = Vector3.MoveTowards(InstructionsUI.transform.position, new Vector3(InstructionsUI.transform.position.x, UIY + 500, InstructionsUI.transform.position.z), 12 * Time.deltaTime);
            }
            if (IsPlayerDead() == true) //do the end game ui's
            {
                NewHighScore();
                PlayEndScreen();
            }
            else
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    Fire();
                }
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    Barrel.transform.RotateAround(GameObject.FindGameObjectWithTag("OuterRing").GetComponent<Collider>().bounds.center, Vector3.forward, BarrelMoveSpeed * Time.deltaTime); //move LR
                    if (BarrelMoveSpeed < 450) // allows fast rotation without the loss of small movement accuracy
                    {
                        BarrelMoveSpeed += 10f;
                    }
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    Barrel.transform.RotateAround(GameObject.FindGameObjectWithTag("OuterRing").GetComponent<Collider>().bounds.center, -Vector3.forward, BarrelMoveSpeed * Time.deltaTime);
                    if (BarrelMoveSpeed < 450)
                    {
                        BarrelMoveSpeed += 10f;
                    }
                }
                else
                {
                    BarrelMoveSpeed = 150;
                }
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        col.gameObject.GetComponent<Rigidbody>().velocity = -col.gameObject.GetComponent<Rigidbody>().velocity * 1.35f; //Bounce away with force, makes GO getting stuck less likely
        if (col.gameObject.tag == "Asteroid" && InvincibilityFrames == false)
        {
            PlayerHealth--;
            AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("Hit"), GameObject.Find("Music").transform.position, 0.35f);
            AsteroidHit = col.gameObject;
            InvokeRepeating("DoneDamage", 0, 1);
            StartCoroutine(WaitFor(0.5f, "InvincibilityFrames"));
            foreach (GameObject Asteroid in GetComponent<AsteroidBehavoir>().GetList("Asteroid"))
            {
                if (Asteroid != null) //stops npe's when GO is deleted from list (shouldn't be needed as lists are edited correctly, but still trips)
                {
                    if (Vector3.Distance(GetComponent<Rigidbody>().transform.position, Asteroid.GetComponent<Rigidbody>().transform.position) >= 1.5)
                    {
                        col.gameObject.GetComponent<Rigidbody>().velocity = transform.position - Vector3.forward * Time.deltaTime; //an asteroid is within outering collider so force it out
                    }
                }
            }
        }
    }

    public void PowerUpEffect()
    {
        AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("Powerup"), GameObject.Find("Music").transform.position, 0.35f);
        var x = UnityEngine.Random.Range(0, 10);
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
                    if (Asteroid != null)
                    {
                        Asteroid.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    }
                }
                SplashString = "Freeze";
                break;
            case 8:
                GetComponent<AsteroidBehavoir>().setShieldBool(true);
                SplashString = "Shield Activated";
                break;
            case 9:
                if (MaxAmmo < 45)
                {
                    MaxAmmo += 3;
                    SplashString = "Ammo Up!";
                }
                else PowerUpEffect();
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
        var Bullet = (GameObject)Instantiate(BulletPrefab, CircleSpawn(Vector3.zero, 3, FireReps * 10), Quaternion.FromToRotation(-Vector3.right, Vector3.zero - CircleSpawn(Vector3.zero, 3, FireReps * 10)));
        BulletCharacteristics(Bullet);
        FireReps++;
        Bullet.GetComponent<Rigidbody>().velocity = Bullet.GetComponent<Rigidbody>().velocity / 5; //bullet ring fires slower bullets
        Destroy(Bullet, 2);
        if (FireReps > 35)
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
        asteroidsondeath = gameObject.GetComponent<AsteroidBehavoir>().GetEvadedAsteroids();
        EndUI.GetComponent<Text>().text = "Score : " + RunScore + System.Environment.NewLine + "Session HighScore : " + HighScore + System.Environment.NewLine + "Asteroids Cleared : " + asteroidsondeath;
        StartCoroutine(WaitFor(10, "ResetScene"));
    }

    public IEnumerator WaitFor(float WaitTime, string callref) //varios timed function calls
    {
        GetComponent<AsteroidBehavoir>().setScore((int)(Mathf.Abs(GetComponent<AsteroidBehavoir>().getScore() * 0.01f)));
        switch (callref)
        {
            case "ResetScene":
                yield return new WaitForSeconds(WaitTime);
                GameStarted = false;
                SceneManager.LoadScene("scene");
                break;
            case ("InvincibilityFrames"):
                InvincibilityFrames = true;
                yield return new WaitForSeconds(WaitTime);
                InvincibilityFrames = false;
                break;
            case ("BigBullet"):
                BulletVelocityModifier = 0.6f;
                BulletMass = 5;
                BulletSize = 3;
                yield return new WaitForSeconds(WaitTime);
                BulletVelocityModifier = 0.75f;
                BulletMass = 1;
                BulletSize = 1;
                break;
            case ("TripleShot"):
                TripleShot = true;
                AmmoWait = 0.25f;
                yield return new WaitForSeconds(WaitTime);
                TripleShot = false;
                AmmoWait = 0.5f;
                break;
            case ("ShotSpeedUp"):
                ShotSpeed -= 0.1f;
                AmmoWait = 0;
                yield return new WaitForSeconds(WaitTime);
                ShotSpeed += 0.1f;
                AmmoWait = 0.5f;
                break;
        }
        yield return new WaitForSeconds(1.5f);
        SplashString = string.Empty;
    }
    internal IEnumerator FadeOut(GameObject FadeMe, float WaitTime) //slowly up the transparency value of an object until it is transparent then destroy it
    {
        Color PreFadeColor = FadeMe.GetComponent<Renderer>().material.color; //this function was inspired by the example game CubeWorld in Casual game development, however i have written my own version with the functionality i required
        yield return new WaitForSeconds(WaitTime);
        if (PreFadeColor.a >= 0 && FadeMe != null)
        {
            FadeMe.GetComponent<Renderer>().material.color = new Color(PreFadeColor.r, PreFadeColor.g, PreFadeColor.b, PreFadeColor.a - 0.02f);
            StartCoroutine(FadeOut(FadeMe, WaitTime));
        }
        else if(FadeMe != null)
        {
            Destroy(FadeMe);
        }
    }
    void Fire() //bullet physics, properties, spawn points
    {
        if (ShotCDTimer > ShotSpeed && AmmoCount > 0)
        {
            AmmoRegenTimer = 0;
            Vector3 FirePoint3 = Barrel.transform.position + Barrel.transform.rotation * new Vector3(0.55f, 0, 0);
            GameObject Bullet = (GameObject)Instantiate(BulletPrefab, FirePoint3, Barrel.transform.rotation);
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
            AmmoCount -= 2.5f;
        }
    }
    private void BulletCharacteristics(GameObject bullet) //physics that applies to all bullets, in this class for useability of variables
    {
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.right * 30 * BulletVelocityModifier;
        bullet.transform.localScale = Vector3.one * BulletSize / 4;
        bullet.GetComponent<Rigidbody>().mass = BulletMass;
        StartCoroutine(FadeOut(bullet, 0.1f));
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
        else
        {
            repeats = 0;
            CancelInvoke("DoneDamage");
            List<GameObject> newlist = GetComponent<AsteroidBehavoir>().GetList("Asteroid");
            GetComponent<AsteroidBehavoir>().addEvadedAsteroids();
            newlist.Remove(AsteroidHit);
            GetComponent<AsteroidBehavoir>().SetList("Asteroid", newlist);
            Destroy(AsteroidHit, 0);
        }
    }
    void UpdateBarUI()
    {
        for (int i = 0; i < HealthBar.transform.childCount; i++) //foreach health activate one hp bar
        {
            if (i < PlayerHealth)
            {
                HealthBar.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                HealthBar.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        for (int i = 1; i <= AmmoBar.transform.childCount; i++) //foreach 3 ammo activate one ammo bar
        {
            if (i * 3 <= AmmoCount)
            {
                AmmoBar.transform.GetChild(i-1).gameObject.SetActive(true);
            }
            else
            {
                AmmoBar.transform.GetChild(i-1).gameObject.SetActive(false);
            }
        }
    }
    IEnumerator StartAudio()
    {
        foreach (var track in Resources.LoadAll("Music", typeof(AudioClip)))
        {
            Music.clip = ((AudioClip)track);
            Music.Play();
            yield return new WaitForSeconds(Music.clip.length);
        }
        StartCoroutine(StartAudio());
    }
}
/* music;
 * space-boss-battle-theme Matthew Pablo
 * wheres-my-spaceship spuispuin
 * spacebossbattle Hitctrl
 * hypersspace MidFag
 * less-appealing Macro
 * through-space maxstack
 * space-music mrpoly
 * misc sound effects;
 * 8-bit-sound-effect-pack-vol-001 Xenocity
 */
