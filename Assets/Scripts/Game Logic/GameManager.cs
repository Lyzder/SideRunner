using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private bool isPaused;
    private int score;
    private short stage;
    private Vector3 playerSpawn;
    private Vector3 targetSpawnPosition;
    private bool shouldSpawnPlayer = false;
    private string previousScene;
    public PlayerController player;
    private short currentHp, currentAmmo;
    private Boss boss;
    public float winTime;
    [Header("Prefabs")]
    [SerializeField] GameObject hudPrefab;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject eventSystem;
    private GameObject activeHud;
    // Events
    public event Action<int> OnScoreChanged;
    public event Action<PlayerController> OnPlayerRegistered;
    public event Action OnWinning;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
        Instantiate(eventSystem);
    }

    // Start is called before the first frame update
    void Start()
    {
        currentAmmo = 3;
        currentHp = 5;
        AudioManager.Instance.PlaySceneBgm();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
        if (boss != null)
            boss.OnDeath -= TriggerVictory;
    }

    public void PauseGame()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
    }

    public void LoadSceneAndSpawnPlayer(string sceneName, Vector3 spawnPosition)
    {
        targetSpawnPosition = spawnPosition;
        shouldSpawnPlayer = true;
        previousScene = SceneManager.GetActiveScene().name;

        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Load the scene (additive or single depending on your needs)
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    public void LoadScene(string sceneName)
    {
        previousScene = SceneManager.GetActiveScene().name;

        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Load the scene (additive or single depending on your needs)
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe to avoid being called again for future scenes
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.SetActiveScene(scene);
        SceneManager.UnloadSceneAsync(previousScene);

        if (!shouldSpawnPlayer)
            return;

        GameObject playerObject = Instantiate(playerPrefab);

        playerObject.transform.position = targetSpawnPosition;
        player = playerObject.GetComponent<PlayerController>();
        shouldSpawnPlayer = false;

        activeHud = Instantiate(hudPrefab);
        player.SetAmmo(currentAmmo);
        player.SetHp(currentHp);
    }

    public void AddScore(int score)
    {
        this.score += score;
        OnScoreChanged?.Invoke(this.score);
    }

    public void RegisterPlayer(PlayerController player)
    {
        this.player = player;
        OnPlayerRegistered?.Invoke(player);
    }

    public int GetScore()
    {
        return score;
    }

    public void RegisterBoss(Boss boss)
    {
        if (this.boss != null)
        {
            this.boss.OnDeath -= TriggerVictory;
        }

        this.boss = boss;

        this.boss.OnDeath += TriggerVictory;
    }

    private void TriggerVictory()
    {
        StartCoroutine(WinSequence());
    }

    private IEnumerator WinSequence()
    {
        float elapsed = 0f;

        while (elapsed < winTime)
        {
            elapsed += Time.deltaTime;
            yield return null; // wait for next rendered frame
        }

        OnWinning?.Invoke();
    }

    public void ResetStats()
    {
        score = 0;
        currentAmmo = 3;
        currentHp = 5;
    }

    public void StoreStats()
    {
        currentAmmo = player.GetAmmo();
        currentHp = player.GetHp();
    }

    public void TransitionLevel(string sceneName, Vector3 coordinates)
    {
        StoreStats();
        LoadSceneAndSpawnPlayer(sceneName, coordinates);
    }
}
