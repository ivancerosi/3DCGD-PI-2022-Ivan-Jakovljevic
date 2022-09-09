using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GhoulAttackScript : StateMachineBehaviour
{
    private Rigidbody playerRb=null;
    private Transform player=null;
    private Transform transform = null;
    private NavMeshAgent nav=null;

    private float walkSpeed=-1;
    private float runSpeed=-1;

    private float timeSinceAttack=0f;
    private float attackCd = -1f;

    int damage = -1;
    float attackRange = -1f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (nav == null) nav = animator.GetComponent<GhoulController>().getNav();
        if (player==null) player = animator.GetComponent<GhoulController>().getPlayer();
        if (transform==null) transform = animator.transform;
        if (attackRange == -1f) attackRange = animator.GetComponent<GhoulController>().attackRange;
        if (playerRb == null) playerRb = player.gameObject.GetComponent<Rigidbody>();
        if (damage == -1) damage =animator.GetComponent<GhoulController>().damage;
        if (attackCd==-1) attackCd = animator.GetComponent<GhoulController>().attackCd;

        if (runSpeed==-1f) runSpeed = nav.speed;
        if (walkSpeed==-1f) walkSpeed = runSpeed * 0.5f;
    }


    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if ((transform.position-player.position).magnitude<=attackRange*0.8f)
        {
            //Debug.Log(Time.time + ": in range "+(animator.GetBool("isHit")&&animator.GetBool("inRange")));
            animator.SetBool("inRange", true);
            nav.speed = walkSpeed;
        }
        else
        {
            animator.SetBool("inRange", false);
            nav.speed = runSpeed;
        }
        
        if (((transform.position-player.position).magnitude<=attackRange) && timeSinceAttack<=0f)
        {
            //Debug.Log(Time.time + ": attack");
            animator.SetTrigger("attack");
            timeSinceAttack = attackCd;
        } else
        {
            timeSinceAttack -= Time.deltaTime;
        }
        nav.SetDestination(player.position);

        if ((playerRb.position+playerRb.velocity-transform.position).magnitude<=attackRange)
        {
            animator.SetBool("haveToRun", false);
        } else
        {
            animator.SetBool("haveToRun", true);
        }
    }

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}


    //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    //{  
    //}

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    //override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}
}
