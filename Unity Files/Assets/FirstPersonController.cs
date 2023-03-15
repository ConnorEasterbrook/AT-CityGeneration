using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    // Essential Movement Variables
    private CharacterController controller;
    private Vector3 moveDirection;

    [Header("Player Movement")]
    [Tooltip("Walking speed. 5.0f feels good for shooter-like movement.")]
    public float walkSpeed = 5.0f;
    [Tooltip("Sprinting speed. Usually 1.5x faster than walking speed for shooter-like movement.")]
    public float sprintSpeed = 7.5f;
    [Tooltip("Jump height. 10.0f feels good for arcade-like jump.")]
    private float currentSpeed; // For determining our speed in code
    [Tooltip("Smooths player movement. 0.1f feels cinematic.")]
    public float movementSmoothTime = 0.1f;
    [Tooltip("Jump height. 7.5f feels good for arcade-like jumping (10.0f gravity). 10.0 for realistic jumping (20.0f gravity)")]
    public float jumpForce = 10.0f;
    [Tooltip("Amount of gravity. 10.0f feels good for arcade-like gravity. 20.0f for realistic gravity.")]
    public float gravityForce = 20.0f;
    private float fallingVelocity = 0.0f; // Keep track of falling speed
    private float lastGroundedTime = 0.0f; // Keep track of when last grounded
    private bool jumping = false; // Keep track of player jumping
    private Vector3 velocity;
    private Vector3 currentVelocity;

    // Essential Camera Variables
    Camera playerCamera;

    [Header("Camera Movement")]
    public bool lockCursor = false;
    public float mouseSensitivity = 10.0f;

    [Header("Camera Controls")]
    [Tooltip("Length of camera movement smoothing. Lower values = sharper stops. 0.1f offers a realistic feel.")]
    [SerializeField] private float cameraRotationSmoothTime = 0.1f;
    private float cameraPan; // Looking left and right
    private float cameraPanSmooth; // For smoothing the pan movement
    private float cameraPanSmoothVelocity; // Pan smoothing speed
    private float cameraTilt; // Looking up and down
    private float cameraTiltSmooth; // For smoothing the tilt movement
    private float cameraTiltSmoothVelocity; // Tilt smoothing speed
    [Tooltip("Control the camera tilt range. X = Up. Y = Down. +-40 = A good range.")]
    [SerializeField] private Vector2 cameraTiltRange = new Vector2(-40.0f, 40.0f); // Control how far player can look (up, down)

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Assign controller variable to the Character Controller
        controller = GetComponent<CharacterController>();

        // Assign playerCamera as the main camera
        playerCamera = Camera.main;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Establish playerCamera's variable values
        cameraPan = transform.eulerAngles.y;
        cameraPanSmooth = cameraPan;
        cameraTilt = playerCamera.transform.localEulerAngles.x;
        cameraTiltSmooth = cameraTilt;

        // If we want to lock the cursor then lock and hide it
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Give currentSpeed variable a value
        currentSpeed = walkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        DefaultMovement();
        CameraMovement();
    }

    private void DefaultMovement()
    {
        // Create a new Vector2 variable that takes in our movement inputs
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Normalize the Vector2 input variable and make it a Vector3. Then transform the input to move in world space.
        Vector3 inputDirection = new Vector3(input.x, 0, input.y).normalized;
        Vector3 inputDirectionWorld = transform.TransformDirection(inputDirection);

        // If sprint key is pressed then make our current speed equal to sprintSpeed
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        // Create a new Vector3 that takes in our world movement and current speed to then use in a movement smoothing calculation
        Vector3 targetVelocity = inputDirectionWorld * currentSpeed;
        velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref currentVelocity, movementSmoothTime); // ref currentVelocity because function needs to set a currentVelocity

        // Establish falling speed. Increase as the falling duration grows
        fallingVelocity -= gravityForce * Time.deltaTime;

        // Set velocity to match the recorded movement from previous movement sections
        velocity = new Vector3(velocity.x, fallingVelocity, velocity.z);

        // Create new variable to record collision with player movement
        CollisionFlags playerCollision = controller.Move(velocity * Time.deltaTime);

        // If there is collision below the player (ground)
        if (playerCollision == CollisionFlags.Below)
        {
            jumping = false; // Set jumping to false
            lastGroundedTime = Time.time; // Set lastGroundedTime to the current time
            fallingVelocity = 0; // Stop fallingVelocity
        }

        // Check for jump input and if true, check that the character isn't jumping or falling. Then call Jump()
        if (Input.GetKey(KeyCode.Space))
        {
            float sinceLastGrounded = Time.time - lastGroundedTime;
            if (controller.isGrounded || (!jumping && lastGroundedTime < 0.15f))
            {
                Jump();
            }
        }
    }

    // Handles jump movement
    private void Jump()
    {
        jumping = true;
        fallingVelocity = jumpForce;
    }

    private void CameraMovement()
    {
        // Get mouse movement
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        // Stop camera from swinging down on game start
        float mouseMagnitude = Mathf.Sqrt(mouseX * mouseX + mouseY * mouseY);
        if (mouseMagnitude > 5)
        {
            mouseX = 0;
            mouseY = 0;
        }

        cameraPan += mouseX * mouseSensitivity; // Move camera left & right
        cameraTilt -= mouseY * mouseSensitivity; // Move camera up & down

        // Clamp the camera pitch so that the there is a limit when looking up & down
        cameraTilt = Mathf.Clamp(cameraTilt, cameraTiltRange.x, cameraTiltRange.y);

        // Smooth camera movement
        cameraTiltSmooth = Mathf.SmoothDampAngle(cameraTiltSmooth, cameraTilt, ref cameraTiltSmoothVelocity, cameraRotationSmoothTime);
        cameraPanSmooth = Mathf.SmoothDampAngle(cameraPanSmooth, cameraPan, ref cameraPanSmoothVelocity, cameraRotationSmoothTime);

        // Keep camera rotation smooth and correct, stopping camera rolling
        transform.eulerAngles = Vector3.up * cameraPanSmooth;
        playerCamera.transform.localEulerAngles = Vector3.right * cameraTiltSmooth;
    }
}