using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class ZombieController : MonoBehaviour, IHitable
{
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


    public float detectionFovAngle=70f;
    public float detectionDistance=20f;
   

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
        health -= damage;
        if (health<=0)
        {
            state = State.DEAD;
        } else
        {
            state = State.ATTACK;
        }
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
        
    }
    // Start is called before the first frame update
    void Start()
    {
    }


    float secondsSlept = 0;
    void doSleep()
    {
        Vector3 playerVector = player.position - transform.position;
        playerVector.y = 0;
        float angle = Vector3.Angle(playerVector, transform.forward);
        if (angle <= detectionFovAngle)
        {
            if (Utils.canAttack(transform.position, player.position, detectionDistance, "Player"))
            {
                state = State.ATTACK;
                return;
            }
        }
        animator.SetBool("playerTargeted", false);
        animator.SetBool("inRange", false);
        animator.SetBool("sleeping", true);
        animator.SetBool("dead", false);
        if (secondsSlept >= 3)
        {
            secondsSlept = 0;
            state = State.PATROL;
        }
        else secondsSlept += Time.deltaTime;
    }

    int currentTarget = -1;
    bool hasIdled = false;
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
                state = State.ATTACK;
                return;
            }
        } 

        if (nav.remainingDistance<2)
        {
            if (hasIdled)
            {
                hasIdled = false;
                currentTarget = (currentTarget + 1) % patrolPoints.Count;
                nav.SetDestination(patrolPoints[currentTarget]);
                animator.SetBool("sleeping", false);
                animator.SetBool("playerTargeted", false);
                animator.SetBool("inRange", false);
                nav.speed = walkSpeed;
            } else
            {
                hasIdled = true;
                state = State.SLEEP;    
            }
        }
    }

    void doAttack()
    {
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
