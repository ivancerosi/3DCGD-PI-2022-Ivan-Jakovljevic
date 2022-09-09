using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhoulIdleScript : StateMachineBehaviour
{
    private Transform player;
    private Transform transform;
    private float detectionFovAngle;
    private float detectionDistance;
    private GhoulController controller;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller = animator.GetComponent<GhoulController>();
        player = animator.GetComponent<GhoulController>().getPlayer();
        transform = animator.transform;
        detectionFovAngle = animator.GetComponent<GhoulController>().detectionAngle;
        detectionDistance = animator.GetComponent<GhoulController>().detectionDistance;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

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


    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller.unHit();
        //Debug.Log(Time.time + ": "+animator.GetBool("isAttacking"));
        Vector3 playerVector = player.position - transform.position;
        float distance = playerVector.magnitude;
        playerVector.y = 0;
        float angle = Vector3.Angle(playerVector, transform.forward);
        if (angle <= detectionFovAngle)
        {
            //Debug.Log(Time.time + ": " + transform.position + " " + player.transform.position + " " + detectionDistance);
            if (Utils.canAttack(transform.position, player.transform.position, detectionDistance, "Player"))
            {
                animator.SetBool("isAttacking", true);
            }
        }
    }
}
