using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BaseAIScript : BaseCharScript
{
    [SerializeField]
    private NavMeshAgent agent;
    [SerializeField]
    private float health = 100f;
    [SerializeField]
    private Slider healthBar;
    private int aiScore = 0;
    [SerializeField]
    private Text aiScoreTextbox;

    private Transform[] SpawnPoints;

    private Transform currentGun;

    [SerializeField]
    private Transform player;

    [SerializeField]
    private GameObject bullet;

    [SerializeField]
    private LayerMask groundMask, playerMask, aiMask;

    [SerializeField]
    private Vector3 walkPoint;
    private bool walkPointSet;
    [SerializeField]
    private float walkPointRange;

    [SerializeField]
    private float attackSpeed;
    private bool alreadyAttacked;

    [SerializeField]
    private float hearingRange, attackRange;
    [SerializeField]
    private bool playerInHearingRange, playerInAttackRange;
    [SerializeField]
    private bool playerInfront;


    private Vector3 lastAIposition;
    private Vector3 playerLastPos;

    private LineRenderer line;
    private CharacterScript playerScript;

    private float patience = 0;
    [SerializeField]
    private int currentchoice = 1;
    [SerializeField]
    private float sightRadius = 180;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        line = GetComponent<LineRenderer>();
        playerScript = player.GetComponent<CharacterScript>();
        currentGun = transform.GetChild(0).GetChild(0).GetChild(0);

        int NumSpawns = GameObject.FindGameObjectWithTag("SpawnPoints").transform.childCount;
        SpawnPoints = new Transform[NumSpawns];
        for (int i = 0; i < NumSpawns; i++)
        {

            SpawnPoints[i] = GameObject.FindGameObjectWithTag("SpawnPoints").transform.GetChild(i).transform;
        }
        Spawn();
    }

    private void Update()
    {
        Attack();

        DrawPath(agent.path);
    }

    #region Player Actions
    private void Attack()
    {
        playerInHearingRange = Physics.CheckSphere(transform.position, hearingRange, playerMask);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerMask);
        playerInfront = Mathf.Abs(Vector3.Angle(player.position - transform.position, transform.forward)) < sightRadius && playerInAttackRange;

        bool ShootableCheck = playerInAttackRange && checkIfSightNotBlocked() && playerInfront && checkIfGunNotBlocked();

        if (health > 30) {
            if (ShootableCheck)
            {
                ShootPlayer();
                patience = 100;
            }
            else if (playerInHearingRange)
            {
                ChasePlayer(!playerScript.getStealth());
            }
            else
            {
                ChasePlayer(false);
            }
        }
        else if(playerScript.getHealth() < 30 && ShootableCheck)
        {
            ShootPlayer();
            ChasePlayer(true);
        }
        else
        {
            Action();
        }
    }

    private bool checkIfSightNotBlocked()
    {
        bool inSight = false;

        Vector3 eyelevel = transform.position;
        eyelevel.y += 1.5f;
        Vector3 playerEyelevel = player.transform.position;
        playerEyelevel.y += 1.5f;

        Vector3 playerDir = transform.GetChild(0).GetChild(1).position - playerEyelevel;
        Ray ray = new Ray(eyelevel, -playerDir);

        

        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo))
        {
            
            GameObject go = hitInfo.collider.gameObject;
            if (go.tag == "Player" || go.tag == "Bullet")
            {
                inSight = true;


            }
            else
            {
                inSight = false;
            }
        }
        return inSight;
    }

    private bool checkIfGunNotBlocked()
    {
        bool inSight = false;

        Vector3 endOfGun = currentGun.GetChild(0).position;
        Vector3 playerEyelevel = player.transform.position;
        playerEyelevel.y += 1.2f;
        playerEyelevel.z += 0.4f;
        Vector3 playerDir = endOfGun - playerEyelevel;
        Ray ray = new Ray(endOfGun, -playerDir);

        
        RaycastHit hitInfo;
        
        if (Physics.Raycast(ray, out hitInfo))
        {
            
            GameObject go = hitInfo.collider.gameObject;
            if (go.tag == "Player" || go.tag == "Bullet")
            {
                inSight = true;


            }
            else
            {
                inSight = false;
            }
        }

        return inSight;
    }

    private void ChasePlayer(bool check)
    {

        if (check)
        {
            agent.SetDestination(player.position);
            Vector3 playerPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(playerPosition);
            playerLastPos = player.position;

        }
        else
        {
            agent.SetDestination(playerLastPos);
            if (Vector3.Distance(agent.destination, agent.transform.position) <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    Action();
                }
            }
        }
    }
    private void ShootPlayer()
    {
        playerLastPos = player.position;
        agent.SetDestination(transform.position);

        Vector3 playerPositionTracked = player.transform.position;
        float distanceFromPlayer = Vector3.Distance(playerPositionTracked, transform.position);
        playerPositionTracked.y += distanceFromPlayer * currentGun.GetComponent<GunInfo>().AIDrop();
        Vector3 playerPosition = player.transform.position;
        playerPosition.y = transform.position.y;
        transform.LookAt(playerPosition);

        playerPositionTracked += playerScript.getMovement() * distanceFromPlayer * 0.07f;

        Vector3 playerDir = currentGun.GetChild(0).position - playerPositionTracked;
        Debug.DrawRay(currentGun.GetChild(0).position, -playerDir, Color.yellow);

        float radius = distanceFromPlayer * 0.02f;
        playerPositionTracked.x += Random.Range(-radius, radius);
        playerPositionTracked.z += Random.Range(-radius, radius);
        playerPositionTracked.y += Random.Range(-radius, radius);
        


        currentGun.LookAt(playerPositionTracked);

        if (!alreadyAttacked)
        {
            Shoot();

            alreadyAttacked = true;

            Invoke(nameof(ResetAttack), currentGun.GetComponent<GunInfo>().GetFireRate());
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void Shoot()
    {
        GunInfo guninfo = currentGun.GetComponent<GunInfo>();
        if (guninfo.GetNumberOfBullets() > 1)
        {
            for (int i = 0; i < guninfo.GetNumberOfBullets(); i++)
            {
                Quaternion ranRotation = currentGun.rotation;
                float radius = 5;
                ranRotation.x += Random.Range(-radius, radius);
                ranRotation.z += Random.Range(-radius, radius);
                ranRotation.y += Random.Range(-radius, radius);


                GameObject bulletFiring = Instantiate(bullet, currentGun.GetChild(0).position, ranRotation);
                bulletFiring.GetComponent<BulletScript>().newBullet(guninfo.GetVelocity(), guninfo.GetDamage(), guninfo.GetSize(), gameObject);
            }
        }
        else
        {
            GameObject bulletFiring = Instantiate(bullet, currentGun.GetChild(0).position, currentGun.rotation);
            bulletFiring.GetComponent<BulletScript>().newBullet(guninfo.GetVelocity(), guninfo.GetDamage(), guninfo.GetSize(), gameObject);
        }
    }

    #endregion Player Actions

    #region AI Actions
    private void Action()
    {
        print("action");
        if (health < 30)
        {
            GoToHealthPack();
        }
        else if (patience <= 100 && patience != 0)
        {
            LookAroundSelf();
        }
        else if (currentGun.name == "Sniper")
        {
            GoToSniperNest();
        }
        else if (currentGun.name == "Ak-47")
        {
            GoToPowerWeapon();
        }
        else if (health != 100)
        {
            GoToHealthPack();
        }
        else
        {
            FindPlayer();
        }

    }

    bool stayCurrentAction(int choicea, int choiceb)
    {
        if (choicea > choiceb)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void GoToHealthPack()
    {
        GameObject closestHealthPack = null;
        float shortest = Mathf.Infinity;
        GameObject[] healthPacks;
        healthPacks = GameObject.FindGameObjectsWithTag("HealthPack");

        foreach (GameObject hp in healthPacks)
        {
            float dist = Vector3.Distance(hp.transform.position, transform.position);
            if (dist < shortest)
            {
                if (!hp.GetComponent<HealthPack>().isRespawning())
                {
                    closestHealthPack = hp;
                    shortest = dist;
                }
                else
                {
                    
                }
            }
        }

        agent.SetDestination(closestHealthPack.transform.position);
    }
    void GoToSniperNest()
    {
        FindPlayer();
    }
    void GoToPowerWeapon()
    {
        FindPlayer();
    }

    void ChangeWeapon()
    {

    }

    void LookAroundSelf()
    {
        if (patience > 0)
        {
            patience -= 500f * Time.deltaTime;
            if (Vector3.Distance(agent.destination, agent.transform.position) <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    print("this1");
                    float radius = 15f;
                    playerLastPos.x += Random.Range(-radius, radius);
                    playerLastPos.z += Random.Range(-radius, radius);
                    agent.SetDestination(playerLastPos);
                }
            }
        }
        else
        {
            patience = 0;
        }
    }



    void FindPlayer()
    {
        if (player.gameObject.activeSelf == false)
        {
            float radius = 60f;
            playerLastPos =  transform.position;
            playerLastPos.x += Random.Range(-radius, radius);
            playerLastPos.z += Random.Range(-radius, radius);
            agent.SetDestination(playerLastPos);
        }
        else
        {
            if (Vector3.Distance(agent.destination, agent.transform.position) <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    print("this2");
                    float radius = 20f;
                    playerLastPos = (player.position + transform.position) / 2;
                    playerLastPos.x += Random.Range(-radius, radius);
                    playerLastPos.z += Random.Range(-radius, radius);
                    agent.SetDestination(playerLastPos);
                }
            }
        }
    }

    public void NoiseAlert(bool loud)
    {
        if (playerInHearingRange)
        {
            playerLastPos = player.position;
        }
        if (loud)
        {
            playerLastPos = player.position;
        }
        patience = 100;
    }

    public override void takeDamage(float damage, string whoShot)
    {
        if (!transform.CompareTag(whoShot))
        {
            health -= damage;
            healthBar.value = health;
            if (health <= 0) { health = 0;
                StartCoroutine(Respawn());
            }
        }
    }

    public override void takeHeal(float damage)
    {
        health += damage;
        healthBar.value = health;
        if (health > 100) health = 100;
    }

    public override float getHealth()
    {
        return health;
    }
    #endregion AI Actions

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }

    public void IncreaseAIScore()
    {
        aiScore += 1;
        aiScoreTextbox.text = aiScore.ToString();
        playerLastPos = transform.position;
        patience = 0f;
    }

    public IEnumerator Respawn()
    {
        agent.Warp(new Vector3(0, -38.9f, 24.579f));
        player.GetComponent<CharacterScript>().IncreasePlayerScore();
        yield return new WaitForSeconds(2f);
        Spawn();
    }

    private void Spawn()
    {
        int currentNumber = Random.Range(0, SpawnPoints.Length);
        while (Vector3.Distance(SpawnPoints[currentNumber].position, player.transform.position) < 60)
        {
            currentNumber = Random.Range(0, SpawnPoints.Length);
        }
        Vector3 spawn = SpawnPoints[currentNumber].position;
        Quaternion spawnRot = SpawnPoints[currentNumber].rotation;
        spawn.y += 1.7f;
        agent.Warp(spawn);
        takeHeal(100f);
        transform.rotation = spawnRot;
        playerLastPos = transform.position;
    }

    private void DrawPath(NavMeshPath path)
    {
        if (path.corners.Length < 2) //if the path has 1 or no corners, there is no need
            return;

        line.positionCount = path.corners.Length;

        for (int i = 0; i < path.corners.Length; i++)
        {
            line.SetPosition(i, path.corners[i]); //go through each corner and set that to the line renderer's position
        }
    }
}