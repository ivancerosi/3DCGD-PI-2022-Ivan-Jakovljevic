using System.Collections;       
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WolfController : MonoBehaviour, IHitable
{
    public bool hasLeaped = false;

    public float minLeapDistance = 5f;
    public float maxLeapDistance = 20f;

    public float leapPrepareDuration = 150f;
    public float leapDuration = 1f;


    enum State
    {
        IDLE, ATTACK, DEAD, LEAP
    }

    public void Hit(int damage)
    {
        animator.SetBool("isHit", true);
        health -= damage;
        if (health<=0)
        {
            animator.Play("Dead");
        }
    }

    State state = State.IDLE;

    public float attackCd=2;

    public int health = 100;
    public int attackRange = 3;
    public float detectRange = 20;
    public float leapRange = 6;
    public int leapForce = 10;
    public int damage = 20;

    public float despawnTime = 10f;

    Animator animator;
    Rigidbody rigidbody;
    NavMeshAgent nav;
    Transform playerTransform;
    Rigidbody playerRigidbody;
    BoxCollider collider;

    IHitable playerHitable;

    Selector behaviorTreeRoot;

    SoundManager soundManager;

    Vector3 startPos;

    public void runBehaviorTree()
    {
        behaviorTreeRoot.evaluate();
    }

    public struct BTreeConditionsStruct
    {
        public bool isPreparingLeap;
        public bool isLeaping;
        public float evadeActionDuration;
        public float evadeActionMaxDuration;
    };

    BTreeConditionsStruct BTreeConditions = new BTreeConditionsStruct() {
        isPreparingLeap = false,
        isLeaping = false,
        evadeActionDuration = 2,
        evadeActionMaxDuration = 2,
    };

    public WolfController()
    {
        behaviorTreeRoot = new Selector();

        Sequence evadeSequence = new Sequence();
        behaviorTreeRoot.nodes.AddLast(evadeSequence);

        Node playerAimingAtMe = new Node(delegate() {
            Vector3 playerVector = transform.position - playerTransform.position;
            if (animator.GetBool("leaping")) return Node.STATE.FAILURE;
            if (playerVector.magnitude <= attackRange * 2f)
            {
                return Node.STATE.FAILURE;
            }
            Vector3 aimVector = playerTransform.forward;

            float aimProjection = Vector3.Project(playerVector, aimVector).magnitude;
            aimProjection = playerVector.magnitude - aimProjection;
            if (aimProjection <= 1)
            {
                return Node.STATE.SUCCESS;
            }
            return Node.STATE.FAILURE;
        });
        evadeSequence.nodes.AddLast(playerAimingAtMe);

        Node increaseTransversalSpeed = new Node(delegate() {
            BTreeConditions.evadeActionDuration += Time.deltaTime;
            if (BTreeConditions.evadeActionDuration < BTreeConditions.evadeActionMaxDuration) {
                return Node.STATE.SUCCESS;
            }
            Vector3 playerDirection = playerTransform.position - transform.position;
            playerDirection.y = 0;
            int direction = Random.Range(0, 2);
            Vector3 evadeDirection = playerDirection.normalized * nav.speed * BTreeConditions.evadeActionMaxDuration * 1.05f;
            float rotationAmount = 60 + 240 * direction;
            evadeDirection = Quaternion.Euler(0, rotationAmount, 0) * evadeDirection;
            Vector3 evadePosition = transform.position + evadeDirection;
            
            NavMeshPath path=new NavMeshPath();
            nav.CalculatePath(evadePosition, path);

            BTreeConditions.evadeActionDuration = 0f;


        if (path.corners.Length <= 1 || (transform.position - path.corners[path.corners.Length - 1]).magnitude<nav.speed)
            {
                if (rotationAmount==60) evadeDirection = Quaternion.Euler(240, 0, 0) * evadeDirection;
                if (rotationAmount == 300) evadeDirection = Quaternion.Euler(120, 0, 0) * evadeDirection;
                evadeDirection = transform.position + evadePosition;
                nav.CalculatePath(evadePosition, path);
                if (path.corners.Length <= 1 || (transform.position - path.corners[path.corners.Length - 1]).magnitude < nav.speed)
                {
                    return Node.STATE.FAILURE;
                }
            }
            nav.destination = evadePosition;
            return Node.STATE.SUCCESS;
        });
        evadeSequence.nodes.AddLast(increaseTransversalSpeed);

        Sequence leapSequence = new Sequence();
        behaviorTreeRoot.nodes.AddLast(leapSequence);
        Node checkRangeAndLos = new Node(delegate () {
            if (animator.GetBool("leaping")) return Node.STATE.SUCCESS;
            float distance = (playerTransform.position - transform.position).magnitude;
            if (distance > 100 || distance < 7) return Node.STATE.FAILURE;
            if (!Utils.canAttack(transform.position, playerTransform.position, 100, "Player")) return Node.STATE.FAILURE;
            return Node.STATE.SUCCESS;
        });
        leapSequence.nodes.AddLast(checkRangeAndLos);

        Node doLeap = new Node(delegate() {
            animator.SetBool("leaping",true);
            return Node.STATE.SUCCESS;
        });
        leapSequence.nodes.AddLast(doLeap);        

        Sequence attackSequence = new Sequence();
        behaviorTreeRoot.nodes.AddLast(attackSequence);

        Node canAttack = new Node(delegate () {
            if (Utils.canAttack(transform.position, playerTransform.position, attackRange, "Player")) return Node.STATE.SUCCESS;
            return Node.STATE.FAILURE;
        });
        attackSequence.nodes.AddLast(canAttack);

        Node attack = new Node(delegate () {
            nav.SetDestination(playerTransform.position);
            return Node.STATE.SUCCESS;
        });
        attackSequence.nodes.AddLast(attack);

        Node approach = new Node(delegate () {
            nav.SetDestination(playerTransform.position);
            return Node.STATE.SUCCESS;
        });
        behaviorTreeRoot.nodes.AddLast(approach);
    }

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

        playerHitable = playerTransform.GetComponent<IHitable>();
    }


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Leaped()
    {
        hasLeaped = true;
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
        rigidbody.AddForce(transform.forward * rigidbody.mass * 70 * (playerTransform.position + playerRigidbody.velocity - transform.position).magnitude + new Vector3(0, rigidbody.mass * 250, 0));

        isLeaping = true;
    }

    const float SIT_WAIT=1;
    float waitSit = SIT_WAIT;
    bool isLeaping = false;
    bool tookOffGround = false;
    float startHeight;

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



    public Animator getAnimator() { return this.animator; }
    public Rigidbody getRigidbody() { return this.rigidbody; }

    public Rigidbody getPlayerRigidbody() { return this.playerRigidbody; }
    public Transform getPlayerTransform() { return this.playerTransform; }
    public NavMeshAgent getNavMeshAgent() { return this.nav; }
    public void UnHit()
    {
        animator.SetBool("isHit", false);
    }

    public void Bite()
    {
        soundManager.PlayBite();
        playerHitable.Hit(damage);
    }
}
