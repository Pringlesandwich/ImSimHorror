using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerInputHandler _inputHandler;

    Camera cam;
    Rigidbody rb;
    Vector3 moveDirection;
    float playerHeight = 2f;

    [Header("Look")]
    [SerializeField] float sensX = 75f;
    [SerializeField] float sensY = 75f;
    [SerializeField] float InteractionLength = 100.0f;
    float multiplier = 0.01f;
    float rotationX;
    float rotationY;

    [Header("Movement")]
    [SerializeField] float slowWalkSpeed = 1f;
    [SerializeField] float walkSpeed = 3f;
    [SerializeField] float runSpeed = 6f;
    [SerializeField] float movementMultiplier = 15f;
    [SerializeField] float airMultiplier = 0.1f;
    float horizontalMovement;
    float verticalMovement;

    [Header("Climb")]
    [SerializeField] float climbTime = 0.5f;
    [SerializeField] GameObject ledgeOverTopOriginObject;
    [SerializeField] AnimationCurve climbYCurve;
    [SerializeField] AnimationCurve climbXZCurve;
    bool isClimbing = false;
    Vector3 climbingStartPosition;
    Vector3 climbingEndPosition;
    float climbingCountdown;

    [Header("Jumping")]
    [SerializeField] float jumpForce = 7f;
    bool isGrounded;

    [Header("Crouch")]
    [SerializeField] float crouchCamSpeed = 15;
    [SerializeField] float standHeight = 0.6f;
    [SerializeField] float crouchHeight = -0.15f;
    [SerializeField] GameObject standMesh;
    [SerializeField] GameObject crouchMesh;
    [SerializeField] GameObject crouchDetector;
    [SerializeField] float crouchSpeedMultiplier = 0.8f;
    [SerializeField] float unDuckedHeight = 0.16f;
    float distanceToOvearhead = 100f;
    bool isCrouching;

    [Header("Lean")]
    [SerializeField] float leanAmount = 0.4f;

    [Header("Drag")]
    [SerializeField] float groundDrag = 12f;
    [SerializeField] float airDrag = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        cam = GetComponentInChildren<Camera>();

        isCrouching = false;
    }

    private void FixedUpdate()
    {
        ControlDrag();
        CharacterMovement();

    }

    // Update is called once per frame
    void Update()
    {
        CheckIsGrounded();

        if(!PlayerClimbing())
        {
            Climb();

            Ducking();

            Crouch();

            Jump();
        }

        CharacterRotation();
    }

    private void CharacterRotation()
    {
        //TODO - need to stop inputs somehow if climbing for example!

        var mouseX = _inputHandler.GetAxisRaw("Mouse X");
        var mouseY = _inputHandler.GetAxisRaw("Mouse Y");

        rotationY += mouseX * sensX * multiplier;
        rotationX -= mouseY * sensY * multiplier;

        rotationX = Mathf.Clamp(rotationX, -85, 85);

        cam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation = Quaternion.Euler(0, rotationY, 0);

        //lean
        var targetCamLeanRight = _inputHandler.GetLeanHeld(false) ? leanAmount : 0;
        if (targetCamLeanRight <= 0)
        {
            targetCamLeanRight = _inputHandler.GetLeanHeld(true) ? -leanAmount : 0;
        }

        //crouching

        var targetCameraHieght = isCrouching ? crouchHeight : standHeight;

        distanceToOvearhead = Mathf.Clamp(distanceToOvearhead, crouchHeight, crouchHeight + unDuckedHeight);


        if (isCrouching)
            targetCameraHieght = distanceToOvearhead;

        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition,
            new Vector3(targetCamLeanRight, targetCameraHieght, 0), Time.deltaTime * crouchCamSpeed);


    }

    private void CharacterMovement()
    {

        horizontalMovement = _inputHandler.GetAxisRaw("Horizontal");
        verticalMovement = _inputHandler.GetAxisRaw("Vertical");

        moveDirection = (transform.forward * verticalMovement) + (transform.right * horizontalMovement);

        float deltaMoveSpeed = walkSpeed;

        if (_inputHandler.GetRunHeld())
        {
            if (!isCrouching)
                deltaMoveSpeed = runSpeed;
        }
        else if (_inputHandler.GetSlowHeld())
        {
            deltaMoveSpeed = slowWalkSpeed;
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

    private void Jump()
    {
        if (isGrounded && _inputHandler.GetJumpInput())
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

    private void CheckIsGrounded()
    {
        if (isCrouching)
            isGrounded = Physics.Raycast(crouchMesh.transform.position, Vector3.down, playerHeight / 4 + 0.1f);
        else
            isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.1f);
    }

    private void Ducking()
    {
        distanceToOvearhead = 100f;
        CheckOverhead(0.2f, 3f, ref distanceToOvearhead);
    }

    private void Crouch()
    {
        if (_inputHandler.GetCrouchInput())
        {
            if (isCrouching)
            {
                if (!CheckOverhead(0.46f, 1.02f))
                {
                    isCrouching = !isCrouching;
                    SetCrouchMesh();
                }
            }
            else
            {
                isCrouching = !isCrouching;
                SetCrouchMesh();
            }
        }
    }

    private void SetCrouchMesh()
    {
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
    }

    private bool CheckOverhead(float radius, float maxDistance)
    {
        RaycastHit hit;
        Vector3 p1 = new Vector3(crouchMesh.transform.position.x, crouchMesh.transform.position.y + 0.02f, crouchMesh.transform.position.z);
        if (Physics.SphereCast(p1, 0.46f, transform.up, out hit, 1.02f))
        {
            return true;
        }
        return false;
    }

    private void CheckOverhead(float radius, float maxDistance, ref float distToOverhead)
    {
        distToOverhead = 100f;
        RaycastHit hit;
        Vector3 p1 = new Vector3(crouchMesh.transform.position.x, crouchMesh.transform.position.y + 0.02f, crouchMesh.transform.position.z);
        if (Physics.SphereCast(p1, 0.46f, transform.up, out hit, 1.02f))
        {
            distToOverhead = hit.distance - radius; //we minus to account for 
        }
    }

    private bool PlayerClimbing()
    {
        if (isClimbing)
        {
            rb.isKinematic = true;

            climbingCountdown += Time.deltaTime;

            float timeFactor = 1 / climbTime; //like const

            float normalizedTime = climbingCountdown * timeFactor;

            var yAnim = climbYCurve.Evaluate(normalizedTime);
            var xzAnim = climbXZCurve.Evaluate(normalizedTime);

            var targetX = climbingStartPosition.x + (xzAnim * (climbingEndPosition.x - climbingStartPosition.x));
            var targetY = climbingStartPosition.y + (yAnim * (climbingEndPosition.y - climbingStartPosition.y));
            var targetZ = climbingStartPosition.z + (xzAnim * (climbingEndPosition.z - climbingStartPosition.z));

            var targetTransform = new Vector3(targetX, targetY, targetZ);

            this.transform.position = targetTransform;

            if (normalizedTime >= 1)
            {
                isClimbing = false;
                rb.isKinematic = false;
            }
        }

        return isClimbing;
    }

    private void Climb()
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

                if (_inputHandler.GetClimbInput())
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
