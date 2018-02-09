using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.PostProcessing;

public class PlayerBehavoir : MonoBehaviour
{
    [SerializeField]
    private GameObject EndUI, Score, Barrel, InstructionsUI, GameStartText, AmmoBar, HealthBar, Music, ExitGameButton, PauseScreen, MusicVolSlider, SFXVolSlider, Tips, Level, BulletPrefab, ScoreName;
    [SerializeField]
    private PostProcessingProfile ppb;
    private float BulletSize = 1, BulletMass = 2, ShotSpeed = 0.125f, BulletVelocityModifier = 0.75f, ShotCDTimer = 0, AmmoRegenTimer = 0, MaxAmmo = 27, AmmoCount, BarrelMoveSpeed = 150, AmmoWait = 0.5f;
    private bool InvincibilityFrames = false, TripleShot = false;
    private static int OnLevel = 1, MaxHP = 5, AmmoCap = 45;
    private ScoreFormat ThisScore;
    internal static int AsteroidsEvaded = 0, PlayerHealth = 3;
    internal static float LevelTime, MaxRotationSpeed = 450;
    internal static bool GameStarted = false, isInput = false, ResettingScene = false;
    
    void Start()
    {
        ThisScore = new ScoreFormat();
        ScoreName = transform.Find("Canvas/ScoreName").gameObject;
        ScoreName.GetComponent<InputField>().onEndEdit.AddListener(delegate{IsInput(ScoreName.GetComponent<InputField>());});
        MusicVolSlider.GetComponent<Slider>().value = SaveLoad.musicVol;
        SFXVolSlider.GetComponent<Slider>().value = SaveLoad.fXVol;
        Tips.SetActive(false);
        PauseScreen.SetActive(false);
        ScoreName.SetActive(false);
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
        var ppp = ppb.colorGrading.settings;
        SaveLoad.hueShift += 0.1f;
        ppp.basic.hueShift = SaveLoad.hueShift;
        ppb.colorGrading.settings = ppp;
        if ((!GameStarted) && Input.anyKey) //player has started game
        {
            GameStarted = true;
            GameStartText.SetActive(false);
            LevelTime = Time.time;
        }
        if (GameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !ResettingScene && PlayerHealth > 0) //pause the game
            {
                PauseGame();
            }
            if (Time.timeScale != 0) //game isnt paused
            {
                AmmoRegenTimer += Time.deltaTime; //wait for regen to start
                ShotCDTimer += Time.deltaTime; //shotspeed incrememnt
                transform.GetChild(1).GetChild(0).GetChild(0).localScale = (Vector3.one/(OnLevel + 0.5f)) * (((Time.time - LevelTime))/ (50 * OnLevel) + OnLevel);
                if ((Time.time - LevelTime) >= (50 * OnLevel) + OnLevel && PlayerHealth > 0) //Level Up
                {
                    LevelUp();
                }
                if (InstructionsUI.transform.position.y < 40 && Time.timeSinceLevelLoad > 8)
                {   
                    InstructionsUI.transform.position += Vector3.up / 50;
                }
                if (AmmoRegenTimer > AmmoWait && AmmoCount < MaxAmmo) //start regening ammo
                {
                    AmmoCount += 1.25f;
                }
                UpdateBarUI();
                if (PlayerHealth <= 0 && ResettingScene == false) //do the end game ui's
                {
                    EndGameUI();
                }
                else
                {
                    if (Input.GetKey(KeyCode.Space))
                    {
                        Fire();
                    }
                    if (Input.GetKey(KeyCode.LeftArrow))
                    {
                        Barrel.transform.GetChild(2).GetComponent<ParticleSystem>().Play();
                        BarrelRotate(Vector3.forward);
                    }
                    else if (Input.GetKey(KeyCode.RightArrow))
                    {
                        Barrel.transform.GetChild(1).GetComponent<ParticleSystem>().Play();
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
    private void PauseGame()
    {
        Time.timeScale = Time.timeScale == 1 ? 0 : 1; //ternary operator for the speed that time runs at
        if (Time.timeScale == 0)
        {
            if (InstructionsUI.transform.position.y > 40) //out of the way?
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
    private void EndGameUI()
    {
        ScoreName.SetActive(true);
        ThisScore.Level = OnLevel;
        ThisScore.Score = AsteroidBehavoir.Score;
        float Highscore = ThisScore.Score;
        if (SaveLoad.scores != null && SaveLoad.scores.Count > 0 && SaveLoad.scores[SaveLoad.scores.Count - 1].Score > Highscore)
        {
            Highscore = SaveLoad.scores[SaveLoad.scores.Count -1].Score;
        }
        EndUI.GetComponent<Text>().text = "RunScore = " + ThisScore.Score + System.Environment.NewLine + "Asteroids Evaded = " + AsteroidsEvaded
            + System.Environment.NewLine + "Highscore = " + Highscore;
    }
    private void BarrelRotate(Vector3 Rd)
    {
        Barrel.transform.RotateAround(GameObject.Find("outer").GetComponent<Renderer>().bounds.center, Rd, BarrelMoveSpeed * Time.deltaTime); //move barrel by move speed var
        if (BarrelMoveSpeed < MaxRotationSpeed)
        {
            BarrelMoveSpeed += 20f;
        }
    }
    private string GetTip()
    {
        switch (Random.Range(0, 7)) //although this is a extent of 6, the integer function of random range is (inclusive, exclusive) 
        {
            case 0:
                return "Asteroid Fragments are too small to damage your ship.";
            case 1:
                return "Push asteroids as far away as possible to better your odds."; 
            case 2:
                return "Increased fire rate also makes your ammo infinite.";
            case 3:
                return "Triple Shot decreases the ammo loss rate by exactly 1/3."; //tips should be no longer than this
            case 4:
                return "Your deflector pellets do not damage your ship."; //tips should be no longer than this
            case 5:
                return "Your barrel can deflect asteroids."; //tips should be no longer than this
            default:
                return "Aim for coloured powerups.";
        }
    }
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Asteroid" && InvincibilityFrames == false && PlayerHealth >= 0) //if player is hit
        {
            PlayerHealth--;
            AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("Sound Effects/Hit"), Music.transform.position, SaveLoad.fXVol);
            col.gameObject.GetComponent<Rigidbody>().velocity = (transform.position - col.gameObject.transform.position).normalized * 5;
            StartCoroutine(WaitFor(0.5f, "InvincibilityFrames")); //cant get hit immediatly after getting hit
            if (Random.value >= 0.5)
            {
                transform.GetChild(5).position = col.contacts[0].point;
                transform.GetChild(5).GetComponent<ParticleSystem>().Play();
            }
            else
            {
                transform.GetChild(4).position = col.contacts[0].point;
                transform.GetChild(4).GetComponent<ParticleSystem>().Play();
            }
        }
    }
    internal void PowerUpEffect(Vector3 pos, Color col)
    {
        col.a = 255;
        AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("Sound Effects/Powerup"), Music.transform.position, SaveLoad.fXVol);
        var x = UnityEngine.Random.Range(0, 10);
        switch (x)
        {
            case 0:
                AsteroidBehavoir.Score += (int)(Mathf.Abs(AsteroidBehavoir.Score * 0.05f)); //add score
                StartCoroutine(DoSplash(pos, col, "More Points!"));
                break;
            case 1:
                if (PlayerHealth < MaxHP)
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
                    Quaternion rot = Quaternion.FromToRotation(transform.right, transform.position - pos1);
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
                foreach (GameObject Asteroid in AsteroidBehavoir.AsteroidList) //freeze all asteroids
                {
                    if (Asteroid != null)
                    {
                        Asteroid.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    }
                }
                StartCoroutine(DoSplash(pos, col, "Freeze"));
                break;
            case 8:
                if (MaxAmmo < AmmoCap) //maxammo is current max, ammo cap is highest possible val
                {
                    MaxAmmo += 3;
                    StartCoroutine(DoSplash(pos, col, "Ammo Up!"));
                }
                else PowerUpEffect(pos, col);
                break;
            case 9:
                StartCoroutine(WaitFor(5, "MASSive"));
                StartCoroutine(DoSplash(pos, col, "MASSive"));
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
    private IEnumerator BulletRing(float Reps)
    {
        int FireReps = 0;
        for (int i = 0; i < Reps * 36; i++)
        {
            yield return new WaitForSeconds(1/36);
            var Bullet = (GameObject)Instantiate(BulletPrefab, CircleSpawn(transform.position, 3, FireReps * 10), Quaternion.FromToRotation(-transform.right, transform.position - CircleSpawn(transform.position, 3, FireReps * 10)));
            BulletCharacteristics(Bullet);
            FireReps++;
            Bullet.GetComponent<Rigidbody>().mass = 10;
            Bullet.GetComponent<Rigidbody>().velocity = -Bullet.GetComponent<Rigidbody>().velocity / 5; //bullet ring fires slower bullets
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
        GameObject SplashText = Instantiate((GameObject)Resources.Load("Prefabs/SplashText"), pos, quat, GameObject.Find("Canvas").transform);
        Text STT = SplashText.GetComponent<Text>();
        STT.color = col;
        STT.text = SplashString;
        SplashText.transform.position = pos;
        yield return new WaitForSeconds(5);
        Destroy(SplashText);
    }
    internal IEnumerator WaitFor(float WaitTime = 0, string callref = "") //varios timed function calls
    {
        switch (callref)
        {
            case "ResetScene":
                ResettingScene = true;
                do
                {
                    yield return null;
                } while (!isInput); //wait for player to input his name for the score board
                OnLevel = 1;
                MaxHP = 5;
                AsteroidsEvaded = 0;
                PlayerHealth = 3;
                LevelTime = 0;
                MaxRotationSpeed = 450;
                AsteroidBehavoir.PowerUpSpawnRate = 5;
                AsteroidBehavoir.AsteroidSpawnRate = 3f;
                AsteroidBehavoir.SpawnRateCap = 1.5f;
                AsteroidBehavoir.Score = 0;
                AsteroidBehavoir.AsteroidList.Clear();
                GameStarted = false;
                isInput = false;
                ResettingScene = false;
                EndUI.GetComponent<Text>().text = string.Empty;
                ScoreName.SetActive(false);
                SceneManager.LoadScene("Play");
                break;
            case ("InvincibilityFrames"):
                InvincibilityFrames = true;
                yield return new WaitForSeconds(WaitTime);
                InvincibilityFrames = false;
                break;
            case ("BigBullet"):
                BulletMass = 6;
                BulletSize = 3;
                AmmoWait = 0.15f;
                yield return new WaitForSeconds(WaitTime);
                BulletMass = 2;
                BulletSize = 1;
                break;
            case ("TripleShot"):
                TripleShot = true;
                AmmoWait = 0.15f;
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
            case ("MASSive"):
                BulletMass = 100;
                yield return new WaitForSeconds(WaitTime);
                BulletMass = 2;
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
            Barrel.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
            AmmoRegenTimer = 0;
            Vector3 FirePoint3 = Barrel.transform.position + -Barrel.transform.right * 3.5f;
            GameObject Bullet = (GameObject)Instantiate(BulletPrefab, FirePoint3, Barrel.transform.rotation * Quaternion.Euler(90,0,0));
            Physics.IgnoreCollision(Bullet.GetComponent<Collider>(), Barrel.GetComponent<Collider>(), true);
            BulletCharacteristics(Bullet);
            if (TripleShot == true)
            {
                Vector3 FirePoint = Barrel.transform.position + -Barrel.transform.right * 4 -Barrel.transform.forward * 2, FirePoint2 = Barrel.transform.position + -Barrel.transform.right * 4 + Barrel.transform.forward *2; //pos L&R of normal shooting, world space to local space
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
        bullet.GetComponent<Rigidbody>().velocity = -bullet.transform.right * 30 * BulletVelocityModifier;
        bullet.transform.localScale = Vector3.one * BulletSize / 4;
        bullet.GetComponent<Rigidbody>().mass = BulletMass;
        StartCoroutine(FadeOut(bullet, 0.03f));
    }
    internal bool GetIsInvincible()
    {
        return InvincibilityFrames;
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
        for (int i = 1; i < transform.GetChild(1).childCount - 1; i++) //foreach health activate one hp bar
        {
            if (i < PlayerHealth +1)
            {
                Display = true;
            }
            else
            {
                Display = false;
            }
            transform.GetChild(1).GetChild(i).gameObject.SetActive(Display);
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
        if (PlayerHealth > 0)
        {
            if (AsteroidBehavoir.PowerUpSpawnRate <= 12.5)
            {
                AsteroidBehavoir.PowerUpSpawnRate += 0.15f;
            }
            transform.GetChild(1).GetChild(0).GetChild(0).localScale = Vector3.one/10;
            AsteroidBehavoir.SpawnRateCap -= 0.01f;
            DoSplash(transform.position + Vector3.up, Color.red, "LevelUp");
            AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("Sound Effects/LevelUp"), Music.transform.position, SaveLoad.fXVol);
            foreach (GameObject asteroid in AsteroidBehavoir.AsteroidList)
            {
                if (asteroid != null)
                {
                    asteroid.GetComponent<Rigidbody>().velocity = (asteroid.transform.position - transform.position).normalized * 10;

                }
            }
            AsteroidBehavoir.Score += (int)Mathf.Round(Time.time - LevelTime);
            OnLevel++;
            if (AsteroidBehavoir.SpawnRateCap > 0.5f)
            {
                AsteroidBehavoir.SpawnRateCap = -0.25f;
            }
            AsteroidBehavoir.AsteroidSpawnRate = 3.5f;
            Level.GetComponent<Text>().text = "Level " + OnLevel;
        }
    }
    internal void IsInput(InputField IF)
    {
        isInput = true;
        ThisScore.Name = IF.text;
        if (SaveLoad.scores == null)
        {
            SaveLoad.scores = new List<ScoreFormat>();
        }
        SaveLoad
            .scores
            .Add(
            ThisScore);
        AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("Sound Effects/GameOver"), Music.transform.position, SaveLoad.fXVol);
        StartCoroutine(WaitFor(callref: "ResetScene"));
    }
}
