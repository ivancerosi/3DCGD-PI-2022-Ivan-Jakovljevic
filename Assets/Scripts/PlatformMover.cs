using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    public float zMin;
    public float zMax;
    public float speed=1;
    private float direction = 1;

    private Rigidbody rigidbody;

    public Vector3 velocity;


    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.z >= zMax) direction = -1;
        if (transform.position.z <= zMin) direction = 1;
        velocity = new Vector3(0,0,speed) * direction;
        transform.position += velocity * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        
    }

    private void OnCollisionStay(Collision collision)
    {
        ITransferMovement movable = collision.gameObject.GetComponent<ITransferMovement>();
        if (movable != null)
        {
            movable.contributeToVelocity(velocity);
        }

    }
}
