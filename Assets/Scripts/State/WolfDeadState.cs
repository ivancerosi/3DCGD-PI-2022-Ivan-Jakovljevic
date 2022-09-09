using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfDeadState : StateMachineBehaviour
{
    private float despawnTime=-1f;
    private WolfController controller;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Entered wolf despawn");
        controller = animator.gameObject.GetComponent<WolfController>();
        if (despawnTime==-1f) despawnTime = controller.despawnTime;
        controller.getNavMeshAgent().enabled = false;
        controller.getRigidbody().freezeRotation = true;
        controller.GetComponent<BoxCollider>().enabled = false;
        controller.getRigidbody().useGravity = false;
        despawnTime -= Time.deltaTime;
        Debug.Log($"Despawn time:{despawnTime} for object:{animator.gameObject.GetHashCode()}");
        if (despawnTime<=0f)
        {
            Destroy(animator.gameObject);
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
