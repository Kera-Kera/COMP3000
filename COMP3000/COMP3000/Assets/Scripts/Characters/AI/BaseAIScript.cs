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
    private bool pointsCheck = false;

    [SerializeField]
    private GameObject PrimaryGun;
    [SerializeField]
    private GameObject SecondaryGun;

    private Transform[] SpawnPoints;

    [SerializeField]
    private Transform itemPickups;
    private Transform inventory;
    private Transform currentGun;
    private int currentGunInt = 0;
    private bool isGunReady = true;

    [SerializeField]
    private Transform player;

    private bool isBored = false;

    [SerializeField]
    private GameObject bullet;

    [SerializeField]
    private LayerMask groundMask, playerMask, aiMask, weaponMask;

    [SerializeField]
    private GameObject locations;
    
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

    private bool Pathcheck1 = true;
    private bool Pathcheck2 = true;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        line = GetComponent<LineRenderer>();
        playerScript = player.GetComponent<CharacterScript>();
        inventory = transform.GetChild(0).GetChild(0);

        int NumSpawns = GameObject.FindGameObjectWithTag("SpawnPoints").transform.childCount;
        SpawnPoints = new Transform[NumSpawns];
        for (int i = 0; i < NumSpawns; i++)
        {

            SpawnPoints[i] = GameObject.FindGameObjectWithTag("SpawnPoints").transform.GetChild(i).transform;
        }
        inventory = transform.GetChild(0).GetChild(0);
        GameObject Primary = Instantiate(PrimaryGun, inventory);
        Primary.transform.localPosition = new Vector3(0, 0, 0);
        Primary.GetComponent<Rigidbody>().useGravity = false;
        GameObject Secondary = Instantiate(SecondaryGun, inventory);
        Secondary.SetActive(false);
        Secondary.transform.localPosition = new Vector3(0, 0, 0);
        Secondary.GetComponent<Rigidbody>().useGravity = false;
        currentGun = inventory.GetChild(currentGunInt);
        currentGun.GetComponent<BoxCollider>().enabled = false;
        inventory.GetChild(1).GetComponent<BoxCollider>().enabled = false;
        Primary.name = "Ak-47";
        Secondary.name = "Pistol";
        Spawn();
    }

    private void Update()
    {
        Check();
    }

    #region Player Actions
    private void Check()
    {
        playerInHearingRange = Physics.CheckSphere(transform.position, hearingRange, playerMask);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerMask);
        playerInfront = Mathf.Abs(Vector3.Angle(player.position - transform.position, transform.forward)) < sightRadius && playerInAttackRange;

        bool ShootableCheck = playerInAttackRange && checkIfSightNotBlocked() && playerInfront && checkIfGunNotBlocked();
        if (health > 30 ) {
            if (ShootableCheck && currentGun.name != "Shotgun")
            { 
                ShootPlayer();
                patience = 100;
                if (Vector3.Distance(player.position, transform.position) < 10 && inventory.GetChild(1).name == "Shotgun")
                {
                    StartCoroutine(ChangeWeapon(currentGun.GetSiblingIndex(), GetOtherWeapon()));
                }
            }
            else if (ShootableCheck && currentGun.name == "Shotgun")
            {
                ShootPlayer();
                if (Vector3.Distance(transform.position, player.position) > 8 )
                {
                    ChasePlayer(true);
                    if (Vector3.Distance(transform.position, player.position) > 11 && currentGun.name == "Shotgun")
                    {
                        StartCoroutine(ChangeWeapon(currentGun.GetSiblingIndex(), GetOtherWeapon()));
                    }
                }
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
        else if (playerScript.getHealth() < currentGun.GetComponent<GunInfo>().GetDamage() && ShootableCheck)
        {
            ShootPlayer();
        }
        else if(playerScript.getHealth() < 30 && health < 30 && ShootableCheck)
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
            isBored = false;
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
        if (inventory.childCount > 0)
        {
            playerLastPos = player.position;
            agent.SetDestination(transform.position);
            currentGun = inventory.GetChild(currentGunInt);
            currentGun.LookAt(playerLastPos);
            if (!alreadyAttacked)
            {
                Shoot();

                alreadyAttacked = true;

                Invoke(nameof(ResetAttack), currentGun.GetComponent<GunInfo>().GetFireRate());
            }
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void Shoot()
    {
        GunInfo guninfo = currentGun.GetComponent<GunInfo>();

        Vector3 playerPositionTracked = player.transform.position;
        float distanceFromPlayer = Vector3.Distance(playerPositionTracked, transform.position);
        playerPositionTracked.y += distanceFromPlayer * guninfo.AIDrop();
        Vector3 playerPosition = player.transform.position;
        playerPosition.y = transform.position.y;
        transform.LookAt(playerPosition);

        playerPositionTracked += playerScript.getMovement() * distanceFromPlayer * (0.03f + guninfo.AICorrection());

        Vector3 playerDir = currentGun.GetChild(0).position - playerPositionTracked;
        Debug.DrawRay(currentGun.GetChild(0).position, -playerDir, Color.yellow);

        if (currentGun.name == "Sniper")
        {
            float radius = distanceFromPlayer * 0.005f;
            playerPositionTracked.x += Random.Range(-radius, radius);
            playerPositionTracked.z += Random.Range(-radius, radius);
            playerPositionTracked.y += Random.Range(-radius, radius);
        }
        else
        {
            float radius = distanceFromPlayer * 0.02f;
            playerPositionTracked.x += Random.Range(-radius, radius);
            playerPositionTracked.z += Random.Range(-radius, radius);
            playerPositionTracked.y += Random.Range(-radius, radius);
        }

        currentGun.LookAt(playerPositionTracked);

        
        if (guninfo.GetNumberOfBullets() > 1)
        {
            for (int i = 0; i < guninfo.GetNumberOfBullets(); i++)
            {
                Quaternion ranRotation = currentGun.rotation;
                float ConeSize = 5;

                float xSpread = Random.Range(-1.0f, 1.0f);
                float ySpread = Random.Range(-1.0f, 1.0f);
                float zSpread = Random.Range(-1.0f, 1.0f);
                Vector3 spread = new Vector3(xSpread, ySpread, zSpread).normalized * ConeSize;

                Quaternion rotation = Quaternion.Euler(spread) * ranRotation;


                GameObject bulletFiring = Instantiate(bullet, currentGun.GetChild(0).position, rotation);
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
        if (health < 30)
        {
            GoToHealthPack();
        }
        else if (patience <= 100 && patience != 0)
        {
            print(patience);
            LookAroundSelf();
        }
        else if (inventory.GetChild(0).name != "Shotgun" && inventory.GetChild(1).name != "Shotgun" && Pathcheck1)
        {
            GoToWeapon("Shotgun", "Ak-47", 1);
        }
        
        else if (inventory.GetChild(0).name != "Sniper" && inventory.GetChild(1).name != "Sniper" && Pathcheck2)
        {
            GoToWeapon("Sniper" , "Shotgun", 2);
        }
        else if (health != 100)
        {
            GoToHealthPack();
        }
        else if (currentGun.name == "Sniper" && !isBored)
        {
            GoToSniperNest();
        }
        else
        {
            FindPlayer();
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
        GameObject closestLocal = null;
        float shortest = Mathf.Infinity;
        GameObject[] foundlocations;
        foundlocations = GameObject.FindGameObjectsWithTag("SniperNest");
        foreach (GameObject local in foundlocations)
        {
            float dist = Vector3.Distance(local.transform.position, transform.position);
            if (dist < shortest)
            {
                closestLocal = local;
                shortest = dist;
            }
        }
        playerLastPos = closestLocal.transform.position;
        agent.SetDestination(playerLastPos);
        if(Vector3.Distance(playerLastPos, transform.position) < 5)
        {
            StartCoroutine(SniperWaiting());
        }
    }

    IEnumerator SniperWaiting()
    {
        yield return new WaitForSeconds(20);
        isBored = true;
    }


    private void GoToWeapon(string weaponName, string gunKeep, int pathcheck)
    {

        GameObject closestWeapon = null;
        float shortest = Mathf.Infinity;
        GameObject[] weapons;
        weapons = GameObject.FindGameObjectsWithTag("PickUpable");

        foreach (GameObject weapon in weapons)
        {
            float dist = Vector3.Distance(weapon.transform.position, transform.position);
            if (dist < shortest)
            {
                if (weapon.name == weaponName)
                {
                    closestWeapon = weapon;
                    shortest = dist;
                    
                }
            }
        }

        if (closestWeapon == null)
        {
            if (pathcheck == 1) Pathcheck1 = false;
            if (pathcheck == 2) Pathcheck2 = false;
        }
        else
        {
            if (pathcheck == 1) Pathcheck1 = true;
            if (pathcheck == 2) Pathcheck2 = true;

            playerLastPos = closestWeapon.transform.position;
            agent.SetDestination(playerLastPos);

            if (Vector3.Distance(closestWeapon.transform.position, transform.position) < 3)
            {
                if (currentGun.name == gunKeep)
                {
                    StartCoroutine(ChangeWeapon(currentGun.GetSiblingIndex(), GetOtherWeapon()));
                }
                StartCoroutine(PickUpWeapon());
            }
        }       
    }

    private int GetOtherWeapon()
    {
        if (currentGun.GetSiblingIndex() == 0)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    IEnumerator ChangeWeapon(int currentWeapon, int newWeapon)
    {

        isGunReady = false;
        inventory.GetChild(currentWeapon).gameObject.SetActive(false);
        yield return new WaitForSeconds(0.3f);
        inventory.GetChild(newWeapon).gameObject.SetActive(true);
        currentGunInt = newWeapon;
        currentGun = inventory.GetChild(currentGunInt);
        isGunReady = true;
    }

    void LookAroundSelf()
    {
        if (patience > 0)
        {
            patience -= 10000f * Time.deltaTime;
            if (Vector3.Distance(agent.destination, agent.transform.position) <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
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
        if (inventory.childCount > 0)
        {
            if ((inventory.GetChild(0).name == "Sniper" || inventory.GetChild(1).name == "Sniper") && Vector3.Distance(player.position, transform.position) > 20)
            {
                if (playerInHearingRange)
                {
                    playerLastPos = player.position;
                }
                if (loud)
                {
                    playerLastPos = player.position;
                }
            }
        }
    }

    public override void takeDamage(float damage, string whoShot)
    {
        if (!transform.CompareTag(whoShot))
        {
            health -= damage;
            healthBar.value = health;
            if (health <= 0) { health = 0;
                if (inventory.childCount > 0)
                {
                    StartCoroutine(Respawn());
                }
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
        if (pointsCheck == false)
        {
            StartCoroutine(pointsChecking());
            aiScore += 1;
            aiScoreTextbox.text = aiScore.ToString();
            playerLastPos = transform.position;
            patience = 0f;
        }
    }
    IEnumerator pointsChecking()
    {
        pointsCheck = true;
        yield return new WaitForSeconds(0.2f);
        pointsCheck = false;
    }

    IEnumerator PickUpWeapon()
    {
        bool itemInRange = Physics.CheckSphere(transform.position, 5, weaponMask);
        
        if (itemInRange && isGunReady)
        {
            
            isGunReady = false;
            GameObject closestWeapon = null;
            float shortest = Mathf.Infinity;
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5, weaponMask);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.transform.tag == "PickUpable")
                {
                    float dist = Vector3.Distance(hitCollider.transform.position, transform.position);
                    if (dist < shortest)
                    {
                        closestWeapon = hitCollider.gameObject;
                        shortest = dist;
                    }
                }
            }
            yield return new WaitForSeconds(0.2f);
            if (closestWeapon != null)
            {
                SetPickupable(currentGun, 20);
                currentGun.position = closestWeapon.transform.position;
                currentGun.rotation = closestWeapon.transform.rotation;

                closestWeapon.transform.SetParent(inventory);
                closestWeapon.transform.localPosition = new Vector3(0, 0, 0);
                closestWeapon.transform.localRotation = new Quaternion(0, 0, 0, 0);
                closestWeapon.transform.SetSiblingIndex(currentGunInt);
                currentGun = inventory.GetChild(currentGunInt);
                currentGun.GetComponent<Rigidbody>().useGravity = false;
                currentGun.GetComponent<Rigidbody>().isKinematic = true;
                currentGun.GetComponent<BoxCollider>().enabled = false;
                currentGun.tag = "Weapon";
                isGunReady = true;
            }
        }
    }

    private void SetPickupable(Transform pickup, float rotation)
    {
        pickup.SetParent(itemPickups);
        pickup.tag = "PickUpable";
        if (!pickup.gameObject.activeSelf)
        {
            pickup.gameObject.SetActive(true);
        }
        pickup.GetComponent<Rigidbody>().useGravity = true;
        pickup.GetComponent<Rigidbody>().isKinematic = false;
        pickup.GetComponent<BoxCollider>().enabled = true;
        pickup.Rotate(0.0f, 0.0f, rotation, Space.Self);
    }

    public IEnumerator Respawn()
    {
        SetPickupable(inventory.GetChild(1), 20);
        SetPickupable(inventory.GetChild(0), -20);
        agent.Warp(new Vector3(0, -38.9f, 24.579f));
        yield return new WaitForSeconds(0.2f);
        player.GetComponent<CharacterScript>().IncreasePlayerScore();
        yield return new WaitForSeconds(1.8f);

        GameObject Primary = Instantiate(PrimaryGun, inventory);
        Primary.transform.localPosition = new Vector3(0, 0, 0);
        GameObject Secondary = Instantiate(SecondaryGun, inventory);
        Secondary.transform.localPosition = new Vector3(0, 0, 0);
        Secondary.SetActive(false);
        currentGun = Primary.transform;

        Primary.name = "Ak-47";
        Secondary.name = "Pistol";

        currentGunInt = 0;
        patience = 0;
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

    public int GetAIScore()
    {
        return aiScore;
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