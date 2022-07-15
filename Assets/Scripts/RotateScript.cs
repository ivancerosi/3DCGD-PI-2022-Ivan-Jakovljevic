using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateScript : MonoBehaviour
{
    public int rotateCycleMilisecs;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float rotationDegs = 360f / (rotateCycleMilisecs/1000);
        transform.Rotate(new Vector3(0,0,rotationDegs * Time.deltaTime),Space.World);
    }
}
