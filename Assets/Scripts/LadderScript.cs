using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerMovementScript player = collision.gameObject.GetComponent<PlayerMovementScript>();
        if (player != null)
        {
            player.isOnLadders = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        PlayerMovementScript player = collision.gameObject.GetComponent<PlayerMovementScript>();
        if (player!=null)
        {
            player.isOnLadders = false;
        }
    }
}
