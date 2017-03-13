﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class TrashChaseState : TrashState
{
    public TrashChaseState(GameObject go) : base(go)
    {
    }

    public override void StartState()
    {
        //EnemyTrash trashState = go.GetComponent<EnemyTrash>();
    }

    public override IEnemyState UpdateState()
    {
        EnemyTrash trashState = go.GetComponent<EnemyTrash>();
        if (trashState.target != null)
        {
            trashState.nav.SetDestination(trashState.target.position);
        }
        return null;
    }

    //public override void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
    //    {
    //        EnemyTrash trashState = go.GetComponent<EnemyTrash>();
    //        trashState.currentState.EndState();
    //        trashState.currentState = new TrashIdleState(go);
    //        trashState.currentState.StartState();
    //    }
    //}
}
