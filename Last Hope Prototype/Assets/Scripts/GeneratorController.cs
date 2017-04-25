﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorController : MonoBehaviour {

    public GameObject energyCore;
    public Vector3 spawnPointPos;
    public Quaternion spawnPointQuat;

    private bool canSpawn = false;
    private bool spawned = false;

    void Start () {
		
	}
	
	void Update () {
        if (canSpawn)
        {
            if (InputManager.Interact())
            {
                spawned = true;
                Debug.Log("Generator charging...");
                Debug.Log("Wait for 5 seconds");
                canSpawn = false;
                Invoke("SpawnSpecialAbility", 5);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (spawned == false)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                canSpawn = true;
                Debug.Log("Press Interact to charge the generator");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (spawned == false)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                canSpawn = false;
            }
        }
    }

    void SpawnSpecialAbility()
    {
        GameObject core = Instantiate(energyCore, spawnPointPos, spawnPointQuat);
        EnergyCoreController coreParameters = core.GetComponent<EnergyCoreController>();
        coreParameters.color = PlayerStance.STANCE_RED;
    }
}