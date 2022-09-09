using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject soundMenu;
    public GameObject graphicsMenu;
    public GameObject ingameMenu;

    int[,] resolutions = new int[3,2] { { 2560,1440},{1920,1080 },{1280,720 } };
 
    private void Awake()
    {
    }


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NewGame()
    {
        ingameMenu.active = true;
        mainMenu.active = false;
        ViewModel.Instance.LoadLevel("Level1");
    }

    public void SoundSettings()
    {
        soundMenu.SetActive(true);
        GameObject menu = ViewModel.Instance.isMainMenu() ? mainMenu : ingameMenu;
        menu.SetActive(false);
        GameObject.Find("SFXVolumeSlider").GetComponent<Slider>().value = PlayerPrefs.GetFloat("sfxVolume", 0.5f);
        GameObject.Find("AmbientVolumeSlider").GetComponent<Slider>().value = PlayerPrefs.GetFloat("ambientVolume", 0.5f);
    }

    public void GraphicSettings()
    {
        graphicsMenu.SetActive(true);
        GameObject menu = ViewModel.Instance.isMainMenu() ? mainMenu : ingameMenu;
        menu.SetActive(false);
    }

    public void MainMenuBack()
    {
        soundMenu.SetActive(false);
        graphicsMenu.SetActive(false);
        if (ViewModel.Instance.isMainMenu())
        {
            mainMenu.SetActive(true);
        } else
        {
            ingameMenu.SetActive(true);
        }
    }

    public void ApplySoundSettings()
    {
        float sfxVolume = GameObject.Find("SFXVolumeSlider").GetComponent<Slider>().value;
        float ambientVolume = GameObject.Find("AmbientVolumeSlider").GetComponent<Slider>().value;

        ViewModel.Instance.setSfxVolume(sfxVolume);
        ViewModel.Instance.setAmbientVolume(ambientVolume);

        MainMenuBack();
    }

    public void ApplyGraphicSettings()
    {
        int texture=GameObject.Find("TextureDropdown").GetComponent<Dropdown>().value;
        int resolution=GameObject.Find("ResolutionDropdown").GetComponent<Dropdown>().value;
        int shadows=GameObject.Find("ShadowsDropdown").GetComponent<Dropdown>().value;

        //QualitySettings.masterTextureLimit=texture;
        //QualitySettings.shadowCascades = 4 - shadows*2;

        ViewModel.Instance.setResolution(resolutions[resolution,0], resolutions[resolution,1],Screen.fullScreenMode);
        ViewModel.Instance.setTexture(texture);
        ViewModel.Instance.setShadows(4-shadows*2);


        MainMenuBack();
    }
    public void Resume()
    {
        ViewModel.Instance.ResumeGame();
    }
    public void Exit()
    {
        ViewModel.Instance.ResumeGame();
        Cursor.lockState = CursorLockMode.Confined;
        mainMenu.active = true;
        ingameMenu.active = false;
        ViewModel.Instance.LoadLevel("MainMenu");
    }
    public void Quit()
    {
        Application.Quit();
    }
}
