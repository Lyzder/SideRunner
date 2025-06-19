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
    [Header("Prefabs")]
    [SerializeField] GameObject hudPrefab;
    [SerializeField] GameObject playerPrefab;
    private GameObject activeHud;
    // Events
    public event Action<int> OnScoreChanged;
    public event Action<PlayerController> OnPlayerRegistered;

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
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe to avoid being called again for future scenes
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.SetActiveScene(scene);

        if (!shouldSpawnPlayer)
            return;

        // Try to find the player in the new scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            player = Instantiate(playerPrefab);
        }

        player.transform.position = targetSpawnPosition;
        shouldSpawnPlayer = false;

        activeHud = Instantiate(hudPrefab);
        SceneManager.UnloadSceneAsync(previousScene);
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
}
