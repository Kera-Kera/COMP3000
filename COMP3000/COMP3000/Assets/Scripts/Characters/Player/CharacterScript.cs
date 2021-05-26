using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class CharacterScript : BaseCharScript
{
    [SerializeField]
    private CharacterController controller;

    [SerializeField]
    private Transform DeathCam;

    private BoxCollider wallCollider;

    [SerializeField]
    private float speed = 12f;
    [SerializeField]
    private float walkingSpeed;
    private float currentSpeed;
    private bool isStealth = false;
    private bool playerLanded;

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

    private Transform currentGun;
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
    private LayerMask groundMask, floatingMask;
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
        
    }
    // Update is called once per frame
    private void Update()
    {
        GroundCheck();
        FPSCamera();
        PlayerInputs();
        Gravity();
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
        if (Input.GetMouseButton(0))
        {
            PrimaryFire();
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
    }
    private void Gravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }



    private void PrimaryFire()
    {

        currentGun = transform.GetChild(0).GetChild(0).GetChild(0);
        
        if (!alreadyAttacked)
        {
            Shoot();

            alreadyAttacked = true;

            Invoke(nameof(ResetAttack), currentGun.GetComponent<GunInfo>().GetFireRate());
        }
    }

    private void Shoot()
    {
        GunInfo guninfo = currentGun.GetComponent<GunInfo>();
        if (guninfo.GetNumberOfBullets() > 1)
        {
            for (int i = 0; i < guninfo.GetNumberOfBullets(); i++)
            {
                  Quaternion ranRotation = Camera.main.transform.rotation;
                //  float radius = 5;
                // ranRotation.x += Random.Range(-radius, radius);
                // ranRotation.z += Random.Range(-radius, radius);
                //ranRotation.y += Random.Range(-radius, radius);
                float ConeSize = 5;

                float xSpread = Random.Range(-1, 1);
                float ySpread = Random.Range(-1, 1);
                //normalize the spread vector to keep it conical
                Vector3 spread = new Vector3(xSpread, ySpread, 0.0f).normalized * ConeSize;
                Quaternion rotation = Quaternion.Euler(spread) * ranRotation;


                GameObject bulletFiring = Instantiate(bullet, Camera.main.transform.position + (Camera.main.transform.forward.normalized * 2), rotation);
                bulletFiring.GetComponent<BulletScript>().newBullet(guninfo.GetVelocity(), guninfo.GetDamage(), guninfo.GetSize(), gameObject);
            }
        }
        else
        {
            GameObject bulletFiring = Instantiate(bullet, currentGun.GetChild(0).position, currentGun.rotation);
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
                    playerDir = transform.position;
                }
            }
        }
        return playerDir + (velocity/8);
    }
    public void setVelocity(Vector3 newVelocity)
    {
        velocity = newVelocity;
    }

    public void IncreasePlayerScore()
    {
        playerScore += 1;
        PlayerScoreTextbox.text = playerScore.ToString();
    }

    public IEnumerator Respawn()
    {
        DeathCam.GetComponent<DeathCamera>().UseDeathCam();
        gameObject.transform.position = new Vector3(11111, 11111, 1111);
        AI.GetComponent<BaseAIScript>().IncreaseAIScore();
        yield return new WaitForSeconds(2f);
        DeathCam.GetComponent<DeathCamera>().DisableDeathCam();
        Spawn();
    }

    private void Spawn()
    {
        isStealth = true;
        int currentNumber = Random.Range(0, SpawnPoints.Length);
        while (Vector3.Distance(SpawnPoints[currentNumber].position, AI.transform.position) < 60)
        {
            currentNumber = Random.Range(0, SpawnPoints.Length);
        }
        Vector3 spawn = SpawnPoints[currentNumber].position;
        Quaternion spawnRot = SpawnPoints[currentNumber].rotation;
        spawn.y += 1.7f;
        transform.position = spawn;
        transform.rotation = spawnRot;
        takeHeal(100f);
        rotationY = 0f;
    }

}
