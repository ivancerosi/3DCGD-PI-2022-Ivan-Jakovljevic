        using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ViewModel : MonoBehaviour
{
    public static ViewModel Instance;
    LinkedList<string> sceneChain = new LinkedList<string>();

    MenuControl ingameMenu;

    public bool paused = false;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        sceneChain.AddLast("MainMenu");
        sceneChain.AddLast("Level1");
        sceneChain.AddLast("Level2");
        sceneChain.AddLast("Level3");

        //SceneManager.LoadSceneAsync(sceneChain.First.Value,LoadSceneMode.Single);
    }

    public void LoadNextLevel()
    {
        string sceneToLoad = sceneChain.Find(SceneManager.GetActiveScene().name).Next.Value;
        SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);

        ingameMenu = GameObject.Find("Canvas").GetComponent<MenuControl>();
    }

    public void LoadLevel(string levelname)
    {
        SceneManager.LoadSceneAsync(levelname, LoadSceneMode.Single);
        ingameMenu = GameObject.Find("Canvas").GetComponent<MenuControl>();
    }

    public void PauseGame()
    {
        if (ingameMenu==null) ingameMenu = GameObject.Find("Canvas").GetComponent<MenuControl>();
        paused = true;
        ingameMenu.Pause();
    }

    public void ResumeGame()
    {
        if (ingameMenu == null) ingameMenu = GameObject.Find("Canvas").GetComponent<MenuControl>();
        paused = false;
        ingameMenu.Resume();
    }

    public void PressEscape()
    {
        if (paused) ResumeGame();
        else PauseGame();
    }

    private void Update()
    {
        if (ingameMenu == null) ingameMenu = GameObject.Find("Canvas").GetComponent<MenuControl>();
        paused = ingameMenu.gameIsPaused;
    }
}
