using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuControl : MonoBehaviour
{
    public bool gameIsPaused = false;
    public float originalTimescale;
    public int originalFrameRate;
    public GameObject pauseMenu;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }


    public void Resume()
    {
        Application.targetFrameRate = originalFrameRate;
        Time.timeScale = originalTimescale;
        gameIsPaused = false;
        pauseMenu.active = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Pause()
    {
        originalFrameRate = Application.targetFrameRate;
        Application.targetFrameRate = 60;
        originalTimescale = Time.timeScale;
        Time.timeScale = 0f;
        gameIsPaused = true;
        pauseMenu.active = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void Exit()
    {
        Resume();
        Cursor.lockState = CursorLockMode.Confined;
        ViewModel.Instance.LoadLevel("MainMenu");
    }
}
