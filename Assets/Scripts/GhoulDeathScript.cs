using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GhoulDeathScript : StateMachineBehaviour
{
    private NavMeshAgent nav = null;
    private Rigidbody rb = null;

    private Vector3 initPos;
    private Quaternion initRot;

    float timePassed;
    float DESPAWN_TIMER = 5f;

    GhoulController controller;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        nav = animator.GetComponent<GhoulController>().getNav();
        rb = animator.GetComponent<GhoulController>().getRigidBody();
        controller = animator.GetComponent<GhoulController>();

        nav.enabled = false;
        initPos = rb.position;
        initRot = rb.rotation;

        DESPAWN_TIMER = controller.despawnTime;


    }


    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log(Time.time + " d onstateupdate");
        rb.position = initPos;
        rb.rotation = initRot;
        timePassed += Time.deltaTime;
        if (timePassed>=DESPAWN_TIMER)
        {
            controller.Despawn();
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
