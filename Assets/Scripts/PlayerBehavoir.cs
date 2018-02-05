﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class PlayerBehavoir : MonoBehaviour
{
    private static int PlayerHealth = 9999999, OnLevel = 1;
    private GameObject EndUI, Score, Barrel, InstructionsUI, GameStartText, AmmoBar, HealthBar, Music, ExitGameButton, PauseScreen, MusicVolSlider, SFXVolSlider, Tips, Level, BulletPrefab;
    private float BulletSize = 1, BulletMass = 1, ShotSpeed = 0.125f, BulletVelocityModifier = 0.75f, ShotCDTimer = 0, AmmoRegenTimer = 0, MaxAmmo = 30, AmmoCount, BarrelMoveSpeed = 150, AmmoWait = 0.5f;
    private bool InvincibilityFrames = false, TripleShot = false;
    internal static float LevelTime;
    internal static bool GameStarted = false;
    internal bool ResettingScene = false;

    void Start()
    {
        BulletPrefab = (GameObject)Resources.Load("BulletPrefab");
        Level = GameObject.Find("Level");
        GameStartText = GameObject.Find("Start"); //set refs
        EndUI = GameObject.Find("EndUI");
        InstructionsUI = GameObject.Find("Instructions");
        Score = GameObject.Find("Score");
        Barrel = GameObject.Find("Barrel");
        AmmoBar = GameObject.Find("GreenBackground");
        HealthBar = GameObject.Find("RedBackground");
        Music = GameObject.Find("Music");
        ExitGameButton = GameObject.Find("Exit Game");
        PauseScreen = GameObject.Find("Pause Screen");
        MusicVolSlider = GameObject.Find("MusicVol");
        SFXVolSlider = GameObject.Find("SFXVol");
        Tips = GameObject.Find("Tips");
        MusicVolSlider.GetComponent<Slider>().value = SaveLoad.musicVol;
        SFXVolSlider.GetComponent<Slider>().value = SaveLoad.fXVol;
        Tips.SetActive(false);
        PauseScreen.SetActive(false);
        AmmoCount = MaxAmmo; //set ammo
        EndUI.GetComponent<Text>().text = string.Empty; //initialised as blank to get rid of the filler text
        ExitGameButton.GetComponent<Button>().onClick.AddListener(ExitGame);
        StartCoroutine(StartAudio()); //play music
        UpdateBarUI();
    }

    void Update()
    {
        if (PauseScreen.activeSelf)
        {
            SaveLoad.musicVol = MusicVolSlider.GetComponent<Slider>().value;
            SaveLoad.fXVol = SFXVolSlider.GetComponent<Slider>().value;
        }
        Music.GetComponent<AudioSource>().volume = SaveLoad.musicVol;
        if ((!GameStarted) && Input.anyKey) //player has started game
        {
            GameStarted = (!GameStarted);
            GameStartText.SetActive(false);
            LevelTime = Time.time;
        }
        if (GameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) //pause the game
            {
                Time.timeScale = Time.timeScale == 1 ? 0 : 1; //ternary operator for the speed that time runs at
                if (Time.timeScale == 0)
                {
                    if (InstructionsUI.transform.position.y > 40)
                    {
                        Tips.GetComponent<Text>().text = GetTip();
                        Tips.SetActive(true);
                    }
                    PauseScreen.SetActive(true);
                }
                else
                {
                    PauseScreen.SetActive(false);
                    Tips.SetActive(false);
                }
            }
            if (Time.timeScale != 0) //game isnt paused
            {
                if ((Time.time - LevelTime) >= 100 + OnLevel)
                {
                    LevelTime = (Time.time - LevelTime);
                    LevelUp();
                }
                if (InstructionsUI.transform.position.y < 40)
                {   
                    InstructionsUI.transform.position += Vector3.up / 50;
                }
                AmmoRegenTimer += Time.deltaTime; //wait for regen to start
                if (AmmoRegenTimer > AmmoWait && AmmoCount < MaxAmmo) //start regening ammo
                {
                    AmmoCount += 1f;
                }
                UpdateBarUI();
                ShotCDTimer += Time.deltaTime; //shotspeed incrememnt
                if (IsPlayerDead() == true && ResettingScene == false) //do the end game ui's
                {
                    StartCoroutine(WaitFor(10, "ResetScene"));
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
    }
    private void BarrelRotate(Vector3 Rd)
    {
        Barrel.transform.RotateAround(GameObject.Find("outer").GetComponent<Collider>().bounds.center, Rd, BarrelMoveSpeed * Time.deltaTime); //move barrel by move speed var
        if (BarrelMoveSpeed < 450)
        {
            BarrelMoveSpeed += 15f;
        }
    }
    private string GetTip()
    {
        switch (Random.Range(0, 5)) //although this is a extent of 6, the integer function of random range is (inclusive, exclusive) 
        {
            case 0:
                return "Asteroid Fragments are too small to damage your ship.";
            case 1:
                return "Push asteroids as far away as possible for the best odds of survival."; //tips should be no longer than this
            case 2:
                return "Increased fire rate also makes your ammo infinite";
            case 3:
                return "Triple Shot decreases the ammo loss rate by exactly 1/3.";
            default:
                return "Aim for coloured powerups.";
        }
    }
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Asteroid" && InvincibilityFrames == false) //if player is hit
        {
            PlayerHealth--;
            AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("Sound Effects/Hit"), GameObject.Find("Music").transform.position, SaveLoad.fXVol);
            StartCoroutine(DoneDamage(col.gameObject));
            StartCoroutine(WaitFor(0.5f, "InvincibilityFrames")); //cant get hit immediatly after getting hit
        }
    }
    internal void PowerUpEffect(Vector3 pos, Color col)
    {
        AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("Sound Effects/Powerup"), GameObject.Find("Music").transform.position, SaveLoad.fXVol);
        var x = UnityEngine.Random.Range(0, 10);
        switch (x)
        {
            case 0:
                GetComponent<AsteroidBehavoir>().addScore((int)(Mathf.Abs(GetComponent<AsteroidBehavoir>().getScore() * 0.05f))); //add score
                StartCoroutine(DoSplash(pos, col, "More Points!"));
                break;
            case 1:
                if (PlayerHealth < 5) //add max hp if not at maxhp
                {
                    PlayerHealth++;
                    StartCoroutine(DoSplash(pos, col, "Health Up!"));
                }
                else PowerUpEffect(pos, col);
                break;
            case 2:
                for (var i = 0; i < 36; i++) //spawn 36 projectiles in a circle
                {
                    Vector3 pos1 = CircleSpawn(transform.position, 3, i * 10); //shoots bullets in a circle around the player
                    Quaternion rot = Quaternion.FromToRotation(-transform.right, transform.position - pos1);
                    GameObject Bullet = (GameObject)Instantiate(BulletPrefab, pos1, rot);
                    BulletCharacteristics(Bullet);
                    Destroy(Bullet, 2);
                }
                StartCoroutine(DoSplash(pos, col, "Bullet Blitz"));
                break;
            case 3:
                StartCoroutine(BulletRing(Random.Range(1f, 6f))); //shoots bullets in a timed rotation
                StartCoroutine(DoSplash(pos, col, "Bullet Wave"));
                break;
            case 4:
                StartCoroutine(WaitFor(3, "BigBullet"));
                StartCoroutine(DoSplash(pos, col, "Pack a Punch"));
                break;
            case 5:
                StartCoroutine(WaitFor(5, "TripleShot"));
                StartCoroutine(DoSplash(pos, col, "TripleShot"));
                break;
            case 6:
                StartCoroutine(WaitFor(5, "ShotSpeedUp"));
                StartCoroutine(DoSplash(pos, col, "ShotSpeedUp"));
                break;
            case 7:
                foreach (GameObject Asteroid in GetComponent<AsteroidBehavoir>().GetList("Asteroid")) //freeze all asteroids
                {
                    if (Asteroid != null)
                    {
                        Asteroid.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    }
                }
                StartCoroutine(DoSplash(pos, col, "Freeze"));
                break;
            case 8:
                GetComponent<AsteroidBehavoir>().setShieldBool(true); //activate shield
                StartCoroutine(DoSplash(pos, col, "Shield Activated"));
                break;
            case 9:
                if (MaxAmmo < 45) //if not at max ammo add max ammo
                {
                    MaxAmmo += 3;
                    StartCoroutine(DoSplash(pos, col, "Ammo Up!"));
                }
                else PowerUpEffect(pos, col);
                break;
            default:
                Debug.Log("Error in power up effect");
                break;
        }
    }
    internal Vector3 CircleSpawn(Vector3 center, float radius, float ang) //returns a v3 point encircling an object in 2dspace
    {
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.z = center.z;
        return pos;
    }
    private IEnumerator BulletRing(float reps)
    {
        int FireReps = 0;
        for (int i = 0; i < reps * 36; i++)
        {
            yield return new WaitForSeconds(1/36);
            var Bullet = (GameObject)Instantiate(BulletPrefab, CircleSpawn(transform.position, 3, FireReps * 10), Quaternion.FromToRotation(-transform.right, transform.position - CircleSpawn(transform.position, 3, FireReps * 10)));
            BulletCharacteristics(Bullet);
            FireReps++;
            Bullet.GetComponent<Rigidbody>().mass = 10;
            Bullet.GetComponent<Rigidbody>().velocity = Bullet.GetComponent<Rigidbody>().velocity / 5; //bullet ring fires slower bullets
            Destroy(Bullet, 2);
        }
    }
    internal bool IsPlayerDead()
    {
        if (PlayerHealth <= 0)
        {
            return true;
        }
        return false;
    }
    private void NewHighScore()
    {
        if (SaveLoad.scores != null)
        {
            ScoreFormat thisscore = new ScoreFormat();
            thisscore.Score = gameObject.GetComponent<AsteroidBehavoir>().getScore();
            SaveLoad.scores.Add(thisscore);
        }
        else
        {
            SaveLoad.Load();
        }
    }
    internal IEnumerator DoSplash(Vector3 pos, Color col, string SplashString)
    {
        Quaternion quat;
        if (pos.y < transform.position.y)
        {
            quat = Quaternion.LookRotation(transform.forward, transform.position - pos); //rotate textbox
        }
        else
        {
            quat = Quaternion.LookRotation(transform.forward, pos - transform.position);
        }
        GameObject SplashText = Instantiate((GameObject)Resources.Load("SplashText"), pos, quat, GameObject.Find("Canvas").transform);
        SplashText.GetComponent<Text>().color = col;
        SplashText.transform.position = pos;
        SplashText.GetComponent<Text>().text = SplashString;
        yield return new WaitForSeconds(5);
        Destroy(SplashText);
    }
    internal IEnumerator WaitFor(float WaitTime = 0, string callref = "") //varios timed function calls
    {
        switch (callref)
        {
            case "ResetScene":
                ResettingScene = true;
                EndUI.GetComponent<Text>().text = "Score : " + gameObject.GetComponent<AsteroidBehavoir>().getScore() + System.Environment.NewLine + "Asteroids Cleared : " + gameObject.GetComponent<AsteroidBehavoir>().GetEvadedAsteroids().ToString() + System.Environment.NewLine + "Restarting Game...";
                NewHighScore();
                yield return new WaitForSeconds(WaitTime);
                GameStarted = false;
                SceneManager.LoadScene("Play");
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
    internal void Fire() //bullet physics, properties, spawn points
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
    internal bool GetIsInvincible()
    {
        return InvincibilityFrames;
    }
    private IEnumerator DoneDamage(GameObject AsteroidHit) //phase flash asteroid that hit you
    {
            Rigidbody ARB = AsteroidHit.GetComponent<Rigidbody>();
            AsteroidBehavoir AB = GetComponent<AsteroidBehavoir>();
            ARB.isKinematic = true;
            AsteroidHit.GetComponent<Collider>().enabled = false;
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(1);
                AsteroidHit.gameObject.SetActive(AsteroidHit.gameObject.activeSelf == true ? false : true);
            }
            List<GameObject> newlist = AB.GetList("Asteroid");
            newlist.Remove(AsteroidHit);
            AB.SetList("Asteroid", newlist);
            Destroy(AsteroidHit, 0);
    }
    private void UpdateBarUI()
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
    internal IEnumerator StartAudio()
    {
        AudioSource music = Music.GetComponent<AudioSource>();
        foreach (var track in Resources.LoadAll("Music", typeof(AudioClip)))
        {
            if (track.name != "GalacticTemple")
            {
                music.clip = ((AudioClip)track);
                music.Play();
                yield return new WaitForSeconds(music.clip.length);
            }
        }
        StartCoroutine(StartAudio());
    }
    internal void ExitGame()
    {
        SaveLoad.Save();
        GameStarted = false;
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
    internal void LevelUp()
    {
        DoSplash(transform.up *5, Color.red, "LevelUp");
        AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("Sound Effects/Powerup"), GameObject.Find("Music").transform.position, SaveLoad.fXVol);
        foreach (GameObject asteroid in GetComponent<AsteroidBehavoir>().GetList("Asteroid"))
        {
            asteroid.GetComponent<Rigidbody>().velocity = (asteroid.transform.position - transform.position).normalized * 10;
        }
        GetComponent<AsteroidBehavoir>().addScore((int)Mathf.Round(Time.time - LevelTime));
        OnLevel++;
        if (AsteroidBehavoir.SpawnRateCap > 0.5f)
        {
            AsteroidBehavoir.SpawnRateCap =- 0.25f;
        }
        AsteroidBehavoir.AsteroidSpawnRate = 3.5f;
        Level.GetComponent<Text>().text = "Level " + OnLevel;
    }
}
