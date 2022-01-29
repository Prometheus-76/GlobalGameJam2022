using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variables

    [Header("Planar Movement")]
    [Range(1f, 20f)] public float planarMoveSpeed = 10f;
    [Range(1f, 5f)] public float directionChangeCoefficient = 3f;
    [Range(1f, 100f)] public float groundAcceleration = 40f;
    [Range(1f, 30f)] public float airAcceleration = 20f;
    [Range(1f, 50f)] public float groundDragCoefficient = 20f;
    [Range(1f, 20f)] public float airDragCoefficient = 5f;

    [Header("Surface Validation")]
    [Range(2, 32)] public int collisionCheckResolution = 8;
    [Range(0.01f, 1f)] public float collisionCheckDistance = 0.1f;
    [Range(0.1f, 0.3f)] public float groundCheckCooldown = 0.1f;
    [Range(0.1f, 0.3f)] public float wallCheckCooldown = 0.1f;
    public Vector2 hitboxSize;
    public LayerMask platformLayer;
    public LayerMask obstacleLayer;
    public LayerMask gravityLayer;
    public LayerMask goalLayer;

    [Header("Gravity")]
    [Range(0f, 50f)] public float gravityAcceleration = 40f;
    [Range(0.2f, 0.8f)] public float gravityJumpModifier = 0.6f;
    [Range(-3f, 0f)] public float fallVelocityModifierThreshold = -1f;

    [Header("Jumping")]
    [Range(10, 100)] public int groundContactBufferDepth = 20;
    [Range(10, 100)] public int jumpInputBufferDepth = 30;
    [Range(10, 100)] public int wallContactBufferDepth = 40;
    [Range(1f, 5f)] public float jumpHeight = 2.2f;
    [Range(1f, 25f)] public float wallJumpForce = 5f;

    [Header("Wall Slide")]
    [Range(1f, 25f)] public float wallSlideSpeedMaximum = 4f;

    [Header("Components")]
    public LevelManager levelManager;
    public GameObject playerModel;
    private Transform playerTransform;
    private Rigidbody2D playerRigidbody;
    private EdgeCollider2D playerCollider;

    private InputMaster controls;

    // Internal
    private bool isGrounded;
    private bool lastGroundedState;
    private float groundCheckTimer;
    private bool isFacingWall;
    private bool lastWallContactState;
    private int lastDirectionFacing;
    private int lastWallDirection;
    private float wallCheckTimer;
    private Vector3 currentVelocity;
    private Vector3 newVelocity;
    private Vector3 planarVelocity;
    private Vector2 inputVector;
    private bool jumpInputQueued;
    private bool jumpInputHeld;
    private bool interactInputHeld;
    private bool canJump;
    private bool isJumping;
    private bool applyingGravity;
    private bool invertGravity;
    private bool lastInvertGravityState;
    private Vector3 planarForward;
    private Vector3 planarRight;
    private Vector3 maxCurrentVelocity;
    private float maxCurrentVelocityMagnitude;
    private Queue<bool> groundContactBuffer;
    private Queue<bool> jumpInputBuffer;
    private Queue<bool> wallContactBuffer;
    private bool inGameplay;

    #endregion

    private void Awake()
    {
        // Instantiation
        controls = new InputMaster();
        groundContactBuffer = new Queue<bool>();
        jumpInputBuffer = new Queue<bool>();
        wallContactBuffer = new Queue<bool>();

        // Component Assignment
        playerTransform = GetComponent<Transform>();
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<EdgeCollider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Variable Assignment
        isGrounded = true;
        lastGroundedState = true;
        groundCheckTimer = 0f;
        isFacingWall = false;
        lastWallContactState = true;
        lastDirectionFacing = 1;
        lastWallDirection = 1;
        wallCheckTimer = 0f;
        currentVelocity = Vector3.zero;
        newVelocity = Vector3.zero;
        planarVelocity = Vector3.zero;
        inputVector = Vector2.zero;
        jumpInputQueued = false;
        jumpInputHeld = false;
        interactInputHeld = false;
        canJump = true;
        isJumping = false;
        applyingGravity = false;
        invertGravity = false;
        lastInvertGravityState = false;
        inGameplay = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Top-down representation of movement directions based on camera orientation
        planarForward = Vector3.forward;
        planarForward.y = 0f;
        planarForward.Normalize();

        planarRight = Vector3.right;
        planarRight.y = 0f;
        planarRight.Normalize();

        #region Movement Input

        // WASD
        inputVector = (inGameplay ? controls.Player.Movement.ReadValue<Vector2>() : Vector2.zero);
        
        // Space
        jumpInputHeld = (inGameplay ? controls.Player.Jump.ReadValue<float>() != 0f : false);
        if (inGameplay && controls.Player.Jump.triggered)
        {
            // On initial press
            jumpInputQueued = true;
        }

        interactInputHeld = (inGameplay ? controls.Player.Interact.ReadValue<float>() != 0f : false);

        #endregion
    }

    void FixedUpdate()
    {
        // Read current velocity values
        CheckVelocity();
        newVelocity = currentVelocity;
        bool overrideVelocityValues = false;

        #region Collision

        #region Hitbox

        // Sets the points individually, edge collider is used to avoid jank when sliding over aligned colliders
        playerCollider.points[0].x = ((hitboxSize.x / 2f) - 0.02f) * -1f;
        playerCollider.points[0].y = 0.02f;

        playerCollider.points[1].x = ((hitboxSize.x / 2f) - 0.02f);
        playerCollider.points[1].y = 0.02f;

        playerCollider.points[2].x = ((hitboxSize.x / 2f) - 0.02f);
        playerCollider.points[2].y = hitboxSize.y - 0.02f;

        playerCollider.points[3].x = ((hitboxSize.x / 2f) - 0.02f) * -1f;
        playerCollider.points[3].y = hitboxSize.y - 0.02f;

        playerCollider.points[4].x = ((hitboxSize.x / 2f) - 0.02f) * -1f;
        playerCollider.points[4].y = 0.02f;

        #endregion

        #region Gravity Zone

        lastInvertGravityState = invertGravity;

        if (Physics2D.OverlapBox(playerTransform.position + (Vector3.up * hitboxSize.y * 0.5f), hitboxSize, 0f, gravityLayer))
        {
            Collider2D gravityZoneInstance = Physics2D.OverlapBox(playerTransform.position + (Vector3.up * hitboxSize.y * 0.5f), hitboxSize, 0f, gravityLayer);
            invertGravity = gravityZoneInstance.gameObject.GetComponent<Switchable>().activated;
        }
        else
        {
            invertGravity = false;
        }

        // Call gravity state change functions
        if (invertGravity != lastInvertGravityState)
        {
            GravityChangeEnter();
        }

        #endregion

        #region Ground Contact

        // Update the previous state
        lastGroundedState = isGrounded;

        // If we are not waiting for the check cooldown to end
        if (groundCheckTimer <= 0f)
        {
            int groundContactCount = 0; // How many points of contact are made between the player and the ground

            for (int pointNumber = 0; pointNumber <= collisionCheckResolution; pointNumber++)
            {
                Vector3 rayOrigin;
                rayOrigin.x = (playerTransform.position.x - (hitboxSize.x / 2f) + (hitboxSize.x * (pointNumber / (float)collisionCheckResolution)));
                rayOrigin.y = playerTransform.position.y + ((invertGravity ? -1f : 1f) * (hitboxSize.y / 10f)) + (invertGravity ? hitboxSize.y : 0f); // Cast from a little above the ground, or below the roof, depending on the gravity
                rayOrigin.z = playerTransform.position.z;

                // If the ground intersected with this ray
                if (Physics2D.Raycast(rayOrigin, (invertGravity ? Vector3.up : Vector3.down), collisionCheckDistance + (hitboxSize.y / 10f), platformLayer))
                {
                    groundContactCount++;
                }
            }

            // Set grounded (true/false) based on contact count and surface angle
            isGrounded = (groundContactCount >= 2);
        }
        else
        {
            // Decrement the timer
            groundCheckTimer -= Time.fixedDeltaTime;

            // Prevent timer going below 0s
            if (groundCheckTimer < 0f)
            {
                groundCheckTimer = 0f;
            }
        }

        // Call grounded state change functions
        if (isGrounded && lastGroundedState == false)
        {
            GroundContactEnter();
        }
        else if (isGrounded == false && lastGroundedState)
        {
            GroundContactExit();
        }

        #endregion

        #region Wall Contact

        // Update the previous state
        lastWallContactState = isFacingWall;

        // If we are not waiting for the check cooldown to end
        if (wallCheckTimer <= 0f)
        {
            int wallContactCount = 0; // How many points of contact are made between the player and the ground

            for (int pointNumber = 0; pointNumber <= collisionCheckResolution; pointNumber++)
            {
                Vector3 rayOrigin;
                rayOrigin.x = playerTransform.position.x + (((hitboxSize.x / 2f) - (hitboxSize.x / 10f)) * lastDirectionFacing); // Cast from a little within the ground
                rayOrigin.y = (playerTransform.position.y + (hitboxSize.y * (pointNumber / (float)collisionCheckResolution)));
                rayOrigin.z = playerTransform.position.z;

                // If the wall intersected with this ray
                if (Physics2D.Raycast(rayOrigin, (lastDirectionFacing == 1 ? Vector3.right : -Vector3.right), collisionCheckDistance + (hitboxSize.x / 10f), platformLayer))
                {
                    wallContactCount++;
                }
            }

            // Set facing wall (true/false) based on contact count
            isFacingWall = (wallContactCount >= 2);
            if (isFacingWall)
            {
                // Allow reassignment of last wall contact on side
                lastWallDirection = lastDirectionFacing;
            }
        }
        else
        {
            // Decrement the timer
            wallCheckTimer -= Time.fixedDeltaTime;

            // Prevent timer going below 0s
            if (wallCheckTimer < 0f)
            {
                wallCheckTimer = 0f;
            }
        }

        // Call grounded state change functions
        if (isFacingWall && lastWallContactState == false)
        {
            WallContactEnter();
        }
        else if (isFacingWall == false && lastWallContactState)
        {
            WallContactExit();
        }

        #endregion

        #region Hazard Contact

        // If we touch a hazard
        if (Physics2D.OverlapBox(playerTransform.position + (Vector3.up * hitboxSize.y * 0.5f), hitboxSize, 0f, obstacleLayer))
        {
            TakeDamage();
        }

        #endregion

        #region Goal Contact

        if (isGrounded && interactInputHeld && Physics2D.OverlapBox(playerTransform.position + (Vector3.up * hitboxSize.y * 0.5f), hitboxSize, 0f, goalLayer))
        {
            if (inGameplay)
            {
                inGameplay = false;
                levelManager.EndLevel();
            }
        }

        #endregion

        #endregion

        #region Planar Movement

        // Which way the player should move in world space
        Vector3 targetVector = (planarRight * inputVector.x).normalized;
        lastDirectionFacing = (inputVector.x != 0f ? Mathf.RoundToInt(inputVector.x) : lastDirectionFacing);

        // Determine how hard the player is changing direction and scale multiplier towards 0
        float directionChangeMultiplier = (((1f - Vector3.Dot(targetVector, planarVelocity.normalized)) / 2f) * (directionChangeCoefficient - 1f)) + 1f;

        // Calculate acceleration force
        Vector3 movementForce = targetVector * (isGrounded ? directionChangeMultiplier : 1f);

        // Apply different acceleration in air and on the ground
        playerRigidbody.AddForce(movementForce * (isGrounded ? groundAcceleration : airAcceleration), ForceMode2D.Force);

        CheckVelocity();

        // Applying varying deceleration forces
        if (inputVector.x == 0f)
        {
            // If the player should come to a natural stop
            Vector3 restingDrag = -planarVelocity;

            // Change drag in air and on the ground
            restingDrag *= (isGrounded ? groundDragCoefficient : airDragCoefficient);

            // Apply drag
            playerRigidbody.AddForce(restingDrag, ForceMode2D.Force);
        }
        else if (planarVelocity.magnitude > maxCurrentVelocityMagnitude)
        {
            // If the player's walk/sprint speed is currently exceeding its expected maximum
            Vector3 excessVelocityDrag = (maxCurrentVelocity - planarVelocity);

            // Apply speed restraining drag force
            playerRigidbody.AddForce(excessVelocityDrag, ForceMode2D.Impulse);
        }

        #endregion

        #region Artificial Gravity

        // Only apply gravity when the player is off the ground
        applyingGravity = (isGrounded == false);

        // Accumulate gravity acceleration from last iteration
        newVelocity.y = playerRigidbody.velocity.y;

        if (applyingGravity || invertGravity)
        {
            // Reduce gravity when moving upward and attempting a high jump
            float gravityModifier = (isJumping && jumpInputHeld && ((invertGravity ? -1f : 1f) * currentVelocity.y) > ((invertGravity ? -1f : 1f) * fallVelocityModifierThreshold)) ? gravityJumpModifier : 1f;
            gravityModifier *= (invertGravity ? -1f : 1f);
            newVelocity.y -= gravityAcceleration * Time.fixedDeltaTime * gravityModifier;
        }

        #endregion

        #region Wall Sliding

        if (isFacingWall && isGrounded == false && currentVelocity.y < 0f && invertGravity == false)
        {
            // Slide down the wall
            newVelocity.y = Mathf.Clamp(newVelocity.y, -wallSlideSpeedMaximum, Mathf.Infinity);
        }
        else if (isFacingWall && isGrounded == false && currentVelocity.y > 0f && invertGravity)
        {
            // Slide up the wall
            newVelocity.y = Mathf.Clamp(newVelocity.y, Mathf.NegativeInfinity, wallSlideSpeedMaximum);
        }

        #endregion

        #region Jump Handling

        #region Coyote Time

        //Record ground contact status
        groundContactBuffer.Enqueue(isGrounded);

        //Constrain queue to set size
        if (groundContactBuffer.Count > groundContactBufferDepth)
        {
            groundContactBuffer.Dequeue();
        }

        //If there is a ground contact in the queue and the player is waiting to jump
        if (groundContactBuffer.Contains(true) && jumpInputQueued && canJump)
        {
            Jump();
        }

        #endregion

        #region Input Buffering

        //Record record jump input status
        jumpInputBuffer.Enqueue(jumpInputQueued);

        //Constrain queue to set size
        if (jumpInputBuffer.Count > jumpInputBufferDepth)
        {
            jumpInputBuffer.Dequeue();
        }

        //If there is an input in the queue and the player has landed on the ground
        if (jumpInputBuffer.Contains(true) && isGrounded && canJump)
        {
            Jump();
        }

        #endregion

        #region Wall Contact

        //Record ground contact status
        wallContactBuffer.Enqueue(isFacingWall && isGrounded == false);

        //Constrain queue to set size
        if (wallContactBuffer.Count > wallContactBufferDepth)
        {
            wallContactBuffer.Dequeue();
        }

        //If there is a ground contact in the queue and the player is waiting to jump
        if (wallContactBuffer.Contains(true) && (jumpInputQueued || jumpInputBuffer.Contains(true)))
        {
            Jump();

            // Add outwards force from the wall
            playerRigidbody.AddForce(-lastWallDirection * playerTransform.right * wallJumpForce, ForceMode2D.Impulse);

            // Turn player to face direction of their wall jump (only relevant if no directional input is applied)
            lastDirectionFacing *= -1;
        }

        #endregion

        jumpInputQueued = false;

        // When the reduced gravity effect is stopped
        if (isJumping && jumpInputHeld == false)
        {
            isJumping = false;
        }

        #endregion

        CheckVelocity();
        newVelocity.x = (overrideVelocityValues == false ? currentVelocity.x : newVelocity.x);

        // Apply modified velocity
        playerRigidbody.velocity = newVelocity;
    }

    void Jump()
    {
        // Empty the input buffer
        jumpInputBuffer.Clear();
        groundContactBuffer.Clear();
        wallContactBuffer.Clear();
        jumpInputQueued = false;
        canJump = false;
        isJumping = true;

        // Perform the jump (prior velocity ignored)
        newVelocity.y = Mathf.Sqrt(2f * gravityAcceleration * (jumpHeight - (collisionCheckDistance / 2f))) * (invertGravity ? -1f : 1f);

        // Start cooldown for surface checks
        groundCheckTimer = groundCheckCooldown;
        wallCheckTimer = wallCheckCooldown;
        isGrounded = false;
        isFacingWall = false;
    }

    void TakeDamage()
    {
        if (inGameplay == false)
            return;

        inGameplay = false;

        // Death effect
        // Player visuals off
        playerModel.SetActive(false);

        // Transition out
        // Reload level
        levelManager.Invoke("ReloadLevel", 0.3f);
    }

    #region Ground Contact

    // When the player lands on the ground
    void GroundContactEnter()
    {
        canJump = true;
    }

    // When the player leaves the ground
    void GroundContactExit()
    {

    }

    #endregion

    #region Wall Contact

    // When the player touches the wall
    void WallContactEnter()
    {
        
    }

    // When the player leaves the wall
    void WallContactExit()
    {
        
    }

    #endregion

    #region Gravity Change

    // When the player changes gravity
    void GravityChangeEnter()
    {
        // Clear jump buffer
        jumpInputBuffer.Clear();
        groundContactBuffer.Clear();
        wallContactBuffer.Clear();
        jumpInputQueued = false;
    }

    #endregion

    void CheckVelocity()
    {
        currentVelocity = playerRigidbody.velocity;

        planarVelocity = currentVelocity;
        planarVelocity.y = 0f;

        // The maximum natural speed the player should be moving at this moment
        maxCurrentVelocityMagnitude = planarMoveSpeed;
        maxCurrentVelocity = planarVelocity.normalized * planarMoveSpeed;
    }

    #region Input System

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    #endregion
}
