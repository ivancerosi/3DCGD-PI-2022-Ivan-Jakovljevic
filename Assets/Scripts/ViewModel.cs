        using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ViewModel : MonoBehaviour
{
    public static ViewModel Instance;
    LinkedList<string> sceneChain = new LinkedList<string>();

    public bool isInMainMenu = true;

    MenuControl ingameMenu;

    public bool paused = false;
    public Canvas canvas;

    public float ambientVolume=-1f;
    public float sfxVolume;
    public AudioSource ambientMusic;

    public float originalTimescale;
    public int originalFrameRate;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(canvas);
        sceneChain.AddLast("MainMenu");
        sceneChain.AddLast("Level1");
        sceneChain.AddLast("Level2");
        sceneChain.AddLast("Level3");

        //SceneManager.LoadSceneAsync(sceneChain.First.Value,LoadSceneMode.Single);
        ambientVolume = PlayerPrefs.GetFloat("ambientVolume", 0.5f);
        ambientMusic.Play();
        ambientMusic.volume = ambientVolume;


        Screen.SetResolution(PlayerPrefs.GetInt("screenWidth"), PlayerPrefs.GetInt("screenHeight"),(FullScreenMode)PlayerPrefs.GetInt("fullscreen"));
        QualitySettings.shadowCascades = PlayerPrefs.GetInt("shadows");
        QualitySettings.masterTextureLimit = PlayerPrefs.GetInt("texture");
    }

    private void Start()
    {
        if (ambientVolume == -1f)
        {
            ambientMusic.Play();
            ambientMusic.volume = ambientVolume;
        }
    }

    public bool isMainMenu()
    {
        return SceneManager.GetActiveScene().name == "MainMenu";
    }

    public void setAmbientVolume(float vol)
    {
        ambientVolume = vol;
        PlayerPrefs.SetFloat("ambientVolume", ambientVolume);
        PlayerPrefs.Save();
        ambientMusic.volume = vol;
    }
    public void setSfxVolume(float vol)
    {
        sfxVolume = vol;
        PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    public void LoadNextLevel()
    {
        canvas.enabled = false;
        isInMainMenu = false;
        string sceneToLoad = sceneChain.Find(SceneManager.GetActiveScene().name).Next.Value;
        SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);

        ingameMenu = GameObject.Find("Canvas").GetComponent<MenuControl>();
    }

    public void LoadLevel(string levelname)
    {
        canvas.enabled = false;
        isInMainMenu = false;
        if (levelname == "MainMenu") isInMainMenu = true;
        SceneManager.LoadSceneAsync(levelname, LoadSceneMode.Single);
        ingameMenu = GameObject.Find("Canvas").GetComponent<MenuControl>();
        ambientMusic.Stop();
        ambientMusic.Play();
    }

    public void PauseGame()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") return;
        paused = true;

        originalFrameRate = Application.targetFrameRate;
        Application.targetFrameRate = 60;
        originalTimescale = Time.timeScale;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.Confined;

        ambientMusic.Pause();
        canvas.enabled = true;
    }

    public void ResumeGame()
    {
        canvas.enabled = false;
        paused = false;
        Application.targetFrameRate = originalFrameRate;
        Time.timeScale = originalTimescale;
        Cursor.lockState = CursorLockMode.Locked;
        ambientMusic.UnPause();
    }

    public void PressEscape()
    {
        if (paused) ResumeGame();
        else PauseGame();
    }

    private void Update()
    {
        if (isInMainMenu) return;
        if (ingameMenu == null) ingameMenu = GameObject.Find("Canvas").GetComponent<MenuControl>();
        paused = ingameMenu.gameIsPaused;
    }

    public void setResolution(int width, int height, FullScreenMode fullscreen)
    {
        PlayerPrefs.SetInt("screenWidth", width);
        PlayerPrefs.SetInt("sreenHeight", height);
        PlayerPrefs.SetInt("fullscreen", (int) fullscreen);
        PlayerPrefs.Save();
        Debug.Log($"changing res to {width}x{height}");
        Screen.SetResolution(width, height, fullscreen);
    }

    public void setShadows(int shadows)
    {
        PlayerPrefs.SetInt("shadows",shadows);
        PlayerPrefs.Save();
        QualitySettings.shadowCascades = shadows;
    }

    public void setTexture(int texture)
    {
        PlayerPrefs.SetInt("texture", texture);
        PlayerPrefs.Save();
        QualitySettings.masterTextureLimit = texture;
    }

}
