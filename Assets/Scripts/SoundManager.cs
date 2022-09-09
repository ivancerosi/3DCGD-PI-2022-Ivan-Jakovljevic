using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource sound1;
    public AudioSource sound2;
    public AudioSource sound3;
    public AudioSource sound4;
    public AudioSource sound5;
    public AudioSource sound6;
    public AudioSource sound7;
    Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 0 doesn't exist so everything will stop playing when ESC is pressed
        if (Input.GetKeyDown(KeyCode.Escape)) StopPlayingExcept(0);
    }

    public void PlayBite()
    {
        PlaySound(2);
    }
    void PlaySound(int sounditem)
    {
        if (ViewModel.Instance.paused) return;
        //Debug.Log(Time.time + ": playing sound " + sounditem);
        if (sounditem == 1)
        {
            sound1.volume = ViewModel.Instance.sfxVolume;
            sound1.Play();
        }
        if (sounditem == 2)
        {
            sound2.volume = ViewModel.Instance.sfxVolume;
            sound2.Play();
        }
        if (sounditem == 3)
        {
            sound3.volume = ViewModel.Instance.sfxVolume;
            sound3.Play();
        }
        if (sounditem == 4)
        {
            sound4.volume = ViewModel.Instance.sfxVolume;
            sound4.Play();
        }
        if (sounditem == 5)
        {
            sound5.volume = ViewModel.Instance.sfxVolume;
            sound5.Play();
        }
        if (sounditem == 6)
        {
            sound6.volume = ViewModel.Instance.sfxVolume;
            sound6.Play();
        }
        if (sounditem == 7)
        {
            sound7.volume = ViewModel.Instance.sfxVolume;
            sound7.Play();
        }
    }

    public void StopPlayingExcept(int sounditem)
    {
        if (sounditem!=1 && sound1 != null) sound1.Stop();
        if (sounditem != 2 && sound2 != null) sound2.Stop();
        if (sounditem != 3 && sound3 != null) sound3.Stop();
        if (sounditem != 4 && sound4 != null) sound4.Stop();
        if (sounditem != 5 && sound5 != null) sound5.Stop();
        if (sounditem != 6 && sound6 != null) sound6.Stop();
        if (sounditem != 7 && sound7 != null) sound7.Stop();
    }


}
