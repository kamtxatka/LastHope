﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashEnemyMoveForwardState : StateMachineBehaviour {

    EnemyTrash enemyTrash;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enemyTrash == null)
        {
            enemyTrash = animator.transform.gameObject.GetComponent<EnemyTrash>();
        }
        enemyTrash.nav.Resume();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int probability = UnityEngine.Random.Range(0, 100);
        if (enemyTrash.attackProbability >= probability /*&& enemyTrash.nav.remainingDistance <= enemyTrash.attackRange*/)
        {
            enemyTrash.nav.Stop();
            animator.SetTrigger("attack");
            //return enemyTrashTypes.ATTACK_STATE;
        }
        else
        {
            enemyTrash.nav.SetDestination(enemyTrash.target.position);
        }

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
