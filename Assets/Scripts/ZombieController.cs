using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class ZombieController : MonoBehaviour, IHitable
{
    struct BTreeConditionsStruct {
        public bool hasBeenHit;
        public bool hasSeenPlayer;
        public bool hasHeardSomething;
        public Vector3 navDestination;
        public Vector3 lastCornerLocation;
        public float goToActionDuration;
        public float waitActionDuration;
        public float waitActionMaxDuration;
        public int rotationStage;

        public int patrolCheckpoint;
        public int numberOfCheckpoints;

        public Queue<Vector3> soundLocations;

    }

    BTreeConditionsStruct BTreeConditions = new BTreeConditionsStruct {
        hasBeenHit = false,
        hasSeenPlayer = false,
        hasHeardSomething = false,
        navDestination = new Vector3(0, 0, 0),
        goToActionDuration = 0f,
        waitActionDuration = 0f,
        waitActionMaxDuration = 3f,
        rotationStage = 0,
        patrolCheckpoint=0,
        numberOfCheckpoints=0,
        soundLocations = new Queue<Vector3>()
    };


    NavMeshAgent nav;
    Transform player;
    Animator animator;
    List<Vector3> patrolPoints;
    CapsuleCollider collider;
    IHitable hitablePlayer;
    float timeToAttack;


    public int health = 100;
    public int damage = 20;
    public float attackDelay = 200;


    public float detectionFovAngle = 70f;
    public float detectionDistance = 20f;

    Node BTreeRootx = new Node(delegate() {
        return Node.STATE.RUNNING;
    });

    Selector PatrolBTreeRoot;

    public ZombieController()
    {
        PatrolBTreeRoot = new Selector();
        /*
         * Hit by player sequence
         */
        Sequence hitSequence = new Sequence();

        Node isHit = new Node(delegate() {
            if (BTreeConditions.hasBeenHit==true)
            {
                BTreeConditions.hasBeenHit = false;
                return Node.STATE.SUCCESS;
            }
            return Node.STATE.FAILURE;
        });

        Node switchToAttack = new Node(delegate () {
            if (state == State.DEAD) return Node.STATE.FAILURE;
            else
            {
                state = State.ATTACK;
                return Node.STATE.SUCCESS;
            }
        });

        hitSequence.nodes.AddLast(isHit);
        hitSequence.nodes.AddLast(switchToAttack);

        PatrolBTreeRoot.nodes.AddLast(hitSequence);


        /*
         * Player spotted sequence
         */
        Sequence spottedSequence = new Sequence();

        Node hasSeenPlayer = new Node(delegate() {
            if (BTreeConditions.hasSeenPlayer)
            {
                BTreeConditions.hasSeenPlayer = false;
                return Node.STATE.SUCCESS;
            }
            else return Node.STATE.FAILURE;
        });

        spottedSequence.nodes.AddLast(hasSeenPlayer);
        spottedSequence.nodes.AddLast(switchToAttack);

        PatrolBTreeRoot.nodes.AddLast(spottedSequence);


        /*
         * Idle node
         */

        Node idle = new Node(delegate () {
            if (BTreeConditions.numberOfCheckpoints<2 && (BTreeConditions.navDestination - transform.position).magnitude <= 1)
            {
                state = State.SLEEP;
                return Node.STATE.SUCCESS;
            }
            return Node.STATE.FAILURE;
        });

        PatrolBTreeRoot.nodes.AddLast(idle);

        /*
         * Investigate sound sequence
         */
        Sequence investigateSoundSequence = new Sequence();

        Node hasHeardSomething = new Node(delegate () {
            if (BTreeConditions.hasHeardSomething)
            {
                return Node.STATE.SUCCESS;
            } else
            {
                return Node.STATE.FAILURE;
            }
        });
            /*
             * Approach location of the sound sequence
             */
        Selector approachLocation = new Selector();
                /*
                 * Go to location of the sound if the path is valid
                 */
        Sequence goToValidSoundDestSequence = new Sequence();
        approachLocation.nodes.AddLast(goToValidSoundDestSequence);

        Node checkPathValid = new Node(delegate() {
            if ((BTreeConditions.navDestination - transform.position).magnitude > 10) return Node.STATE.SUCCESS;

            NavMeshPath path = new NavMeshPath();
            nav.CalculatePath(BTreeConditions.navDestination, path);
            if (path.status == NavMeshPathStatus.PathComplete) return Node.STATE.SUCCESS;
            else return Node.STATE.FAILURE;
        });

        Node goToLocation = new Node(delegate() {
            nav.destination = BTreeConditions.navDestination;
            if ((transform.position-BTreeConditions.navDestination).magnitude <= 1f)
            {
                BTreeConditions.goToActionDuration = 0f;
                nav.destination = transform.position;
                return Node.STATE.SUCCESS;
            }
            if (BTreeConditions.goToActionDuration>30f)
            {
                BTreeConditions.goToActionDuration = 0f;
                BTreeConditions.hasHeardSomething = false;
                BTreeConditions.goToActionDuration = 0f;
                return Node.STATE.FAILURE;
            }
            else {
                state = State.PATROL;
                BTreeConditions.goToActionDuration += Time.deltaTime;
                return Node.STATE.RUNNING;
            }
        });

        goToValidSoundDestSequence.nodes.AddLast(checkPathValid);
        goToValidSoundDestSequence.nodes.AddLast(goToLocation);
            /*
             * Go to aproximate location of the sound if the path is invalid
            */
        Sequence goToAproxSoundDestSequence = new Sequence();
        approachLocation.nodes.AddLast(goToAproxSoundDestSequence);

        Node getLastCorner = new Node(delegate () {
            NavMeshPath navPath = new NavMeshPath();
            nav.CalculatePath(BTreeConditions.navDestination, navPath);
            int corners = navPath.corners.Length;
            if (corners>0)
            {
                BTreeConditions.navDestination = navPath.corners[corners-1];
                return Node.STATE.SUCCESS;
            }
            return Node.STATE.FAILURE;
        });

        goToAproxSoundDestSequence.nodes.AddLast(getLastCorner);
        goToAproxSoundDestSequence.nodes.AddLast(goToLocation);


        Node waitInLocation = new Node(delegate () {
            state = State.SLEEP;
            BTreeConditions.waitActionDuration += Time.deltaTime;

             transform.RotateAround(transform.position, transform.up, 360*(Time.deltaTime/BTreeConditions.waitActionMaxDuration));

            if (BTreeConditions.waitActionDuration >= BTreeConditions.waitActionMaxDuration)
            {
                state = State.PATROL;
                BTreeConditions.waitActionDuration = 0f;
                return Node.STATE.SUCCESS;
            }
            return Node.STATE.RUNNING;
        });


        investigateSoundSequence.nodes.AddLast(hasHeardSomething);
        investigateSoundSequence.nodes.AddLast(approachLocation);
        investigateSoundSequence.nodes.AddLast(waitInLocation);
        PatrolBTreeRoot.nodes.AddLast(investigateSoundSequence);

        /*
         * Regular patrol sequence
         */
        Sequence regularPatrolSequence = new Sequence();
        PatrolBTreeRoot.nodes.AddLast(regularPatrolSequence);
        /*
         * Set patrol waypoint
         */

        Node checkpointSelector = new Node(delegate() {
            if (BTreeConditions.numberOfCheckpoints > 0)
            {
                BTreeConditions.navDestination = patrolPoints[BTreeConditions.patrolCheckpoint];
                return Node.STATE.SUCCESS;
            }
            return Node.STATE.FAILURE;
        });

        Node waitInPatrolCheckpoint = new Node(delegate() {
            Node.STATE outcome = waitInLocation.evaluate();
            if (outcome==Node.STATE.SUCCESS)
            {
                BTreeConditions.patrolCheckpoint = (BTreeConditions.patrolCheckpoint+1) % patrolPoints.Count;
            }
            return outcome;
        });

        regularPatrolSequence.nodes.AddLast(checkpointSelector);
        regularPatrolSequence.nodes.AddLast(approachLocation);
        regularPatrolSequence.nodes.AddLast(waitInPatrolCheckpoint);

    }
   

    enum State
    {
        PATROL,
        SLEEP,
        DEAD,
        ATTACK
    }

    State state = State.SLEEP;

    private float speed;


    public int walkSpeed=1;

    class Comparer : IComparer<Transform> {
        public int Compare(Transform x, Transform y)
        {
            int a = int.Parse(x.name.Substring("PatrolPoint".Length));
            int b = int.Parse(y.name.Substring("PatrolPoint".Length));
            if (a > b) return 1;
            if (b < a) return -1;
            else return 0;
        }
    }

    public void Hit(int damage)
    {
        BTreeConditions.hasBeenHit = true;
        health -= damage;
        if (health<=0)
        {
            state = State.DEAD;
            return;
        } else
        {
            state = State.ATTACK;
        }
        animator.SetFloat("hitBlend",(nav.velocity.magnitude/nav.speed/2));
        animator.SetTrigger("hit");
    }

    private void Awake()
    {
        timeToAttack = attackDelay;
        collider = GetComponent<CapsuleCollider>();
        nav = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player").transform;
        hitablePlayer = player.GetComponent<IHitable>();
        animator = GetComponent<Animator>();
        animator.SetBool("dead", false);
        speed = nav.speed;

        List<Transform> children = new List<Transform>(gameObject.GetComponentsInChildren<Transform>());
        List<Transform> filtered = new List<Transform>();
        patrolPoints = new List<Vector3>();
        foreach (Transform point in children)
        {
            if (point.tag == "PatrolPoint") filtered.Add(point);
        }
        filtered.Sort(new Comparer());
        foreach (Transform point in filtered)
        {
            patrolPoints.Add(point.position);
        }
        BTreeConditions.numberOfCheckpoints = patrolPoints.Count;
    }

    void doSleep()
    {
        Vector3 playerVector = player.position - transform.position;
        playerVector.y = 0;
        float angle = Vector3.Angle(playerVector, transform.forward);
        if (angle <= detectionFovAngle)
        {
            if (Utils.canAttack(transform.position, player.position, detectionDistance, "Player"))
            {
                BTreeConditions.hasSeenPlayer = true;
            }
        }
        nav.speed = walkSpeed;
        PatrolBTreeRoot.evaluate();

        animator.SetBool("playerTargeted", false);
        animator.SetBool("inRange", false);
        animator.SetBool("sleeping", true);
        animator.SetBool("dead", false);
    }


    void doPatrol()
    {
        
        Vector3 playerVector = player.position - transform.position;
        float distance = playerVector.magnitude;
        playerVector.y = 0;
        float angle = Vector3.Angle(playerVector, transform.forward);
        if (angle<=detectionFovAngle)
        {
            if (Utils.canAttack(transform.position, player.position, detectionDistance, "Player"))
            {
                BTreeConditions.hasSeenPlayer = true;
            }
        }
        nav.speed = walkSpeed;
        PatrolBTreeRoot.evaluate();


        animator.SetBool("sleeping", false);
        animator.SetBool("playerTargeted", false);
        animator.SetBool("inRange", false);

    }

    void doAttack()
    {
        nav.speed = speed;
        Vector3 relative = player.position - transform.position;
        nav.SetDestination(player.position);
        float angle = Vector3.SignedAngle(new Vector3(relative.x, 0, relative.z), new Vector3(transform.forward.x, 0, transform.forward.z), new Vector3(0, 1, 0));
        if ((transform.position - player.position).magnitude < 2.7)
        {
            animator.SetBool("playerTargeted", false);
            float maxRotation = Time.deltaTime * nav.angularSpeed;
            angle = Mathf.Min(maxRotation, angle);
            transform.eulerAngles -= new Vector3(0, angle, 0);

        }
        else
        {
            animator.SetBool("playerTargeted", true);
        }
        if ((transform.position - player.position).magnitude < 3)
        {
            animator.SetFloat("attackBlend",Mathf.Min(1,(nav.velocity.magnitude/nav.speed)*3f));
            animator.SetBool("inRange", true);
            if (timeToAttack <=0)
            {
                hitablePlayer.Hit(damage);
                timeToAttack = attackDelay;
            } else
            {
                timeToAttack -= Time.deltaTime*1000;
            }
        }
        else
        {
            animator.SetBool("inRange", false);
            timeToAttack = attackDelay;
        }
    }


    void doDead()
    {
        IEnumerator Vanish()
        {
            yield return new WaitForSeconds(10);
            Destroy(this.gameObject);
        }
        collider.enabled = false;
        animator.SetBool("dead",true);
        nav.speed = 0;
        StartCoroutine(Vanish());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (state==State.PATROL)
        {
            doPatrol();
        }
        else if (state==State.SLEEP)
        {
            doSleep();
        }
        else if (state==State.ATTACK)
        {
            doAttack();
        }
        else if (state==State.DEAD)
        {
            doDead();
        }
        
    }
}
