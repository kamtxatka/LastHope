﻿using Assets.Scripts.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCoreController : MonoBehaviour {
    public PlayerStanceType stance;

	void Start ()
    {

    }

    void Update()
    {
        transform.Rotate(new Vector3(0, 0, 0) * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            switch (stance)
            {
                case PlayerStanceType.STANCE_BLUE:
                    player.EnableBlueAbility();
                    player.ChangeStance(stance);
                    player.DialogBlueEnergyCore();
                    Debug.Log("Blue special ability learned");
                    break;
                case PlayerStanceType.STANCE_RED:
                    player.EnableRedAbility();
                    //player.ChangeStance(stance);
                    player.DialogSwitchStances();
                    Debug.Log("Red special ability learned");
                    break;
            }
            AudioSources.instance.PlaySound((int)AudiosSoundFX.Environment_Unclassified_Core);
            gameObject.SetActive(false);
        }
    }
}
