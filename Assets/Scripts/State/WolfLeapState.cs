using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WolfLeapState : StateMachineBehaviour
{
    WolfController controller=null;
    NavMeshAgent nav=null;
    Transform player = null;
    Rigidbody playerRb = null;
    Animator animator;

    private float prepareTime;
    private bool leaped = false;

    private float FLIGHT_TIME = 1.14f;
    private float leapTime;

    private Vector3 target;
    private Vector3 initial;

    private bool finished = false;

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (controller == null) controller = animator.GetComponent<WolfController>();
        if (nav == null) nav = controller.getNavMeshAgent();
        if (player == null) player = controller.getPlayerTransform();
        if (playerRb == null) playerRb = controller.getPlayerRigidbody();
        controller.getNavMeshAgent().enabled = false;
        if (stateInfo.IsName("Leap"))
        {
            Debug.Log("Prepare time is " + prepareTime);
        } else
        {
            prepareTime = controller.leapPrepareDuration;
            //leaped = false;
            finished = false;
        }
        this.animator = animator;
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller = animator.GetComponent<WolfController>();
        nav = controller.getNavMeshAgent();
        player = controller.getPlayerTransform();
        playerRb = controller.getPlayerRigidbody();
        controller.getNavMeshAgent().enabled = false;

        if (!controller.hasLeaped) {
            controller.transform.LookAt(playerRb.position + playerRb.velocity * 0.5f);
            prepareTime -= Time.deltaTime;
        }

        //if (false && !leaped && prepareTime<=0f)
        if (controller.hasLeaped)
        {
            leapTime = Time.time;
            Rigidbody rb = controller.getRigidbody();
            rb.AddForce(new Vector3(0, 1, 0) * 7*rb.mass, ForceMode.Impulse);
            //controller.getRigidbody().velocity += new Vector3(0,7,0);
            var velocity = playerRb.velocity * 0.5f;
            var playerposi = playerRb.position;
            var wolfposi = animator.transform.position;
            target = playerRb.velocity * 0.5f + playerRb.position-animator.transform.position;
            target = target - target.normalized*2;
            leaped = true;
            initial = animator.transform.position;
            controller.hasLeaped = false;
        }
        if (leaped)
        {
            if (Time.time-leapTime>=FLIGHT_TIME)
            {
                animator.SetBool("leaping", false);
                finished = true;
            }
            else
            {
                var timeInFlight = Time.time-leapTime;
                //Vector3 delta = target*(Time.deltaTime / FLIGHT_TIME);
                animator.transform.position = initial + target * (Mathf.Min(timeInFlight / FLIGHT_TIME, 1));
            }
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

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    //override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}
}
