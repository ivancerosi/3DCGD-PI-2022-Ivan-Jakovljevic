using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatePlayerScript : MonoBehaviour
{
    public bool ran = false;
    public Camera lastCamera;
    GameObject player;

    public AudioSource cutscenePlayer;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!ran)
        {
            UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            lastCamera.enabled = false;
            foreach (GameObject obj in rootObjects)
            {
                if (obj.name=="Player")
                {
                    obj.GetComponent<PlayerMovementScript>().enabled = true;
                    obj.GetComponent<MouseLookScript>().enabled = true;
                    GameObject camera=obj.transform.Find("Main Camera").gameObject;
                    camera.GetComponent<Camera>().enabled = true;
                }
            }
            ran = true;
            cutscenePlayer.Stop();
            ViewModel.Instance.ambientMusic.Play();
        }
    }

}
