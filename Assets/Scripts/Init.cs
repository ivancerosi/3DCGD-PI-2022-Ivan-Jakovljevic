using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour
{
    public AudioSource clip;
    private void Awake()
    {
        ViewModel.Instance.ambientMusic.Stop();
        clip.volume = ViewModel.Instance.ambientVolume;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
