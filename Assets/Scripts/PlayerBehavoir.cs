﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerBehavoir : MonoBehaviour
{
    private int asteroidsondeath = 0, PlayerHealth = 3, HighScore = 0, FireReps = 0, RunScore;
    private GameObject EndUI, Score, Barrel, AsteroidHit, InstructionsUI, GameStartText, AmmoBar, HealthBar, Music, ExitGameButton;
    public GameObject BulletPrefab; // assign in inspector
    private float BulletSize = 1, BulletMass = 1, ShotSpeed = 0.125f, BulletVelocityModifier = 0.75f, ShotCDTimer = 0, repeats = 0, AmmoRegenTimer = 0, MaxAmmo = 30, AmmoCount, BarrelMoveSpeed = 150, AmmoWait = 0.5f;
    private bool InvincibilityFrames = false, TripleShot = false;
    internal static bool GameStarted = false;
    internal float LastFrameTime, FakeDT;
    private string SplashString;

    void Start()
    {
        AmmoCount = MaxAmmo; //set ammo
        GameStartText = GameObject.Find("GameStartText"); //set refs
        EndUI = GameObject.Find("EndUI");
        InstructionsUI = GameObject.Find("InstructionsUI");
        Score = GameObject.Find("Score");
        Barrel = GameObject.Find("Barrel");
        EndUI.GetComponent<Text>().text = string.Empty; //initialised as blank to get rid of the filler text
        HighScore = PlayerPrefs.GetInt("highscore"); //playerprefs persist through scene reloads
        AmmoBar = GameObject.Find("GreenBackground");
        HealthBar = GameObject.Find("RedBackground");
        Music = GameObject.Find("Music");
        ExitGameButton = GameObject.Find("Exit Game");
        ExitGameButton.GetComponent<Button>().onClick.AddListener(ExitGame);
        ExitGameButton.SetActive(false);
        StartCoroutine(StartAudio()); //play music
        UpdateBarUI();
    }

    void Update()
    {
        FakeDT = Time.realtimeSinceStartup - LastFrameTime; //realtimesincestartup isnt affected by timescale manipulation, this is accurate to 3decimal places, but isnt exactly the same as 
        if ((!GameStarted) && Input.anyKey) //player has started game
        {
            GameStarted = (!GameStarted);
            GameStartText.SetActive(false);
        }
        if (GameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) //pause the game
            {
                Time.timeScale = Time.timeScale == 1 ? 0 : 1; //ternary operator for the speed that time runs at
                if (Time.timeScale == 0)
                {
                    ExitGameButton.SetActive(true);
                }
                else
                {
                    ExitGameButton.SetActive(false);
                }
            }
            if (Time.timeScale != 0) //game isnt paused
            {
                AmmoRegenTimer += Time.deltaTime; //wait for regen to start
                if (AmmoRegenTimer > AmmoWait && AmmoCount < MaxAmmo) //start regening ammo
                {
                    AmmoCount += 1f;
                }
                UpdateBarUI();
                ShotCDTimer += Time.deltaTime; //shotspeed incrememnt
                if (Time.time > 15)
                {
                    InstructionsUI.transform.position = Vector3.MoveTowards(InstructionsUI.transform.position, new Vector3(InstructionsUI.transform.position.x, InstructionsUI.transform.position.y + 20, InstructionsUI.transform.position.z), Time.deltaTime);
                }
                if (IsPlayerDead() == true) //do the end game ui's
                {
                    NewHighScore();
                    StartCoroutine(WaitFor(Vector3.zero, 10, "ResetScene"));
                }
                else
                {
                    if (Input.GetKey(KeyCode.Space))
                    {
                        Fire();
                    }
                    if (Input.GetKey(KeyCode.LeftArrow))
                    {
                        BarrelRotate(Vector3.forward);
                    }
                    else if (Input.GetKey(KeyCode.RightArrow))
                    {
                        BarrelRotate(-Vector3.forward);
                    }
                    else
                    {
                        BarrelMoveSpeed = 100;
                    }
                }
            }
        }
        LastFrameTime = Time.realtimeSinceStartup; //last frame time ref
    }
    private void BarrelRotate(Vector3 Rd)
    {
        Barrel.transform.RotateAround(GameObject.Find("outer").GetComponent<Collider>().bounds.center, Rd, BarrelMoveSpeed * Time.deltaTime); //move barrel by move speed var
        if (BarrelMoveSpeed < 450)
        {
            BarrelMoveSpeed += 15f;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Asteroid" && InvincibilityFrames == false) //if player is hit
        {
            PlayerHealth--;
            AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("Sound Effects/Hit"), GameObject.Find("Music").transform.position, 0.35f);
            AsteroidHit = col.gameObject;
            InvokeRepeating("DoneDamage", 0, 1); //asteroid flashes and loses its collider
            StartCoroutine(WaitFor(Vector3.zero, 0.5f, "InvincibilityFrames")); //cant get hit immediatly after getting hit
        }
    }

    public void PowerUpEffect(Vector3 pos)
    {
        AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("Sound Effects/Powerup"), GameObject.Find("Music").transform.position, 0.35f);
        var x = UnityEngine.Random.Range(0, 10);
        switch (x)
        {
            case 0:
                GetComponent<AsteroidBehavoir>().addScore((int)(Mathf.Abs(GetComponent<AsteroidBehavoir>().getScore() * 0.05f))); //add score
                SplashString = "More Points!";
                StartCoroutine(WaitFor(pos));
                break;
            case 1:
                if (PlayerHealth < 5) //add max hp if not at maxhp
                {
                    PlayerHealth++;
                    SplashString = "Health Up!";
                    StartCoroutine(WaitFor(pos));
                }
                else PowerUpEffect(pos);
                break;
            case 2:
                for (var i = 0; i < 36; i++) //spawn 36 projectiles in a circle
                {
                    Vector3 pos1 = CircleSpawn(Vector3.zero, 3, i * 10); //shoots bullets in a circle around the player
                    Quaternion rot = Quaternion.FromToRotation(-Vector3.right, Vector3.zero - pos1);
                    GameObject Bullet = (GameObject)Instantiate(BulletPrefab, pos1, rot);
                    BulletCharacteristics(Bullet);
                    Destroy(Bullet, 2);
                }
                SplashString = "Bullet Blitz";
                StartCoroutine(WaitFor(pos));
                break;
            case 3:
                InvokeRepeating("BulletRing", 0, 0.03f); //shoots bullets in a timed rotation
                SplashString = "Bullet Wave";
                StartCoroutine(WaitFor(pos));
                break;
            case 4:
                SplashString = "Pack a Punch"; //makes bullets large
                StartCoroutine(WaitFor(pos, 3, "BigBullet"));
                break;
            case 5:
                SplashString = "TripleShot"; //shoot triple shot for a short time
                StartCoroutine(WaitFor(pos, 5, "TripleShot"));
                break;
            case 6:
                SplashString = "ShotSpeedUp";
                StartCoroutine(WaitFor(pos, 5, "ShotSpeedUp"));
                break;
            case 7:
                foreach (GameObject Asteroid in GetComponent<AsteroidBehavoir>().GetList("Asteroid")) //freeze all asteroids
                {
                    if (Asteroid != null)
                    {
                        Asteroid.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    }
                }
                SplashString = "Freeze";
                StartCoroutine(WaitFor(pos));
                
                break;
            case 8:
                GetComponent<AsteroidBehavoir>().setShieldBool(true); //activate shield
                SplashString = "Shield Activated";
                StartCoroutine(WaitFor(pos));
                break;
            case 9:
                if (MaxAmmo < 45) //if not at max ammo add max ammo
                {
                    MaxAmmo += 3;
                    SplashString = "Ammo Up!";
                    StartCoroutine(WaitFor(pos));
                }
                else PowerUpEffect(pos);
                break;
            default:
                SplashString = "error in powerupeffect";
                StartCoroutine(WaitFor(Vector3.zero));
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
        if (FireReps >= 36)
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
    public IEnumerator WaitFor(Vector3 pos, float WaitTime = 0, string callref = "") //varios timed function calls
    {
        GetComponent<AsteroidBehavoir>().addScore((int)(Mathf.Abs(GetComponent<AsteroidBehavoir>().getScore() * 0.01f)));
        if (pos.x < -423) //is poweruptext offscreen?
        {
            pos.x = -423;
        }
        else if (pos.x > -375)
        {
            pos.x = -375;
        }
        if (pos.y < -210)
        {
            pos.y = -210;
        }
        else if (pos.y > -235)
        {
            pos.y = -235;
        }
        Quaternion quat;
        if (pos.y < transform.position.y)
        {
            quat = Quaternion.LookRotation(transform.forward, transform.position - pos); //rotate textbox
        }
        else
        {
            quat = Quaternion.LookRotation(transform.forward, pos - transform.position);
        }
        GameObject SplashText = Instantiate((GameObject)Resources.Load("SplashText"), pos, quat , GameObject.Find("Canvas").transform);
        SplashText.GetComponent<Text>().text = SplashString;
        switch (callref)
        {
            case "ResetScene":
                asteroidsondeath = gameObject.GetComponent<AsteroidBehavoir>().GetEvadedAsteroids();
                EndUI.GetComponent<Text>().text = "Score : " + RunScore + System.Environment.NewLine + "Session HighScore : " + HighScore + System.Environment.NewLine + "Asteroids Cleared : " + asteroidsondeath;
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
            default:
                break;
        }
        yield return new WaitForSeconds(3f);
        if (SplashText != null)
        {   
            Destroy(SplashText);
        }
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
    private void DoneDamage() //phase flash asteroid that hit you
    {
        repeats++;
        if (repeats < 6)
        {
            Rigidbody ARB = AsteroidHit.GetComponent<Rigidbody>();
            ARB.isKinematic = true;
            ARB.velocity = Vector3.zero;
            AsteroidHit.GetComponent<Collider>().enabled = false;
            AsteroidHit.gameObject.SetActive(AsteroidHit.gameObject.activeSelf == true ? false : true);
        }
        else
        {
            repeats = 0;
            CancelInvoke("DoneDamage");
            AsteroidBehavoir AB = GetComponent<AsteroidBehavoir>();
            List<GameObject> newlist = AB.GetList("Asteroid");
            AB.addEvadedAsteroids();
            newlist.Remove(AsteroidHit);
            AB.SetList("Asteroid", newlist);
            Destroy(AsteroidHit, 0);
        }
    }
    void UpdateBarUI()
    {
        bool Display;
        for (int i = 0; i < HealthBar.transform.childCount; i++) //foreach health activate one hp bar
        {
            if (i < PlayerHealth)
            {
                Display = true;
            }
            else
            {
                Display = false;
            }
            HealthBar.transform.GetChild(i).gameObject.SetActive(Display);
        }
        for (int i = 1; i <= AmmoBar.transform.childCount; i++) //foreach 3 ammo activate one ammo bar
        {
            if (i * 3 <= AmmoCount)
            {
                Display = true;
            }
            else
            {
                Display = false;
            }
            AmmoBar.transform.GetChild(i - 1).gameObject.SetActive(Display);
        }
    }
    IEnumerator StartAudio()
    {
        AudioSource music = Music.GetComponent<AudioSource>();
        foreach (var track in Resources.LoadAll("Music", typeof(AudioClip)))
        {
            music.clip = ((AudioClip)track);
            music.Play();
            yield return new WaitForSeconds(music.clip.length);
        }
        StartCoroutine(StartAudio());
    }
    internal void ExitGame()
    {
        Application.Quit();
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
