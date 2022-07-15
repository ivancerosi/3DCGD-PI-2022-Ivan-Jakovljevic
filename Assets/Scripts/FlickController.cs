using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickController : MonoBehaviour
{
    public int milisecondsOn;
    public int milisecondsOff;
    Light spotlight;
    Light spotlightInverse;
    GameObject emission;
    private void Awake()
    {
        spotlight = transform.Find("Spot Light").GetComponent<Light>();
        spotlightInverse = transform.Find("Spot Light Inverse").GetComponent<Light>();
        emission = transform.Find("Emission").gameObject;
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Flick());
    }
    
    IEnumerator Flick()
    {
        while (true)
        {
            yield return new WaitForSeconds((float)milisecondsOn / 1000);
            emission.active = !emission.active;
            spotlight.enabled = !spotlight.enabled;
            spotlightInverse.enabled = !spotlightInverse.enabled;
            yield return new WaitForSeconds((float)milisecondsOff / 1000);
            emission.active = !emission.active;
            spotlight.enabled = !spotlight.enabled;
            spotlightInverse.enabled = !spotlightInverse.enabled;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
