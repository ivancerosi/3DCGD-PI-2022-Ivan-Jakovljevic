using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GhoulController : MonoBehaviour, IHitable
{
    public Rigidbody getRigidBody() { return rb; }
    Rigidbody rb;

    public NavMeshAgent getNav() { return nav; }
    NavMeshAgent nav;

    public Transform getPlayer() { return this.player; }
    Transform player;
    public float detectionDistance = 50f;
    public float attackRange = 6f;

    public float detectionAngle = 180;


    public int health = 100;

    public int damage = 10;
    public float attackCd = 1f;

    private bool isAttacking=false;

    public float despawnTime = 10f;

    public AudioSource walk;
    public AudioSource hit;
    public AudioSource attack;
    public AudioSource die;

    public void PlayStep()
    {
        // check viewmodel paused
        if (!ViewModel.Instance.paused)
        {
            walk.volume = ViewModel.Instance.sfxVolume;
            walk.Play();
        }
    }

    public void PlayHit()
    {
        if (!ViewModel.Instance.paused)
        {
            attack.volume = ViewModel.Instance.sfxVolume;
            attack.Play();
        }
    }

    public void PlayAttack()
    {
        if (!ViewModel.Instance.paused)
        {
            attack.volume = ViewModel.Instance.sfxVolume;
            attack.Play();
        }
    }

    public void PlayDie()
    {
        if (!ViewModel.Instance.paused)
        {
            die.volume = ViewModel.Instance.sfxVolume;
            die.Play();
        }
    }



    private Animator animator;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        animator.SetInteger("health", health);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isHit", false);

        player = GameObject.Find("Player").transform;
        nav = GetComponent<NavMeshAgent>();
    }

    public void Hit()
    {
        player.GetComponent<IHitable>().Hit(damage);
        PlayAttack();
    }

    public void unHit()
    {
        Debug.Log("unhit called");
        animator.SetBool("isHit",false);
    }

    public void Hit(int damage)
    {
        Debug.Log(Time.time + ": "+health);
        health -= damage;
        if (health <= 0f)
        {
            animator.Play("Death");
            PlayDie();
        }
        else
        {
            animator.SetInteger("health", health);
            animator.SetBool("isHit", true);
            PlayHit();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Despawn()
    {
        GameObject.Destroy(this.gameObject);
    }
}
