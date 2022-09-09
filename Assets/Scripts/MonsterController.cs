using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour, IHitable
{
    enum State
    {
        SLEEP, AWAKE, ATTACK, DIE   
    }

    State state = State.SLEEP;

    public int health=300;
    public int detectRange = 20;
    public int meleerange = 2;
    public int damage = 20;
    public float attackcd=1000;
    Animator animator;
    NavMeshAgent nav;
    Rigidbody rb;
    BoxCollider collider;
    private Vector3 jumpVector;
    private Vector3 initialRotation;
    Transform playerTransform;
    IHitable playerHitable;

    public void Hit(int damage)
    {
        health -= damage;
        if (health <= 0) state = State.DIE;
        else if (state == State.SLEEP) state = State.AWAKE;
       // animator.SetBool("takeDamage", true);
    }


    public void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        playerTransform = GameObject.Find("Player").transform;
        playerHitable=playerTransform.GetComponent<IHitable>();
        nav = GetComponent<NavMeshAgent>();
        collider = GetComponent<BoxCollider>();
        animator.SetBool("isDead", false);
        animator.SetBool("inRange", false);
        animator.SetBool("isAttacking", false);
    }

    // Start is called before the first frame update
    void Start()
    {
    }


    void Jump()
    {
        rb.useGravity = true;
        rb.AddForce(jumpVector * 20000);
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void doSleep()
    {
        Vector3 playerpos = playerTransform.position;
        float distance = (playerpos - transform.position).magnitude;
        if (distance <= detectRange) state = State.AWAKE;
    }

    bool groundHit = false;
    float timePassed = 0;
    float WAIT = 0.5f;
    void doAwake()
    {
        if (rb.useGravity == false)
        {
            Jump();
        }
        RaycastHit hitinfo;
        if (groundHit && timePassed>=WAIT)
        {
            nav.enabled = true;
            state = State.ATTACK;
            return;
        }
        if (groundHit)
        {
            timePassed += Time.deltaTime;
        }
        if (Physics.Raycast(transform.position, Vector3.up*-1,out hitinfo,2))
        {
            if (hitinfo.transform.tag=="LevelPart")
            {
                groundHit = true;
            }
        }
        transform.eulerAngles = initialRotation;

    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(10);
        Destroy(gameObject);
    }
    Vector3 initialpos;
    void doDie()
    {
        if (nav.enabled)
        {
            initialpos = transform.position;
            StartCoroutine(Despawn());
            nav.enabled = false;
        }
        rb.useGravity = false;
        rb.freezeRotation = true;
        collider.enabled = false;
        transform.position = initialpos;
        animator.SetBool("isDead", true);
    }

    public float attackTimer;
    void doAttack()
    {
        animator.SetBool("isAttacking", true);
        nav.destination = playerTransform.position;

        float distance = (playerTransform.position - transform.position).magnitude;
        if (distance<=meleerange && attackTimer<=0)
        {
            playerHitable.Hit(damage);
            animator.SetBool("inRange", true);
            attackTimer = attackcd;
        } else
        {
            attackTimer -= Time.deltaTime * 1000;
            animator.SetBool("inRange", false);
        }
    }



    void FixedUpdate()
    {
        if (state==State.SLEEP) doSleep();
        else if (state == State.ATTACK) doAttack();
        else if (state == State.DIE) doDie();
        else if (state == State.AWAKE) doAwake();
    }




    private void OnCollisionEnter(Collision collision)
    {
        jumpVector=collision.contacts[0].normal;
    }
    private void OnCollisionStay(Collision collision)
    {
        if (state==State.SLEEP)
        {
            rb.velocity = new Vector3(0, 0, 0);
        }
    }
}
