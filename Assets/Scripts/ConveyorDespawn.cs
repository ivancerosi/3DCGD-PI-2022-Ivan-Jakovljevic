using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorDespawn : MonoBehaviour
{
    Transform spawn;
    private void Awake()
    {
        spawn = transform.parent.Find("Spawn");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent==transform.parent)
        {
            other.transform.position = new Vector3(spawn.position.x, other.transform.position.y ,spawn.position.z);
        }
    }
}
