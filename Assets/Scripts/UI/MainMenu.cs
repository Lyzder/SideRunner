using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject instructions;
    [SerializeField] GameObject credits;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        GameManager.Instance.ResetStats();
        GameManager.Instance.LoadSceneAndSpawnPlayer("Level1", new Vector3(-7, -0.4f));
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ShowInstructions()
    {
        mainMenu.SetActive(false);
        instructions.SetActive(true);
        credits.SetActive(false);
    }

    public void ShowCredits()
    {
        mainMenu.SetActive(false);
        instructions.SetActive(false);
        credits.SetActive(true);
    }

    public void BackToMenu()
    {
        mainMenu.SetActive(true);
        instructions.SetActive(false);
        credits.SetActive(false);
    }
}
