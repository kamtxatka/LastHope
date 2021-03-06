﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ArtilleryController : MonoBehaviour
{

    public float maxHp = 100;
    public float currentHp;
    [HideInInspector]
    public bool alive = true;
    public ParticleSystem leftBarrelParticles;
    public ParticleSystem rightBarrelParticles;
    public GameObject deadExplosion;
    public GameObject deadDecal;
    public Slider hpSlider;

    public void InitData()
    {
        currentHp = maxHp;
        hpSlider.maxValue = maxHp;
        UpdateHpBar();
        alive = true;
    }

    void Start()
    {
        InitData();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("EnemyAttack") && alive)
        {
            EnemyTrash trashScript = other.gameObject.GetComponentInParent<EnemyTrash>();
            Attack currentAttackReceived = trashScript.GetAttack();
            if (currentAttackReceived != null)
            {
                TakeDamage(currentAttackReceived.damage);
            }
        }
    }
    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        UpdateHpBar();
        if (currentHp <= 0)
        {
            SpawnExplosion();
            SpawnDecal();
            alive = false;
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    //void Die()
    //{
    //    Debug.Log("You lose");
    //    SpawnExplosion();
    //    SpawnDecal();
    //    alive = false;
    //    Destroy(gameObject);
    //}

    void SpawnExplosion()
    {
        Instantiate(deadExplosion, transform.position, transform.rotation);
    }

    void SpawnDecal()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit);
        Quaternion hitRotation = Quaternion.Euler(90, Random.Range(0, 360), 0);
        GameObject spawnedDecal = Instantiate(deadDecal, hit.point + new Vector3(0, 0.001f, 0), hitRotation);
        spawnedDecal.transform.localScale *= 5;
    }

    void LeftBarrelShoot()
    {
        if (leftBarrelParticles != null)
        {
            leftBarrelParticles.Play();
        }
        //AudioSources.instance.PlaySound((int)AudiosSoundFX.Environment_Artillery_Shot);
        AudioSources.instance.Play3DSound((int)AudiosSoundFX.Environment_Artillery_Shot, transform.position, 0.9f, AudioRolloffMode.Linear, 0.35f);

    }
    void RightBarrelShoot()
    {
        if (rightBarrelParticles != null)
        {
            rightBarrelParticles.Play();
        }
        //AudioSources.instance.PlaySound((int)AudiosSoundFX.Environment_Artillery_Shot);
        AudioSources.instance.Play3DSound((int)AudiosSoundFX.Environment_Artillery_Shot, transform.position, 0.9f, AudioRolloffMode.Linear, 0.35f);

    }

    void MovementSound()
    {
        //AudioSources.instance.PlaySound((int)AudiosSoundFX.Environment_Artillery_Movement);
        AudioSources.instance.Play3DSound((int)AudiosSoundFX.Environment_Artillery_Movement, transform.position, 1f, AudioRolloffMode.Linear, 0.3f);
    }

    void UpdateHpBar()
    {
        hpSlider.value = currentHp;
    }



}
