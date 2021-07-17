using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    enum PlayerMoveState
    {
        Slow,
        Medium,
        Fast
    }

    PlayerMoveState playerMoveState = PlayerMoveState.Medium;

    float playerHeight = 2f;

    [Header("Movement")]
    public float slowWalkSpeed = 2f;
    public float walkSpeed = 4.5f;
    public float runSpeed = 8f;
    public float movementMultiplier = 15f;
    public float airMultiplier = 0.1f;

    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;

    [Header("Jumping")]
    public float jumpForce = 8f;

    [Header("Crouch")]
    public float crouchCamSpeed;
    public float standHeight;
    public float crouchHeight;
    public GameObject standMesh;
    public GameObject crouchMesh;
    public GameObject crouchDetector;
    public float crouchSpeedMultiplier;
    public float unDuckedHeight;

    [Header("Drag")]
    [SerializeField] float groundDrag = 12f;
    [SerializeField] float airDrag = 1f;

    [Header("Lean")]
    [SerializeField] float leanAmount = 0.4f;

    [Header("Climb")]
    [SerializeField] float climbTime = 0.5f;
    //[SerializeField] float ledgeDetectDistance = 6.5f;
    [SerializeField] GameObject ledgeOverTopOriginObject;
    //[SerializeField] GameObject ledgeTestCapsult;
    [SerializeField] AnimationCurve climbYCurve;
    [SerializeField] AnimationCurve climbXZCurve;

    float horizontalMovement;
    float verticalMovement;

    bool isGrounded;
    bool isClimbing = false;
    Vector3 climbingStartPosition;
    Vector3 climbingEndPosition;
    //float climbingTime = 0.5f;
    float climbingCountdown;

    //sort this out
    bool isCrouching;
    Camera cam;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        cam = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.1f);

        if (isClimbing)
        {
            rb.isKinematic = true;

            climbingCountdown += Time.deltaTime;

            float timeFactor = 1 / climbTime; //like const

            float normalizedTime = climbingCountdown * timeFactor;


            //var yAnim = climbYCurve.Evaluate(climbingCountdown);
            //var xzAnim = climbXZCurve.Evaluate(climbingCountdown);
            var yAnim = climbYCurve.Evaluate(normalizedTime);
            var xzAnim = climbXZCurve.Evaluate(normalizedTime);

            var targetX = climbingStartPosition.x + (xzAnim * (climbingEndPosition.x - climbingStartPosition.x));
            var targetY = climbingStartPosition.y + (yAnim * (climbingEndPosition.y - climbingStartPosition.y));
            var targetZ = climbingStartPosition.z + (xzAnim * (climbingEndPosition.z - climbingStartPosition.z));

            var targetTransform = new Vector3(targetX, targetY, targetZ);

            this.transform.position = targetTransform;

            //if (climbingCountdown >= climbTime)
            if (normalizedTime >= 1)
            {
                isClimbing = false;
                rb.isKinematic = false;
            }
            return;
        }


        MyInput();
        ControlDrag();


        //MOVE STATE
        playerMoveState = PlayerMoveState.Medium;
        if (Input.GetKey(KeyCode.LeftControl))
        {
            playerMoveState = PlayerMoveState.Slow;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            playerMoveState = PlayerMoveState.Fast;
        }



        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }






        bool somethingAbove = false;
        float distanceToOvearhead = 100.0f;
        RaycastHit hit;
        Vector3 origin = new Vector3(crouchMesh.transform.position.x,
            crouchMesh.transform.position.y + 0.02f, crouchMesh.transform.position.z);
        Vector3 origin2 = new Vector3(origin.x, origin.y + 1f, origin.z);
        //Vector3 direction = transform.TransformDirection(Vector3.up);
        //6 is player 
        if (Physics.SphereCast(origin, 0.46f, transform.up, out hit, 1.02f))
        {
            somethingAbove = true;
        }

        // I CAN JUST USE THIS SMALL BALL AND CHECK THAT ITS EITHER FALSE 
        // OR DISTANCE IS ABOVE THE HEIGHT TO SET SOMETHINGABOVE TO FALSE 

        if (somethingAbove)
        {
            RaycastHit hitDuck;
            if (Physics.SphereCast(origin, 0.2f, transform.up, out hitDuck, 3f))
            {
                distanceToOvearhead = hitDuck.distance - 0.5f;
            }
        }


        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isCrouching)
            {
                if (!somethingAbove)
                {
                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        isCrouching = !isCrouching;
                    }
                }
            }
            else
            {
                isCrouching = !isCrouching;
            }
        }



        //TEST LEAN
        var targetCamLeanRight = Input.GetKey(KeyCode.E) ? leanAmount : 0;

        if (targetCamLeanRight <= 0)
        {
            targetCamLeanRight = Input.GetKey(KeyCode.Q) ? -leanAmount : 0;
        }



        if (isCrouching)
        {
            crouchMesh.GetComponent<SphereCollider>().enabled = true;
            standMesh.GetComponent<CapsuleCollider>().enabled = false;
        }
        else
        {
            crouchMesh.GetComponent<SphereCollider>().enabled = false;
            standMesh.GetComponent<CapsuleCollider>().enabled = true;
        }



        //distanceToOvearhead

        var targetCameraHieght = isCrouching ? crouchHeight : standHeight;


        distanceToOvearhead = Mathf.Clamp(distanceToOvearhead, crouchHeight, crouchHeight + unDuckedHeight);
        

        if (isCrouching)
            targetCameraHieght = distanceToOvearhead;

        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition,
            new Vector3(targetCamLeanRight, targetCameraHieght, 0), Time.deltaTime * crouchCamSpeed);




        //we can climb! or are climbing?
        //leter we will do player states and state switching with cooldowns etc...
        {
            RaycastHit hitOver;

            if (Physics.Raycast(
                ledgeOverTopOriginObject.transform.position,
                -transform.up, out hitOver, 1.5f))
            {
                //now check to see if the space is viable!
                var p1 = new Vector3(hitOver.point.x, hitOver.point.y + 0.7f, hitOver.point.z);
                var p2 = new Vector3(hitOver.point.x, hitOver.point.y + 1.7f, hitOver.point.z);
                RaycastHit hitCapsule;


                // Cast character controller shape 10 meters forward to see if it is about to hit anything.
                if (!Physics.CapsuleCast(p1, p2, 0.5f, -transform.up, out hitCapsule, 0.2f))
                {
                    //TODO - show climb ui symbol!!!!!!

                    if (Input.GetKey(KeyCode.Space))
                    {
                        //you are now climbing!
                        climbingCountdown = 0;
                        isClimbing = true;
                        climbingStartPosition = this.transform.position;
                        climbingEndPosition = hitOver.point;
                        climbingEndPosition.y = climbingEndPosition.y + 1;
                    }
                }
            }
        }

    }


    //
    bool canMove = true;

    private void FixedUpdate()
    {
        if (canMove)
            MovePlayer();
    }

    private void MyInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        moveDirection = (transform.forward * verticalMovement) + (transform.right * horizontalMovement);
    }

    private void Jump()
    {
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ControlDrag()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
        }
    }

    private void MovePlayer()
    {
        float deltaMoveSpeed = 0;
        switch (playerMoveState)
        {
            case PlayerMoveState.Slow:
                deltaMoveSpeed = slowWalkSpeed;
                break;
            case PlayerMoveState.Medium:
                deltaMoveSpeed = walkSpeed;
                break;
            case PlayerMoveState.Fast:
                deltaMoveSpeed = runSpeed;
                if (isCrouching)
                    deltaMoveSpeed = walkSpeed;
                break;
        }

        if (isGrounded)
        {
            var deltaCrouchingMultiplier = isCrouching ? crouchSpeedMultiplier : 1;

            rb.AddForce((moveDirection.normalized * deltaMoveSpeed * movementMultiplier) * deltaCrouchingMultiplier, 
                ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * deltaMoveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
        }
    }

}
