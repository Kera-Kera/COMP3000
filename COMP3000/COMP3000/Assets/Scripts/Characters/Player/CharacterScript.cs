using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class CharacterScript : BaseCharScript
{
    [SerializeField]
    private CharacterController controller;

    [SerializeField]
    private Transform DeathCam;

    [SerializeField]
    private GameObject crosshair;

    private BoxCollider wallCollider;

    private bool sniperScoped = false;
    [SerializeField]
    private float speed = 12f;
    [SerializeField]
    private float walkingSpeed;
    private float currentSpeed;
    private bool isStealth = false;
    private bool playerLanded;

    [SerializeField]
    private GameObject PrimaryGun;
    [SerializeField]
    private GameObject SecondaryGun;



    [SerializeField]
    private float cameraSensitivity = 100f;
    [SerializeField]
    private float gravity = -9f;

    private float health = 100f;
    [SerializeField]
    private Slider healthBar;
    private Transform[] SpawnPoints;
    private int playerScore = 0;
    [SerializeField]
    private Text PlayerScoreTextbox;
    private bool pointsCheck = false;

    [SerializeField]
    private Transform itemPickups;
    private Transform inventory;
    private Transform currentGun;
    private int currentGunInt = 0;
    private bool isGunReady = true;
    private bool alreadyAttacked = false;
    [SerializeField]
    private GameObject bullet;

    [SerializeField]
    private Transform AI;
    private BaseAIScript AIScript;


    [SerializeField]
    private bool isGrounded;
    [SerializeField]
    private bool isGroundedFloating;
    [SerializeField]
    private float groundDistance = 0.4f;
    [SerializeField]
    private LayerMask groundMask, floatingMask, weaponMask;
    private Vector3 move;

    private Vector3 velocity;
    private float rotationY = 0F;
    private float YVel;

    // Start is called before the first frame update
    private void Start()
    {
        
        
        Cursor.lockState = CursorLockMode.Locked;
        AIScript = AI.GetComponent<BaseAIScript>();
        wallCollider = GetComponent<BoxCollider>();
        int NumSpawns = GameObject.FindGameObjectWithTag("SpawnPoints").transform.childCount;
        SpawnPoints = new Transform[NumSpawns];
        for (int i = 0; i < NumSpawns; i++)
        {
            
            SpawnPoints[i] = GameObject.FindGameObjectWithTag("SpawnPoints").transform.GetChild(i).transform;
        }
        Spawn();
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
    }
    // Update is called once per frame
    private void Update()
    {
        GroundCheck();
        FPSCamera();
        PlayerInputs();
        Gravity();
        getMovement();
    }
    private void GroundCheck()
    {
        
        isGrounded = Physics.CheckSphere(transform.GetChild(2).position, groundDistance, groundMask);
        isGroundedFloating = Physics.CheckSphere(transform.GetChild(2).position, groundDistance, floatingMask);
        YVel = velocity.y;
        if ((isGrounded || isGroundedFloating) && velocity.y < 0)
        {
            if (velocity.y < -9f)
            {
                AIScript.NoiseAlert(false);
            }
            velocity.y = -2f;
        }
    }
    private void FPSCamera()
    {
        transform.Rotate(0, Input.GetAxis("Mouse X") * cameraSensitivity, 0);

        rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity;
        rotationY = Mathf.Clamp(rotationY, -80f, 85f);

        transform.GetChild(0).transform.localEulerAngles = new Vector3(-rotationY, transform.GetChild(0).transform.localEulerAngles.y, 0);
    }
    private void PlayerInputs()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        move = transform.right * x + transform.forward * z;

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(3f * -2f * gravity);
        }
        if (Input.GetMouseButton(0) && isGunReady)
        {
            PrimaryFire();
        }
        if (Input.GetMouseButtonDown(1) && isGunReady)
        {
            SecondaryFire();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1) && isGunReady && inventory.childCount!=0)
        {
            int otherWeapon;
            
            if(currentGun.GetSiblingIndex() == 0)
            {
                otherWeapon = 1;
            }
            else
            {
                otherWeapon = 0;
            }
            StartCoroutine(ChangeWeapon(currentGun.GetSiblingIndex(), otherWeapon));
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(PickUpWeapon());
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isStealth = true;
            currentSpeed = walkingSpeed;
        }
        else
        {
            if (controller.velocity.magnitude != 0)
            {
                isStealth = false;
            }
            currentSpeed = speed;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }
    }
    private void Gravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }

    IEnumerator ChangeWeapon(int currentWeapon, int newWeapon)
    {
        Camera.main.fieldOfView = 60;
        isGunReady = false;
        if (inventory.childCount > 0)
        {
            inventory.GetChild(currentWeapon).gameObject.SetActive(false);
            yield return new WaitForSeconds(0.3f);
            if (inventory.childCount != 0)
            {
                inventory.GetChild(newWeapon).gameObject.SetActive(true);
                currentGunInt = newWeapon;
                currentGun = inventory.GetChild(currentGunInt);
            }
        }
        isGunReady = true;
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

    

    private void PrimaryFire()
    {
        if (inventory.childCount > 0)
        {
            currentGun = inventory.GetChild(currentGunInt);
            if (!alreadyAttacked)
            {
                Shoot();

                alreadyAttacked = true;

                Invoke(nameof(ResetAttack), currentGun.GetComponent<GunInfo>().GetFireRate());
            }
        }
    }

    private void SecondaryFire()
    {
        if (inventory.childCount > 0)
        {
            currentGun = inventory.GetChild(currentGunInt);
            if (currentGun.name == "Sniper")
            {
                if (!sniperScoped)
                {
                    Camera.main.fieldOfView = 30;
                }
                else
                {
                    Camera.main.fieldOfView = 60;
                }
                sniperScoped = !sniperScoped;
            }
        }
    }

    IEnumerator ZoomingWait(bool sniperScope)
    {

        
        yield return new WaitForSeconds(0.1f);

    }

    private void Shoot()
    {
        GunInfo guninfo = currentGun.GetComponent<GunInfo>();

        for (int i = 0; i < guninfo.GetNumberOfBullets(); i++)
        {
            float ConeSize = guninfo.GetSpread();

            float xSpread = Random.Range(-1.0f, 1.0f);
            float ySpread = Random.Range(-1.0f, 1.0f);
            float zSpread = Random.Range(-1.0f, 1.0f);

            Vector3 spread = new Vector3(xSpread, ySpread, zSpread).normalized * ConeSize;
            Quaternion rotation = Quaternion.Euler(spread) * Camera.main.transform.rotation;



            GameObject bulletFiring = Instantiate(bullet, Camera.main.transform.position + (Camera.main.transform.forward.normalized * 2), rotation);
            bulletFiring.GetComponent<BulletScript>().newBullet(guninfo.GetVelocity(), guninfo.GetDamage(), guninfo.GetSize(), gameObject);
        }


        AIScript.NoiseAlert(true);
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public override void takeDamage(float damage, string whoShot)
    {
        if (!transform.CompareTag(whoShot))
        {
            health -= damage;
            healthBar.value = health;
            if (health <= 0)
            {
                health = 0;
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

    public bool getStealth()
    {
        return isStealth;
    }

    public float getYVel()
    {
        return YVel;
    }

    public Vector3 getVelocity()
    {
        return velocity;
    }

    public Vector3 getMovement()
    {
        Vector3 playerDir = move;
        Vector3 feetArea = transform.position;
        feetArea.y += -1f;
        Ray ray = new Ray(feetArea, playerDir);
        RaycastHit hitInfo;
        Debug.DrawRay(feetArea, playerDir, Color.green);
        
        if (Physics.Raycast(ray, out hitInfo))
        {
            GameObject go = hitInfo.collider.gameObject;
            if (go.layer == 8)
            {
                if (Vector3.Distance(transform.position, hitInfo.point) < 1.5)
                {
                    print("XD");
                    playerDir = new Vector3(0,0,0);
                }
            }
        }
        return playerDir + (velocity/18);
    }
    public void setVelocity(Vector3 newVelocity)
    {
        velocity = newVelocity;
    }

    public void IncreasePlayerScore()
    {
        if (pointsCheck == false)
        {
            StartCoroutine(pointsChecking());
            playerScore += 1;
            PlayerScoreTextbox.text = playerScore.ToString();
        }
    }
    IEnumerator pointsChecking()
    {
        pointsCheck = true;
        yield return new WaitForSeconds(0.2f);
        pointsCheck = false;
    }

    private void SetPickupable(Transform pickup, float rotation)
    {
        pickup.SetParent(itemPickups);
        currentGun.tag = "PickUpable";
        if (!pickup.gameObject.activeSelf)
        {
            pickup.gameObject.SetActive(true);
        }
        pickup.GetComponent<Rigidbody>().useGravity = true;
        pickup.GetComponent<Rigidbody>().isKinematic = false;
        pickup.GetComponent<BoxCollider>().enabled = true;
        pickup.Rotate(0.0f, 0.0f, rotation, Space.World);
    }

    public int GetPlayerScore()
    {
        return playerScore;
    }

    public IEnumerator Respawn()
    {
        SetPickupable(inventory.GetChild(1), 20);
        SetPickupable(inventory.GetChild(0), -20);
        DeathCam.GetComponent<DeathCamera>().UseDeathCam();
        crosshair.SetActive(false);
        gameObject.transform.position = new Vector3(11111, 11111, 1111);
        AI.GetComponent<BaseAIScript>().IncreaseAIScore();
        yield return new WaitForSeconds(2f);
        if (playerScore != 10 || AIScript.GetAIScore() != 10)
        {
            DeathCam.GetComponent<DeathCamera>().DisableDeathCam();
        }


        GameObject Primary = Instantiate(PrimaryGun, inventory);
        Primary.name = "Ak-47";
        Primary.transform.localPosition = new Vector3(0, 0, 0);
        GameObject Secondary = Instantiate(SecondaryGun, inventory);
        Secondary.name = "Pistol";
        Secondary.transform.localPosition = new Vector3(0, 0, 0);
        Secondary.SetActive(false);
        currentGun = Primary.transform;
        
        currentGunInt = 0;


        Spawn();
        
    }

    

    private void Spawn()
    {
        isStealth = true;
        crosshair.SetActive(true);
        int currentNumber = Random.Range(0, SpawnPoints.Length);
        while (Vector3.Distance(SpawnPoints[currentNumber].position, AI.transform.position) < 60)
        {
            currentNumber = Random.Range(0, SpawnPoints.Length);
        }
        Vector3 spawn = SpawnPoints[currentNumber].position;
        Quaternion spawnRot = SpawnPoints[currentNumber].rotation;
        spawn.y += 1.7f;

        Camera.main.fieldOfView = 60;
        //Ak47.transform.name = "Ak-47";

        transform.position = spawn;
        transform.rotation = spawnRot;
        takeHeal(100f);
        rotationY = 0f;
    }
}
