using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
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
        //GameManager.Instance.LoadSceneAndSpawnPlayer("Level1", new Vector3(-7, -0.4f));
        GameManager.Instance.LoadSceneAndSpawnPlayer("Level2", new Vector3(-6.25f, -1.5f));
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
