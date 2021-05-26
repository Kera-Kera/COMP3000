using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CharacterScript : MonoBehaviour
{
    [SerializeField]
    private CharacterController controller;
    [SerializeField]
    private float speed = 12f;
    [SerializeField]
    private float cameraSensitivity = 100f;
    [SerializeField]
    private float gravity = -9f;


    [SerializeField]
    private bool isGrounded;
    [SerializeField]
    private float groundDistance = 0.4f;
    [SerializeField]
    private LayerMask groundMask;

    private Vector3 velocity;

    private float rotationY = 0F;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(transform.GetChild(2).position, groundDistance, groundMask);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        FPSCamera();
        PlayerMovement();
        Gravity();
    }

    void FPSCamera()
    {
        transform.Rotate(0, Input.GetAxis("Mouse X") * cameraSensitivity, 0);

        rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity;
        rotationY = Mathf.Clamp(rotationY, -80f, 85f);

        transform.GetChild(1).transform.localEulerAngles = new Vector3(-rotationY, transform.GetChild(1).transform.localEulerAngles.y, 0);
    }
       
    
    void PlayerMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(3f * -2f * gravity);
        }
    }

    void Gravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
