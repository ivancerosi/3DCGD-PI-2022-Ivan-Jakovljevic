using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaRespawn : MonoBehaviour
{
    Transform spawn;
    // Start is called before the first frame update
    void Start()
    {
        spawn = transform.Find("Spawn");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name=="Player")
        {
            collision.gameObject.transform.position = spawn.position;
        }
    }
}
