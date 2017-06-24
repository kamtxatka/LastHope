﻿using Assets.Scripts.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct CameraShakeStats
{
    public float duration;
    public float magnitude;
    public float xMultiplier;
    public float yMultiplier;
}

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public Animator anim;
    public Collider sword;
    public Collider swordHeavy;
    public MeleeWeaponTrail swordEmitter;
    public Transform swordAoeSpawn;
    public Transform shieldAoeSpawn;
    public Collider shield;
    public Collider shieldHeavy;
    public MeleeWeaponTrail shieldEmitter;
    public GameObject hitParticles;
    public ParticleSystem redAbilityParticles;
    public ParticleSystem dodgeParticles;
    [SerializeField]
    public ParticleSystem greyHeavyAttackParticles;
    [SerializeField]
    public ParticleSystem blueHeavyAttackParticles;
    [SerializeField]
    public ParticleSystem redHeavyAttackParticles;
    [HideInInspector]
    public ParticleSystem currentHeavyAttackParticles;
    [HideInInspector]
    public PlayerStance stance;
    [HideInInspector]
    public PlayerStanceType newStance;
    public bool debugMode = false;
    [SerializeField]
    private Material baseMat;
    [SerializeField]
    private Texture baseGrey;
    [SerializeField]
    private Texture baseBlue;
    [SerializeField]
    private Texture baseRed;
    [SerializeField]
    private Material extraMat;
    [SerializeField]
    private Texture extraGrey;
    [SerializeField]
    private Texture extraBlue;
    [SerializeField]
    private Texture extraRed;
    [SerializeField]
    private Material shieldMat;
    [SerializeField]
    private Texture shieldGrey;
    [SerializeField]
    private Texture shieldBlue;
    [SerializeField]
    private Texture shieldRed;
    [SerializeField]
    private GameObject swordBlueOrb;
    [SerializeField]
    private GameObject swordRedOrb;
    [SerializeField]
    private GameObject swordBlueLine;
    [SerializeField]
    private GameObject swordRedLine;

    private bool redAbilityEnabled = false;
    private bool greyAbilityEnabled = false;

    public Image currentHPBar;
    public Image currentEnergyBar;

    //public PlayerPassiveStats noneStats;
    public PlayerPassiveStatsAbsolute baseStats;
    public PlayerPassiveStatsRelative blueStats;
    public PlayerPassiveStatsRelative redStats;

    public PlayerPassiveStatsAbsolute currentStats;

    public CameraShakeStats defaultCameraShakeStats;

    private Attack currentAttack;
    //private Attack lastAttackReceived;

    // HP
    public int maxHP = 100;
    public int currentHP;
    private int initialMaxHP;
    public float timeBetweenDmg = 0.5f;
    public SpawnManager respawnManager;
    private PlayerAttack attackScript;
    private bool dmged;
    private bool dead;
    private float timer;

    // Energy
    public int maxEnergy = 5;
    public int currentEnergy;
    private int initialMaxEnergy;

    // Movement
    public float turnSpeed = 50;
    //public float speed = 10;
    public Transform camT;
    private CameraShake camShake;
    private ControllerEvents controllerEvents;
    public Vector3 movement;
    public Vector3 targetDirection;
    public float dodgeThrust;
    public bool pendingMove = false;
    public bool canDodge;
    private bool attackMoving = false;
    [SerializeField]
    private float dodgeMaxCooldown;
    [SerializeField]
    private float dodgeCurrentCooldown;
    private float dodgeTimer;
    private float movementHorizontal, movementVertical;
    private Rigidbody rigidBody;
    private Vector3 camForward;
    private Vector3 camRight;
    private Dictionary<string, Attack> playerAttacks;

    //UI
    [SerializeField]
    private UIManager uiManager;

    // Interact
    public bool canInteract = false;

    // Block
    public bool blocking = false;

    // Attack
    private bool inputWindow = false;
    private bool canChangeAttackState = false;

    // Special Attack
    public GameObject neutralSphere;
    public GameObject neutralAttackParticles;
    private GameObject spawnedParticle;
    public GameObject redSpehre;
    public float redSpecialAttackThrust = 30;
    public float specialAttackDamage = 40;
    private bool canSpecialAttack = false;

    private bool isDodge = false;
    public Dictionary<PlayerStanceType, PlayerStance> stances;

    void Start()
    {
        //Hide cursor
        Cursor.visible = false;

        anim = GetComponent<Animator>();

        // Stats setup
        stances = new Dictionary<PlayerStanceType, PlayerStance>();
        stances.Add(PlayerStanceType.STANCE_NONE, new PlayerStance(PlayerStanceType.STANCE_NONE, new PlayerPassiveStatsRelative(1, 1, 1, 1)));
        stances.Add(PlayerStanceType.STANCE_BLUE, new PlayerStance(PlayerStanceType.STANCE_BLUE, new PlayerPassiveStatsRelative(1, 0.66f, 1.33f, 40)));
        stances.Add(PlayerStanceType.STANCE_RED, new PlayerStance(PlayerStanceType.STANCE_RED, new PlayerPassiveStatsRelative(1.5f, 1, 0.85f, 30)));
        ChangeStance(PlayerStanceType.STANCE_NONE);

        playerAttacks = new Dictionary<string, Attack>();

        //Light attacks
        playerAttacks.Add("L1", new Attack("L1", 10));
        playerAttacks.Add("L2", new Attack("L2", 15));
        playerAttacks.Add("L3", new Attack("L3", 20));

        //Heavy attacks
        playerAttacks.Add("H1", new Attack("H1", 25));
        playerAttacks.Add("H2", new Attack("H2", 30));
        playerAttacks.Add("H3", new Attack("H3", 35));

        //Special Attacks
        playerAttacks.Add("Red", new Attack("Red", 30));
        playerAttacks.Add("Blue", new Attack("Blue", 40));

        //Camera Shake Default Stats
        defaultCameraShakeStats.duration = 0.2f;
        defaultCameraShakeStats.magnitude = 0.5f;
        defaultCameraShakeStats.xMultiplier = 1.0f;
        defaultCameraShakeStats.yMultiplier = 1.0f;

        stance = stances[PlayerStanceType.STANCE_NONE];

        initialMaxHP = maxHP;
        currentHP = maxHP;
        initialMaxEnergy = maxEnergy;
        currentEnergy = maxEnergy;

        canDodge = true;

        camT = GameObject.FindGameObjectWithTag("MainCamera").transform;
        camShake = camT.GetComponent<CameraShake>();
        controllerEvents = camT.GetComponent<ControllerEvents>();
        rigidBody = GetComponent<Rigidbody>();

        DisableSwordEmitter();
        DisableShieldEmitter();

        uiManager.UpdateMaxHealth(initialMaxHP);
        uiManager.UpdateHealth(currentHP);
        uiManager.UpdateEnergyCapacity(initialMaxEnergy);
        uiManager.UpdateEnergy(currentEnergy);
        uiManager.UpdatePlayerStance(stance);
    }

    public void CallFX()
    {
        GameObject temp1 = GameObject.Find("FxCaller");
        if (temp1 != null)
        {
            FxCaller_OnKey_Pools temp = temp1.GetComponent<FxCaller_OnKey_Pools>();
            if (temp != null)
            {
                temp.ThrowFX();
            }
        }
    }

    public void HeavyAttackEffect()
    {
        if (camShake != null)
        {
            StartCoroutine(camShake.Shake(0.1f, 0.25f, 1, 1, this.transform));
        }
        controllerEvents.AddRumble(0.4f, new Vector2(0.7f, 0.7f), 0.2f);
        //TODO: Add attack sound fx when we have one
    }

    public void SwordHeavyAttackAoEParticles()
    {
        ParticleSystem ps = Instantiate(currentHeavyAttackParticles, swordAoeSpawn.position, Quaternion.identity);
        float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(ps.gameObject, totalDuration);
        //controllerEvents.AddRumble(0.2f, new Vector2(0.7f, 0.7f), 0.1f);
    }

    public void ShieldHeavyAttackAoEParticles()
    {
        ParticleSystem ps = Instantiate(currentHeavyAttackParticles, shieldAoeSpawn.position, Quaternion.identity);
        float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(ps.gameObject, totalDuration);
        //controllerEvents.AddRumble(0.2f, new Vector2(0.7f, 0.7f), 0.1f);
    }

    public void EnableSwordArea()
    {
        swordHeavy.enabled = true;
    }

    public void DisableSwordArea()
    {
        swordHeavy.enabled = false;
    }

    public void EnableShieldArea()
    {
        shieldHeavy.enabled = true;
    }

    public void DisableShieldArea()
    {
        shieldHeavy.enabled = false;
    }

    void Update()
    {
        if (InputManager.DebugMode())
        {
            debugMode = !debugMode;
        }
        if (debugMode)
        {
            if (Input.GetKeyDown(KeyCode.T))
                LoseHp(10);

            if (Input.GetKeyDown(KeyCode.H))
                Heal(5);

            if (Input.GetKeyDown(KeyCode.C))
                LoseEnergy(1);

            if (Input.GetKeyDown(KeyCode.V))
                GainEnergy(1);
        }
        if (dmged)
        {
            timer += Time.deltaTime;
            if (timer > timeBetweenDmg)
            {
                dmged = false;
            }
        }

        if (!canDodge)
        {
            dodgeCurrentCooldown = dodgeMaxCooldown - (Time.time - dodgeTimer);
            if (Time.time - dodgeTimer >= dodgeMaxCooldown)
            {
                canDodge = true;
            }
        }

        //UpdateHPBar();
        //UpdateEnergyBar();
    }

    void FixedUpdate()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dodge"))
        {
            Dodge();
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("RedSpecialAttack"))
        {
            UpdateRedSpecialAttack();
        }
        else if (pendingMove)
        {
            Rotate();
            Move();
            pendingMove = false;
        }
        else if (attackMoving)
        {
            Rotate();
            Move();
        }
    }

    public void ChangeStance(PlayerStanceType typeStance)
    {
        switch (typeStance)
        {
            case PlayerStanceType.STANCE_NONE:
                Debug.Log("NO STANCE");
                this.stance = stances[typeStance];
                currentStats = baseStats;
                ChangeSwordParticles(typeStance);
                ChangeEmissives(typeStance);
                break;
            case PlayerStanceType.STANCE_BLUE:
                if (greyAbilityEnabled)
                {
                    Debug.Log("BLUE STANCE");
                    this.stance = stances[typeStance];
                    currentStats = baseStats * blueStats;
                    ChangeSwordParticles(typeStance);
                    ChangeEmissives(typeStance);
                }
                break;
            case PlayerStanceType.STANCE_RED:
                if (redAbilityEnabled)
                {
                    Debug.Log("RED STANCE");
                    this.stance = stances[typeStance];
                    currentStats = baseStats * redStats;
                    ChangeSwordParticles(typeStance);
                    ChangeEmissives(typeStance);
                }
                break;
        }
        uiManager.UpdatePlayerStance(stance);
    }

    private void ChangeSwordParticles(PlayerStanceType value)
    {
        switch (value)
        {
            case PlayerStanceType.STANCE_NONE:
                swordBlueOrb.SetActive(false);
                swordBlueLine.SetActive(false);
                swordRedOrb.SetActive(false);
                swordRedLine.SetActive(false);
                swordEmitter.ChangeMaterial(0);
                shieldEmitter.ChangeMaterial(0);
                currentHeavyAttackParticles = greyHeavyAttackParticles;
                break;
            case PlayerStanceType.STANCE_BLUE:
                swordBlueOrb.SetActive(true);
                swordBlueLine.SetActive(true);
                swordRedOrb.SetActive(false);
                swordRedLine.SetActive(false);
                swordEmitter.ChangeMaterial(1);
                shieldEmitter.ChangeMaterial(1);
                currentHeavyAttackParticles = blueHeavyAttackParticles;
                break;
            case PlayerStanceType.STANCE_RED:
                swordBlueOrb.SetActive(false);
                swordBlueLine.SetActive(false);
                swordRedOrb.SetActive(true);
                swordRedLine.SetActive(true);
                swordEmitter.ChangeMaterial(2);
                shieldEmitter.ChangeMaterial(2);
                currentHeavyAttackParticles = redHeavyAttackParticles;
                break;
        }
    }

    private void ChangeEmissives(PlayerStanceType value)
    {
        switch (value)
        {
            case PlayerStanceType.STANCE_NONE:
                baseMat.SetTexture("_EmissionMap", baseGrey);
                extraMat.SetTexture("_EmissionMap", extraGrey);
                shieldMat.SetTexture("_EmissionMap", shieldGrey);
                break;
            case PlayerStanceType.STANCE_BLUE:
                baseMat.SetTexture("_EmissionMap", baseBlue);
                extraMat.SetTexture("_EmissionMap", extraBlue);
                shieldMat.SetTexture("_EmissionMap", shieldBlue);
                break;
            case PlayerStanceType.STANCE_RED:
                baseMat.SetTexture("_EmissionMap", baseRed);
                extraMat.SetTexture("_EmissionMap", extraRed);
                shieldMat.SetTexture("_EmissionMap", shieldRed);
                break;
        }
    }

    public void EnableBlueAbility()
    {
        greyAbilityEnabled = true;
    }

    public void EnableRedAbility()
    {
        redAbilityEnabled = true;
    }

    public bool IsBlueAbilityEnabled()
    {
        return greyAbilityEnabled;
    }

    public bool IsRedAbilityEnabled()
    {
        return redAbilityEnabled;
    }

    public bool TakeDamage(int value)
    {
        if ((!IsDead()) && (!debugMode))
        {
            return LoseHp(value);
        }
        return false;
    }

    private bool LoseHp(int value)
    {
        if (!blocking)
        {
            dmged = true;
            timer = 0;
            currentHP -= value;
            uiManager.UpdateHealth(currentHP);
        }
        return false;
    }

    public bool Heal(int value)
    {
        bool ret = false;
        if (!IsDead() && currentHP < maxHP)
        {
            currentHP += value;
            if (currentHP > maxHP)
            {
                currentHP = maxHP;
            }
            uiManager.UpdateHealth(currentHP);
            ret = true;
        }
        return ret;
    }

    public void IncreaseMaxHealthAndHeal(int value)
    {
        maxHP += value;
        currentHP = maxHP;
        uiManager.UpdateMaxHealth(maxHP);
        uiManager.UpdateHealth(currentHP);
    }

    public void Die()
    {
        dead = true;
    }

    public bool IsDead()
    {
        return dead;
    }

    public void Respawn()
    {
        anim.SetTrigger("respawn");
        if (respawnManager != null)
        {
            transform.position = respawnManager.GetRespawnPoint();
        }
        currentHP = maxHP;
        uiManager.UpdateHealth(currentHP);
        dead = false;
    }

    public bool CanLoseEnergy(int value)
    {
        bool ret = false;
        if (currentEnergy >= value)
        {
            ret = true;
        }
        return ret;
    }

    public bool LoseEnergy(int value)
    {
        bool ret = false;
        if (currentEnergy > 0)
        {
            currentEnergy -= value;
            ret = true;
            if (currentEnergy < 0)
            {
                currentEnergy = 0;
            }
            uiManager.UpdateEnergy(currentEnergy);
        }
        return ret;
    }
    public bool GainEnergy(int value)
    {
        bool ret = false;
        if (currentEnergy < maxEnergy)
        {
            currentEnergy += value;
            if (currentEnergy > maxEnergy)
            {
                currentEnergy = maxEnergy;
            }
            uiManager.UpdateEnergy(currentEnergy);
            ret = true;
        }
        return ret;
    }

    public Attack ChangeAttack(string name)
    {
        currentAttack = playerAttacks[name];
        return playerAttacks[name];

    }

    public void IncreaseMaxEnergy(int value)
    {
        maxEnergy += value;
        currentEnergy = maxEnergy;
        uiManager.UpdateEnergyCapacity(maxEnergy);
        uiManager.UpdateEnergy(currentEnergy);
    }

    public void Rotate()
    {
        camForward = camT.TransformDirection(Vector3.forward);
        camForward.y = 0;
        camForward.Normalize();

        Debug.DrawRay(transform.position, camForward, Color.black);
        Debug.DrawRay(transform.position, transform.forward, Color.cyan);

        camRight = new Vector3(camForward.z, 0, -camForward.x);
        Debug.DrawRay(transform.position, camRight, Color.red);

        targetDirection = camForward * movementVertical + camRight * movementHorizontal;
        Debug.DrawRay(transform.position, targetDirection, Color.blue);

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    public void PendingMovement(float h, float v)
    {
        movementHorizontal = h;
        movementVertical = v;
        pendingMove = true;
    }

    public void Move()
    {

        movement = Vector3.zero;
        //movement.z = 0;
        float movementHorizontalTemp = Mathf.Abs(movementHorizontal);
        float movementVerticalTemp = Mathf.Abs(movementVertical);
        float totalImpulse = movementHorizontalTemp + movementVerticalTemp;
        totalImpulse = (totalImpulse > 1) ? 1 : totalImpulse;
        movement.x = totalImpulse * targetDirection.x;
        movement.z = totalImpulse * targetDirection.z;
        movement.Normalize();

        rigidBody.MovePosition(rigidBody.position + movement * currentStats.movementSpeed * Time.deltaTime);

    }

    public void StartAttackMovement()
    {
        attackMoving = true;
    }

    public void EndAttackMovement()
    {
        attackMoving = false;
    }

    // Testing to use generic start and end current attack so we can change easily 
    // what colliders we want to use for each attack
    // Use a Enum instead of string would be a good idea for attacks
    public void StartCurrentAttack()
    {
        switch (currentAttack.name)
        {
            case "L1":
            case "L2":
            case "L3":
            case "H1":
                StartSwordAttack();
                break;
            case "H2":
            case "H3":
                StartShieldAttack();
                break;
            case "Red":
                StartRedSpecialAttack();
                break;
            case "Blue":
                StartBlueSpecialAttack();
                break;
        }
    }

    public void EndCurrentAttack()
    {
        switch (currentAttack.name)
        {
            case "L1":
            case "L2":
            case "L3":
            case "H1":
                EndSwordAttack();
                DisableSwordArea();
                break;
            case "H2":
            case "H3":
                EndShieldAttack();
                DisableShieldArea();
                break;
            case "Red":
                EndRedSpecialAttack();
                break;
            case "Blue":
                EndBlueSpecialAttack();
                break;
        }
    }

    //TODO: General collider activition method (only depends from which is current attack) 
    protected void StartSwordAttack()
    {
        sword.enabled = true;
    }

    protected void EndSwordAttack()
    {
        sword.enabled = false;
    }

    protected void StartShieldAttack()
    {
        shield.enabled = true;
    }

    protected void EndShieldAttack()
    {
        shield.enabled = false;
    }

    //TODO END

    public void Dodge()
    {
        rigidBody.MovePosition(transform.position + transform.forward * dodgeThrust * Time.deltaTime);
    }

    public void StartDodgeTimer()
    {
        dodgeTimer = Time.time;
    }

    public void SetBlocking(bool value)
    {
        blocking = value;
    }

    public PlayerStanceType SpecialAttackToPerform()
    {
        PlayerStanceType ret = PlayerStanceType.STANCE_NONE;
        canSpecialAttack = false;
        if (stance.type != PlayerStanceType.STANCE_NONE && CanLoseEnergy(1))
        {
            canSpecialAttack = true;
            ret = stance.type;
        }

        return ret;
    }

    protected void StartBlueSpecialAttack()
    {
        canSpecialAttack = false;
        if (stance.type == PlayerStanceType.STANCE_BLUE && LoseEnergy(1))
        {
            canSpecialAttack = true;
            neutralSphere.gameObject.SetActive(true);
            spawnedParticle = Instantiate(neutralAttackParticles, neutralSphere.transform.position + new Vector3(0, 1, 0), neutralSphere.transform.rotation);
            ParticleSystem ps = spawnedParticle.GetComponent<ParticleSystem>();
            float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(spawnedParticle, totalDuration);
            if (camShake != null)
            {
                StartCoroutine(camShake.Shake());
            }
            controllerEvents.AddRumble(0.4f, new Vector2(1.0f, 0.0f), 0.2f);
        }
    }
    protected void StartRedSpecialAttack()
    {
        canSpecialAttack = false;
        if (stance.type == PlayerStanceType.STANCE_RED)
        {
            if (LoseEnergy(1))
            {
                canSpecialAttack = true;
                redSpehre.gameObject.SetActive(true);
            }
            controllerEvents.AddRumble(0.8f, new Vector2(0.1f, 0.2f), 0.2f);
        }
    }
    protected void UpdateRedSpecialAttack()
    {
        if (canSpecialAttack)
        {
            rigidBody.MovePosition(transform.position + transform.forward * redSpecialAttackThrust * Time.deltaTime);
        }
    }

    protected void EndBlueSpecialAttack()
    {
        if (canSpecialAttack)
        {
            neutralSphere.gameObject.SetActive(false);
        }
    }

    protected void EndRedSpecialAttack()
    {
        if (canSpecialAttack)
        {
            redSpehre.gameObject.SetActive(false);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            Interactable interactable = other.gameObject.GetComponent<Interactable>();
            if (interactable != null && interactable.CanInteract())
            {
                canInteract = true;
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("EnemyAttack"))
        {
            EnemyTrash trashScript = other.gameObject.GetComponentInParent<EnemyTrash>();
            Attack currentAttackReceived = trashScript.GetAttack();
            if (currentAttackReceived != null)
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Damaged") == false && anim.GetCurrentAnimatorStateInfo(0).IsName("Block") == false && anim.GetCurrentAnimatorStateInfo(0).IsName("Die") == false)
                {
                    TakeDamage(currentAttackReceived.damage);
                    anim.SetTrigger("damaged");
                }
            }
        }
    }
    public void OnTriggerExit(Collider other)
    {
        // TODO: Possible bug if two interactable triggers are colliding with the player and one exits
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            canInteract = false;
        }
    }

    private void UpdateHPBar()
    {
        float ratio = (float)currentHP / maxHP;
        currentHPBar.rectTransform.localScale = new Vector3(ratio * maxHP / initialMaxHP, 1, 1);
    }

    private void UpdateEnergyBar()
    {
        float ratio = (float)currentEnergy / maxEnergy;
        currentEnergyBar.rectTransform.localScale = new Vector3(ratio * maxEnergy / initialMaxEnergy, 1, 1);
    }

    public void OpenInputWindow()
    {
        inputWindow = true;
        //Debug.Log("window opened");
    }

    public void CloseInputWindow()
    {
        inputWindow = false;
        //Debug.Log("window closed");
    }

    public bool GetInputWindowState()
    {
        return inputWindow;
    }

    public void EnableComboInput()
    {
        canChangeAttackState = false;
    }

    public void DisableComboInput()
    {
        canChangeAttackState = true;
    }

    public bool GetCanChangeAttackState()
    {
        return canChangeAttackState;
    }


    public Attack GetAttack()
    {
        return currentAttack == null ? null : playerAttacks[currentAttack.name];
    }

    public void EnableSwordEmitter()
    {
        swordEmitter.Emit = true;
    }

    public void DisableSwordEmitter()
    {
        swordEmitter.Emit = false;
    }

    public void EnableShieldEmitter()
    {
        shieldEmitter.Emit = true;
    }

    public void DisableShieldEmitter()
    {
        shieldEmitter.Emit = false;
    }
    public bool IsDodge
    {
        get
        {
            return isDodge;
        }

        set
        {
            isDodge = value;
        }
    }

    public void SpawnHitParticles(Vector3 position)
    {
        GameObject particle = Instantiate(hitParticles, position, transform.rotation);
        ParticleSystem ps = particle.GetComponent<ParticleSystem>();
        float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(particle, totalDuration);
    }

    public void PlayRedAbilityParticles()
    {
        redAbilityParticles.Play();
    }

    public void StopRedAbilityParticles()
    {
        redAbilityParticles.Stop();
    }

    public void PlayDodgeParticles()
    {
        dodgeParticles.Play();
    }

    public void StopDodgeParticles()
    {
        dodgeParticles.Stop();
    }
}
