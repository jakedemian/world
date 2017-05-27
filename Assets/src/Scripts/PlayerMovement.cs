using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    [HideInInspector]
    public bool grounded = false;

    [HideInInspector]
    public float currentFacingDirection = 1; // 1 = right, -1 = left.  cannot be 0

    [HideInInspector]
    public bool playerLocked = false;

    [HideInInspector]
    public const float PLAYER_MIN_MOVE_SPEED = 3f;

    [HideInInspector]
    public bool isOnWall = false;


    private Rigidbody2D rb;
    private PlayerSoundController soundCtrl;
    private CollisionDirections collisions = new CollisionDirections();
    private Bounds playerBounds;
    private Vector2 playerDimensions;
    private float inputLockTimer = 0f;
    private PlayerData playerData;
    private PlayerCombatController combatCtrl;

    private const float MAX_PLAYER_FALL_SPEED = -18f;
    private const float MAX_WALL_SLIDE_SPEED = -7f;

    private const float PLAYER_MOVE_SPEED = 7f;
    private const float PLAYER_SPRINT_SPEED = 10f;
    private const float PLAYER_ROLL_SPEED = 15f;
    private const float MAX_PLAYER_MOVE_SPEED = 15f;
    private const float MAX_PLAYER_MOVE_SPEED_AIR = 10f;

    private const int LAYER_TERRAIN = 1 << 9;

    private const float GRAVITY_VELOCITY = -40f;
    private const float WALL_SLIDE_GRAVITY_VELOCITY = -15f;

    private const float JUMP_FORCE = 16f;
    private const float WALL_JUMP_FORCE = 12f;
    private Vector2 WALL_JUMP_DIRECTION_WEIGHT = new Vector2(0.75f, 1.5f);

    private const float JUMP_FORCE_RELEASE_DIVIDER = 1.5f;

    private const float WALL_JUMP_DELAY_TIMER = 0.2f;
    private const float ROLL_DELAY_TIMER = 0.3f;

    private const float COLLISION_RAYCAST_DISTANCE = 0.05f;
    
	/// <summary>
    ///     START
    /// </summary>
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        playerData = GetComponent<PlayerData>();
        soundCtrl = GetComponent<PlayerSoundController>();
        combatCtrl = GetComponent<PlayerCombatController>();

        // update player dimensions
        playerBounds = GetComponent<BoxCollider2D>().bounds;
        playerDimensions = new Vector2(playerBounds.max.x - playerBounds.min.x, playerBounds.max.y - playerBounds.min.y);
	}

    /// <summary>
    ///     FIXED UPDATE
    /// </summary>
    void FixedUpdate() {
        applyGravity();

        // manage input lock timer (eg: after a wall jump or a roll)
        if(inputLockTimer > 0f) {
            inputLockTimer -= Time.deltaTime;

            if(inputLockTimer <= 0f) {
                inputLockTimer = 0f;
            }
        }
    }

    /// <summary>
    ///     UPDATE
    /// </summary>
	void Update () {
        bool wasInAir = !collisions.down;
        updateCollisions();

        if(wasInAir && grounded) {
            // player has landed, play sound
            soundCtrl.playLandSound(collisions.downCollisionObj.tag, transform);
        }

        handleUserInput();
        capPlayerSpeed();

        // update player direction, unless they are using their shield
        if(!combatCtrl.activeShield) {
            if(rb.velocity.x < 0) {
                currentFacingDirection = -1;
            } else if(rb.velocity.x > 0) {
                currentFacingDirection = 1;
            }
        }

        if(!grounded || rb.velocity.x == 0f) {
            soundCtrl.stopFootsteps();
        }
    }

    private void handleUserInput() {
        handleMoveInput();
        handleJumpInput();
        handleRollInput();
    }

    /// <summary>
    ///     Handle a move input from the user.
    /// </summary>
    private void handleMoveInput() {
        if(Input.GetAxis("Horizontal") != 0 && inputLockTimer == 0f && !playerLocked) {
            float speed = 0f;
            if(Input.GetAxis("Sprint") != 0 && playerData.stamina > 0f) {
                // sprint
                int dir = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
                speed = PLAYER_SPRINT_SPEED * dir;
                playerData.useStamina(PlayerData.PLAYER_SPRINT_STAMINA_DRAIN_RATE * Time.deltaTime);

                if(grounded) {
                    soundCtrl.startFootsteps(PlayerSoundController.FOOTSTEP_TYPE_SPRINT, collisions.downCollisionObj.tag, transform);
                }
            } else {
                // walk / jog
                speed = PLAYER_MOVE_SPEED * Input.GetAxis("Horizontal");
                if(Mathf.Abs(speed) < PLAYER_MIN_MOVE_SPEED) {
                    speed = speed > 0 ? PLAYER_MIN_MOVE_SPEED : -PLAYER_MIN_MOVE_SPEED;
                }

                if(grounded) {
                    soundCtrl.startFootsteps(PlayerSoundController.FOOTSTEP_TYPE_JOG, collisions.downCollisionObj.tag, transform);
                }
            }

            if(Input.GetAxisRaw("Horizontal") > 0 && !collisions.right 
            || Input.GetAxisRaw("Horizontal") < 0 && !collisions.left) {
                rb.velocity = new Vector2(speed, rb.velocity.y);
            } 
        } else {
            if(inputLockTimer == 0) {
                rb.velocity = new Vector2(rb.velocity.x / 1.2f, rb.velocity.y);
                if(Mathf.Abs(rb.velocity.x) < 0.1f) {
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                }
            }
        }
    }

    /// <summary>
    ///     Handle a jump input from the user.
    /// </summary>
    private void handleJumpInput() {
        if(playerData.stamina > 0f && !playerLocked && !combatCtrl.shieldIsUp) {
            if(Input.GetButtonDown("Jump") && inputLockTimer == 0f && (grounded || isOnWall)) {
                triggerJump();
                playerData.useStamina(20f);

                if(collisions.down && !isOnWall) {
                    soundCtrl.playJumpSound(collisions.downCollisionObj.tag, transform);
                } else {
                    if(collisions.right) {
                        soundCtrl.playJumpSound(collisions.rightCollisionObj.tag, transform);
                    } else {
                        soundCtrl.playJumpSound(collisions.leftCollisionObj.tag, transform);
                    }
                }
            } else if(Input.GetButtonUp("Jump")) {
                if(!grounded && rb.velocity.y > 0) {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / JUMP_FORCE_RELEASE_DIVIDER);
                }
            }
        }
    }

    /// <summary>
    ///     Perform a regular jump or wall jump.
    /// </summary>
    private void triggerJump() {
        if(grounded) {
            grounded = false;
            rb.AddForce(Vector2.up * JUMP_FORCE, ForceMode2D.Impulse);
        } else if(isOnWall) {
            if(collisions.right) {
                rb.velocity = new Vector2(-WALL_JUMP_DIRECTION_WEIGHT.x, WALL_JUMP_DIRECTION_WEIGHT.y) * WALL_JUMP_FORCE;
                inputLockTimer = WALL_JUMP_DELAY_TIMER;
            } else if(collisions.left) {
                rb.velocity = WALL_JUMP_DIRECTION_WEIGHT * WALL_JUMP_FORCE;
                inputLockTimer = WALL_JUMP_DELAY_TIMER;
            }
        }
    }

    /// <summary>
    ///     Handle the player pressing the roll button.
    /// </summary>
    private void handleRollInput() {
        if(Input.GetButtonDown("Roll") && grounded && inputLockTimer == 0f && playerData.stamina > 0f && !playerLocked) {
            inputLockTimer = ROLL_DELAY_TIMER;
            rb.velocity = new Vector2(currentFacingDirection * PLAYER_ROLL_SPEED, rb.velocity.y);
            playerData.useStamina(20f);
            soundCtrl.stopFootsteps();
        }
    }

    /// <summary>
    ///     Max out the player's speed
    /// </summary>
    private void capPlayerSpeed() {
        // fall speed
        float maxVertSpeed = isOnWall ? MAX_WALL_SLIDE_SPEED : MAX_PLAYER_FALL_SPEED;
        if(rb.velocity.y < maxVertSpeed) {
            rb.velocity = new Vector2(rb.velocity.x, maxVertSpeed);
        }

        // move speed
        float maxMoveSpeed = grounded ? MAX_PLAYER_MOVE_SPEED : MAX_PLAYER_MOVE_SPEED_AIR;
        if(grounded) {
            if(rb.velocity.x > maxMoveSpeed) {
                rb.velocity = new Vector2(maxMoveSpeed, rb.velocity.y);
            } else if(rb.velocity.x < -maxMoveSpeed) {
                rb.velocity = new Vector2(-maxMoveSpeed, rb.velocity.y);
            }
        }

        if(collisions.down && rb.velocity.y < 0f) {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }

    /// <summary>
    ///     Apply the gravity force to the player for this frame, if necessary and depending on current player state.
    /// </summary>
    private void applyGravity() {
        if(!grounded) {
            if(isOnWall && rb.velocity.y < 0) {                
                rb.AddForce(new Vector2(0f, WALL_SLIDE_GRAVITY_VELOCITY));
            } else {
                rb.AddForce(new Vector2(0f, GRAVITY_VELOCITY));
            }
        }
    }

    /// <summary>
    ///     Spawn two raycasts in a direction from two different points.
    /// </summary>
    /// <param name="startPoint1">The start point of the first raycast</param>
    /// <param name="startPoint2">The start point of the second raycast</param>
    /// <param name="direction">The direction both raycasts will cast</param>
    /// <returns>The first raycast if there is a hit, the second if the first has no hit</returns>
    private RaycastHit2D spawnRaycasts(Vector2 startPoint1, Vector2 startPoint2, Vector2 direction) {
        Debug.DrawRay(startPoint1, direction * COLLISION_RAYCAST_DISTANCE, Color.green);
        Debug.DrawRay(startPoint2, direction * COLLISION_RAYCAST_DISTANCE, Color.green);

        RaycastHit2D hit = Physics2D.Raycast(startPoint1, direction, COLLISION_RAYCAST_DISTANCE, LAYER_TERRAIN);
        if(hit) {
            return hit;
        } else {
            return Physics2D.Raycast(startPoint2, direction, COLLISION_RAYCAST_DISTANCE, LAYER_TERRAIN);
        }
    }

    /// <summary>
    ///     Prevent the player from phasing through the ground this frame
    /// </summary>
    /// <param name="downHit">The downward RaycastHit2D object</param>
    private void preventPlayerPhaseThroughGround(RaycastHit2D downHit) {
        BoxCollider2D otherCollider = (BoxCollider2D)downHit.collider;
        float otherColliderVerticalRadius = otherCollider.size.y / 2f;
        float collisionPointToColliderCenterVerticalDistance = Mathf.Abs(downHit.point.y - downHit.collider.gameObject.transform.position.y);
        float collisionPointToColliderCenterHorizontalDistance = Mathf.Abs(downHit.point.x - downHit.collider.gameObject.transform.position.x);
        float arbitraryThresholdForVerticalPositionFix = -10f;

        if(collisionPointToColliderCenterVerticalDistance <= 0.51f
            && collisionPointToColliderCenterHorizontalDistance <= 0.51f
            && rb.velocity.y <= arbitraryThresholdForVerticalPositionFix) {
            transform.position = new Vector2(transform.position.x, downHit.collider.gameObject.transform.position.y + 1f);
        }
    }

    private bool isBothDownRaycastsColliding(Vector2 startPoint1, Vector2 startPoint2) {
        bool res = false;

        RaycastHit2D hit1 = Physics2D.Raycast(startPoint1, Vector2.down, COLLISION_RAYCAST_DISTANCE, LAYER_TERRAIN);
        RaycastHit2D hit2 = Physics2D.Raycast(startPoint2, Vector2.down, COLLISION_RAYCAST_DISTANCE, LAYER_TERRAIN);
        
        if(hit1 && hit2) {
            res = true;
        }

        return res;
    }

    /// <summary>
    ///     Update the player's collision states in all directions
    /// </summary>
    private void updateCollisions() {
        playerBounds = GetComponent<BoxCollider2D>().bounds;
        grounded = false;
        isOnWall = false;

        Vector2 bottomLeft = new Vector2(playerBounds.min.x + 0.001f, playerBounds.min.y + 0.001f);
        Vector2 bottomRight = new Vector2(playerBounds.max.x - 0.001f, playerBounds.min.y + 0.001f);
        Vector2 topLeft = new Vector2(playerBounds.min.x + 0.001f, playerBounds.max.y - 0.001f);
        Vector2 topRight = new Vector2(playerBounds.max.x - 0.001f, playerBounds.max.y - 0.001f);

        collisions.clear();

        // up
        RaycastHit2D upHit = spawnRaycasts(topLeft, topRight, Vector2.up);
        collisions.up = upHit;
        if(collisions.up) {
            collisions.upCollisionObj = upHit.collider.gameObject;
        }

        // right
        RaycastHit2D rightHit = spawnRaycasts(bottomRight, topRight, Vector2.right);
        collisions.right = rightHit;
        if(collisions.right) {
            collisions.rightCollisionObj = rightHit.collider.gameObject;
        }

        // left
        RaycastHit2D leftHit = spawnRaycasts(bottomLeft, topLeft, Vector2.left);
        collisions.left = leftHit;
        if(collisions.left) {
            collisions.leftCollisionObj = leftHit.collider.gameObject;
        }

        // down
        RaycastHit2D downHit = spawnRaycasts(bottomLeft, bottomRight, Vector2.down);
        collisions.down = downHit;
        if(collisions.down) {
            collisions.downCollisionObj = downHit.collider.gameObject;
            preventPlayerPhaseThroughGround(downHit);            
        }

        if((collisions.right || collisions.left) && collisions.down) {
            // the down collision must have BOTH raycasts colliding in order to consider the player grounded
            grounded = isBothDownRaycastsColliding(bottomLeft, bottomRight);
        } else if(collisions.down && rb.velocity.y <= 0f) {
            grounded = true;
        }

        // update on wall state
        if(!grounded) {
            // we check collisions.up here to prevent weird glitches when player hits a ceiling
            if(collisions.right && !collisions.up && rb.velocity.x >= 0f) {
                isOnWall = true;
            } else if(collisions.left && !collisions.up && rb.velocity.x <= 0f) {
                isOnWall = true;
            }
        }
    }
    
}
