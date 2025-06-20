using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    [Header("Hud Components")]
    [SerializeField] private Image[] hpBar;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private TextMeshProUGUI scoreResult;
    [Header("Sprites")]
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;
    // References
    private PlayerController player;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateScore(GameManager.Instance.GetScore());
    }

    private void OnEnable()
    {
        GameManager.Instance.OnPlayerRegistered += HandlePlayerRegistered;
        GameManager.Instance.OnScoreChanged += UpdateScore;
        GameManager.Instance.OnWinning += ShowWinScreen;

        // If a player is already registered (e.g., on scene reload), subscribe immediately
        if (GameManager.Instance.player != null)
        {
            HandlePlayerRegistered(GameManager.Instance.player);
        }
    }

    private void OnDisable()
    {
        GameManager.Instance.OnPlayerRegistered -= HandlePlayerRegistered;
        GameManager.Instance.OnScoreChanged -= UpdateScore;
        GameManager.Instance.OnWinning -= ShowWinScreen;

        if (player != null)
        {
            player.OnHealthChanged -= UpdateHealth;
            player.OnAmmoChanged -= UpdateAmmo;
            player.OnDeath -= ShowLoseScreen;
        }
    }

    private void UpdateHealth(short health)
    {
        foreach (Image sprite in hpBar)
        {
            if (health > 0)
            {
                sprite.sprite = fullHeart;
                health -= 1;
            }
            else
            {
                sprite.sprite = emptyHeart;
            }
        }
    }

    private void UpdateAmmo(short ammo)
    {
        ammoText.text = ammo.ToString();
    }

    private void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }

    private void HandlePlayerRegistered(PlayerController player)
    {
        if (this.player != null)
        {
            this.player.OnHealthChanged -= UpdateHealth;
            this.player.OnAmmoChanged -= UpdateAmmo;
            this.player.OnDeath -= ShowLoseScreen;
        }

        this.player = player;

        if (this.player != null)
        {
            this.player.OnHealthChanged += UpdateHealth;
            this.player.OnAmmoChanged += UpdateAmmo;
            this.player.OnDeath += ShowLoseScreen;
        }
    }

    private void ShowWinScreen()
    {
        scoreResult.text = "SCORE: " + GameManager.Instance.GetScore().ToString();
        winScreen.SetActive(true);
    }

    private void ShowLoseScreen()
    {
        loseScreen.SetActive(true);
    }

    public void MenuButtonClick()
    {
        GameManager.Instance.LoadScene("MainMenu");
    }

    public void RetryButtonClick()
    {
        GameManager.Instance.ResetStats();
        GameManager.Instance.LoadSceneAndSpawnPlayer("Level1", new Vector3(-7, -0.4f));
    }
}
