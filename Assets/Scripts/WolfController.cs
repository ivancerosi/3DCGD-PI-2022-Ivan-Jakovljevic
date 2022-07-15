using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WolfController : MonoBehaviour, IHitable
{
    enum State
    {
        IDLE, ATTACK, DEAD, LEAP
    }

    public void Hit(int damage)
    {
        health -= damage;
        if (health<=0)
        {
            state = State.DEAD;
        } else
        {
            state = State.ATTACK;
        }
    }

    State state = State.IDLE;

    public int health = 100;
    public float attackRange = 3;
    public float detectRange = 20;
    public float leapRange = 6;
    public int leapForce = 10;

    Animator animator;
    Rigidbody rigidbody;
    NavMeshAgent nav;
    Transform playerTransform;
    Rigidbody playerRigidbody;
    BoxCollider collider;

    SoundManager soundManager;

    Vector3 startPos;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        startPos = gameObject.transform.position;
        playerTransform = GameObject.Find("Player").transform;
        nav = GetComponent<NavMeshAgent>();
        playerRigidbody = playerTransform.GetComponent<Rigidbody>();
        collider = GetComponent<BoxCollider>();
        soundManager = GetComponent<SoundManager>();
    }


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    void doIdle()
    {
        // prevent sliding
        transform.position = startPos;

        Vector3 playerDirection = playerTransform.position - transform.position;
        float distance = playerDirection.magnitude;
        if (distance<=detectRange)
        {
            RaycastHit hitinfo;
            if (Physics.Raycast(transform.position, playerDirection, out hitinfo, detectRange)) {
                if (hitinfo.transform.name=="Player")
                {
                    state = State.ATTACK;
                }
            }
        }
    }

    const float LEAP_WAIT = 5;
    float leapCD = LEAP_WAIT;
    void doAttack()
    {
        nav.destination = playerTransform.position;
        animator.SetBool("attacking",true);
        if (Utils.canAttack(transform.position, playerTransform.position, attackRange, "Player")) { 
           animator.SetBool("inRange", true);
        } else
        {
            animator.SetBool("inRange", false);
        }
        if (leapCD<=0)
        {
            leapCD = LEAP_WAIT;
            state = State.LEAP;
        } else
        {
            leapCD -= Time.deltaTime;
        }
    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(10);
        Destroy(gameObject);    
    }

    bool deadSet = false;
    const int DEAD_SOUND = 3;
    void doDead()
    {
        soundManager.StopPlayingExcept(DEAD_SOUND);
        if (!deadSet)
        {
            nav.enabled = false;
            rigidbody.useGravity = false;
            collider.enabled = false;
            animator.SetBool("dead", true);
            StartCoroutine(Despawn());
            deadSet = true;
        }
    }

    // rotate the wolf towards target and apply leap force
    void Leap()
    {
        Vector3 targetPosition = playerTransform.position + new Vector3(playerRigidbody.velocity.x,0,playerRigidbody.velocity.z);
        transform.LookAt(new Vector2(targetPosition.x,targetPosition.z));
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
        //rigidbody.AddForce((targetPosition-transform.position)*rigidbody.mass*70+new Vector3(0,rigidbody.mass*250,0));
        //rigidbody.AddForce(transform.forward * rigidbody.mass * 70 + Vector3.up*rigidbody.mass*250);
        rigidbody.AddForce(transform.forward * rigidbody.mass * 70 * (playerTransform.position + playerRigidbody.velocity - transform.position).magnitude + new Vector3(0, rigidbody.mass * 250, 0));

        isLeaping = true;
    }

    const float SIT_WAIT=1;
    float waitSit = SIT_WAIT;
    bool isLeaping = false;
    bool tookOffGround = false;
    float startHeight;

    Vector3 previousPlayerPos;

    void doLeap()
    {
        if (isLeaping)
        {
            if (Utils.canAttack(transform.position, playerTransform.position, 2, "Player")) animator.SetBool("inRange", true);
            if (transform.position.y >= startHeight + 0.5) tookOffGround = true;
            RaycastHit hitinfo;
            if (Physics.Raycast(transform.position,Vector3.up*-1f,out hitinfo,0.3f))
            {
                if (tookOffGround)
                {
                    tookOffGround = false;
                    isLeaping = false;
                    state = State.ATTACK;
                    nav.enabled = true;
                }
            }
            return;
        }

        animator.SetBool("leaping",true);
        nav.enabled = false;

        // if leap charge is ready
        if (waitSit<=0)
        {
            RaycastHit hitinfo;
            // check if player is targeted
            if (Physics.Raycast(transform.position,(playerTransform.position-transform.position),out hitinfo, leapRange))
            {
                if (hitinfo.transform.name == "Player")
                {
                    waitSit = SIT_WAIT;
                    startHeight = transform.position.y;
                    Leap();
                    return;
                }
            } // if unable to target the player then exit the leap sequence

            // reset sit time for future leap
            waitSit = SIT_WAIT;
            // reenable nav to follow the player
            nav.enabled = true;
            // update animations
            animator.SetBool("leaping", false);
            // switch to attack behavior
            state = State.ATTACK;
        } else // if leap is still charging then decrement the sit timer and rotate the wolf towards target
        {
            waitSit -= Time.deltaTime;
            transform.LookAt(new Vector2((playerTransform.position + playerRigidbody.velocity).x, (playerTransform.position + playerRigidbody.velocity).z));
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }

    private void FixedUpdate()
    {
        if (state == State.IDLE) doIdle();
        else if (state == State.ATTACK) doAttack();
        else if (state == State.DEAD) doDead();
        else if (state == State.LEAP) doLeap();
    }
}
