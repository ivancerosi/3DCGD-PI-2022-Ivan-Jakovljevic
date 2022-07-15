using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpController : MonoBehaviour
{
    public float speed = 10f;
    public float GRAVITY = 10f;

    Transform shoulders;
    Rigidbody rigidbody;
    Animator playerAnimator;

    public float sensitivity = 10f;


    public void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        playerAnimator = GetComponentInChildren<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        shoulders = transform.Find("RotationPivot").transform;
        rigidbody = GetComponent<Rigidbody>();
    }


    void applyGravity()
    {
        rigidbody.velocity -= new Vector3(0, GRAVITY, 0) * Time.deltaTime;
    }


    private bool canJump = true;

    // Update is called once per frame
    void Update()
    {

        float xAngle = (shoulders.transform.eulerAngles.x - Input.GetAxis("Mouse Y") * sensitivity);
        if (xAngle > 50 && xAngle < 310 && (Input.GetAxis("Mouse Y") > 0))
        {
            xAngle = 310;
        }
        else if (xAngle >= 50 && xAngle < 310 && (Input.GetAxis("Mouse Y") < 0))
        {
            xAngle = 50;
        }


        float yAngle = transform.eulerAngles.y + Input.GetAxis("Mouse X") * sensitivity;

        transform.eulerAngles = new Vector3(0, yAngle, 0);
        shoulders.transform.eulerAngles = new Vector3(xAngle, yAngle, 0);



        float moveX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        float currentSpeed = Mathf.Sqrt(moveX * moveX + moveZ * moveZ);
        float normalizer = currentSpeed / speed;
        if (normalizer == 0)
        {
            moveX = 0;
            moveZ = 0;
        }
        else
        {
            moveX /= normalizer;
            moveZ /= normalizer;
        }

        Vector3 myForward = shoulders.transform.forward;
        myForward.y = 0;
        Vector3 forwardMovementVector = myForward.normalized * moveZ;

        Vector3 myRight = shoulders.transform.right;
        myRight.y = 0;
        Vector3 lateralMovementVector = myRight.normalized * moveX;

        Vector3 movementVector = forwardMovementVector + lateralMovementVector;
        movementVector.y = rigidbody.velocity.y;

        if (canJump)
        {
            rigidbody.velocity = movementVector;
        }

        if (movementVector.magnitude > 0.1)
        {
            playerAnimator.SetBool("isMoving", true);
        }
        else
        {
            playerAnimator.SetBool("isMoving", false);
        }

        if (Input.GetButtonDown("Jump") && canJump)
        {
            rigidbody.velocity += new Vector3(0, 6, 0);
            canJump = false;
        }
        applyGravity();

    }

    private void FixedUpdate()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 6)
        {
            canJump = true;
        }
    }


}
