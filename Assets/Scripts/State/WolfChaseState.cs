using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WolfChaseState : StateMachineBehaviour
{
    private int attackRange=-1;
    private float attackCd = -1;
    private float remainingTime = 0;

    private float minLeapDistance;
    private float maxLeapDistance;


    private WolfController controller=null;
    private Transform player=null;
    private NavMeshAgent nav=null;
    private Transform transform=null;
    private Rigidbody playerRb = null;

    private float timeSinceCheck;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (controller == null) controller = animator.GetComponent<WolfController>();
        if (player == null) player = controller.getPlayerTransform();
        if (nav == null) nav = controller.getNavMeshAgent();
        if (transform == null) transform =controller.transform;
        if (attackRange == -1) attackRange =controller.attackRange;

        if (attackCd == -1) attackCd = controller.attackCd;

        if (playerRb == null) playerRb = controller.getPlayerRigidbody();

        minLeapDistance = controller.minLeapDistance;
        maxLeapDistance = controller.maxLeapDistance;
        nav.enabled = true;

        timeSinceCheck = Time.time;
    }

    // onStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller = animator.GetComponent<WolfController>();
        player = controller.getPlayerTransform();
        nav = controller.getNavMeshAgent();
        transform = controller.transform;
        attackRange = controller.attackRange;

        attackCd = controller.attackCd;

        playerRb = controller.getPlayerRigidbody();

        minLeapDistance = controller.minLeapDistance;
        maxLeapDistance = controller.maxLeapDistance;
        nav.enabled = true;

        timeSinceCheck = Time.time;

        controller.runBehaviorTree();
        if (animator.GetBool("leaping")) base.OnStateUpdate(animator, stateInfo, layerIndex);

        remainingTime -= Time.deltaTime;
        if (remainingTime<=0 && (Utils.canAttack(transform.position, player.position, attackRange, "Player")))
        {
            animator.SetTrigger("attack");
            remainingTime = attackCd;
        }
        if ((playerRb.velocity+player.position-transform.position).magnitude<=attackRange)
        {
            animator.SetBool("run", false);
        }
        else
        {
            animator.SetBool("run", true);
        }
     
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
