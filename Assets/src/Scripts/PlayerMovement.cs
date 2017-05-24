﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    [HideInInspector]
    public bool grounded = false;

    [HideInInspector]
    public float currentFacingDirection = 1; // 1 = right, -1 = left.  cannot be 0

    private Rigidbody2D rb;
    private PlayerSoundController soundCtrl;
    private CollisionDirections collisions = new CollisionDirections();
    private Bounds playerBounds;
    private Vector2 playerDimensions;
    private bool isOnWall = false;
    private float inputLockTimer = 0f;
    private PlayerData playerData;

    private const float MAX_PLAYER_FALL_SPEED = -18f;
    private const float MAX_WALL_SLIDE_SPEED = -7f;

    private const float PLAYER_MOVE_SPEED = 7f;
    private const float PLAYER_MIN_MOVE_SPEED = 3f;
    private const float PLAYER_SPRINT_SPEED = 10f;
    private const float PLAYER_ROLL_SPEED = 15f;
    private const float MAX_PLAYER_MOVE_SPEED = 15f;
    private const float MAX_PLAYER_MOVE_SPEED_AIR = 10f;

    private const int LAYER_TERRAIN = 1 << 9;

    private const float GRAVITY_VELOCITY = -40f;
    private const float WALL_SLIDE_GRAVITY_VELOCITY = -15f;

    private const float JUMP_FORCE = 16f;
    private const float WALL_JUMP_FORCE = 16f;

    private const float JUMP_FORCE_RELEASE_DIVIDER = 1.5f;

    private const float WALL_JUMP_DELAY_TIMER = 0.2f;
    private const float ROLL_DELAY_TIMER = 0.3f;
    
	/// <summary>
    ///     START
    /// </summary>
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        playerData = GetComponent<PlayerData>();
        soundCtrl = GetComponent<PlayerSoundController>();

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
        updateCollisions();
        handleUserInput();
        capPlayerSpeed();

        if(rb.velocity.x < 0) {
            currentFacingDirection = -1;
        } else if(rb.velocity.x > 0) {
            currentFacingDirection = 1;
        }

        if(!grounded || rb.velocity.x == 0f) {
            soundCtrl.stopFootsteps();
        }
    }

    void handleUserInput() {
        handleMoveInput();
        handleJumpInput();
        handleRollInput();
    }

    /// <summary>
    ///     Handle a move input from the user.
    /// </summary>
    void handleMoveInput() {
        if(Input.GetAxis("Horizontal") != 0 && inputLockTimer == 0f) {
            // set the speed based on whether or not the player is sprinting
            float speed = 0f;
            if(Input.GetAxis("Sprint") != 0 && playerData.stamina > 0f) {
                // sprint
                int dir = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
                speed = PLAYER_SPRINT_SPEED * dir;
                playerData.useStamina(12f * Time.deltaTime);

                if(grounded) {
                    soundCtrl.startFootsteps(PlayerSoundController.FOOTSTEP_TYPE_SPRINT, collisions.downCollisionObj.tag, transform);
                }
            } else {
                // walk / jog
                speed = PLAYER_MOVE_SPEED * Input.GetAxis("Horizontal");

                if(grounded) {
                    soundCtrl.startFootsteps(PlayerSoundController.FOOTSTEP_TYPE_JOG, collisions.downCollisionObj.tag, transform);
                }

                // if not sprinting, we don't want the player to go TOO slow, so limit that
                if(Mathf.Abs(speed) < PLAYER_MIN_MOVE_SPEED) {
                    speed = speed > 0 ? PLAYER_MIN_MOVE_SPEED : -PLAYER_MIN_MOVE_SPEED;
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
    void handleJumpInput() {
        if(playerData.stamina > 0f) {
            if(Input.GetButtonDown("Jump") && inputLockTimer == 0f) {
                if(grounded) {
                    grounded = false;
                    rb.AddForce(Vector2.up * JUMP_FORCE, ForceMode2D.Impulse);
                    soundCtrl.playJumpSound(collisions.downCollisionObj.tag, transform);
                    playerData.useStamina(20f);
                } else if(isOnWall) {
                    if(collisions.right) {
                        Vector2 upLeft = new Vector2(-1f, 1f) * WALL_JUMP_FORCE;
                        rb.velocity = upLeft;
                        inputLockTimer = WALL_JUMP_DELAY_TIMER;
                        soundCtrl.playJumpSound(collisions.rightCollisionObj.tag, transform);
                    } else if(collisions.left) {
                        Vector2 upRight = new Vector2(1f, 1f) * WALL_JUMP_FORCE;
                        rb.velocity = upRight;
                        inputLockTimer = WALL_JUMP_DELAY_TIMER;
                        soundCtrl.playJumpSound(collisions.leftCollisionObj.tag, transform);
                    }
                    playerData.useStamina(20f);
                }
            } else if(Input.GetButtonUp("Jump")) {
                if(!grounded && rb.velocity.y > 0) {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / JUMP_FORCE_RELEASE_DIVIDER);
                }
            }
        }
    }

    void handleRollInput() {
        if(Input.GetButtonDown("Roll") && grounded && inputLockTimer == 0f && playerData.stamina > 0f) {
            inputLockTimer = ROLL_DELAY_TIMER;
            rb.velocity = new Vector2(currentFacingDirection * PLAYER_ROLL_SPEED, rb.velocity.y);
            playerData.useStamina(20f);
            soundCtrl.stopFootsteps();
        }
    }

    /// <summary>
    ///     Max out the player's speed
    /// </summary>
    void capPlayerSpeed() {
        // fall speed
        if(isOnWall) {
            if(rb.velocity.y < MAX_WALL_SLIDE_SPEED) {
                rb.velocity = new Vector2(rb.velocity.x, MAX_WALL_SLIDE_SPEED);
            }
        } else {
            if(rb.velocity.y < MAX_PLAYER_FALL_SPEED) {
                rb.velocity = new Vector2(rb.velocity.x, MAX_PLAYER_FALL_SPEED);
            }
        }

        // grounded move speed
        if(grounded) {
            if(rb.velocity.x > MAX_PLAYER_MOVE_SPEED) {
                rb.velocity = new Vector2(MAX_PLAYER_MOVE_SPEED, rb.velocity.y);
            } else if(rb.velocity.x < -MAX_PLAYER_MOVE_SPEED) {
                rb.velocity = new Vector2(-MAX_PLAYER_MOVE_SPEED, rb.velocity.y);
            }
        } else {
            if(rb.velocity.x > MAX_PLAYER_MOVE_SPEED_AIR) {
                rb.velocity = new Vector2(MAX_PLAYER_MOVE_SPEED_AIR, rb.velocity.y);
            } else if(rb.velocity.x < -MAX_PLAYER_MOVE_SPEED_AIR) {
                rb.velocity = new Vector2(-MAX_PLAYER_MOVE_SPEED_AIR, rb.velocity.y);
            }
        }

        if(collisions.down && rb.velocity.y < 0f) {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }

    /// <summary>
    ///     Apply the gravity force to the player for this frame, if necessary and depending on current player state.
    /// </summary>
    void applyGravity() {
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
    /// <param name="startPoint1"></param>
    /// <param name="startPoint2"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    private RaycastHit2D spawnRaycasts(Vector2 startPoint1, Vector2 startPoint2, Vector2 direction) {
        Debug.DrawRay(startPoint1, direction * 0.1f, Color.green);
        Debug.DrawRay(startPoint2, direction * 0.1f, Color.green);

        RaycastHit2D hit = Physics2D.Raycast(startPoint1, direction, 0.1f, LAYER_TERRAIN);
        if(hit) {
            return hit;
        } else {
            return Physics2D.Raycast(startPoint2, direction, 0.1f, LAYER_TERRAIN);
        }
    }

    /// <summary>
    ///     Update the player's collision states in all directions
    /// </summary>
    private void updateCollisions() {
        playerBounds = GetComponent<BoxCollider2D>().bounds;

        Vector2 bottomLeft = new Vector2(playerBounds.min.x + 0.001f, playerBounds.min.y + 0.001f);
        Vector2 bottomRight = new Vector2(playerBounds.max.x - 0.001f, playerBounds.min.y + 0.001f);
        Vector2 topLeft = new Vector2(playerBounds.min.x + 0.001f, playerBounds.max.y - 0.001f);
        Vector2 topRight = new Vector2(playerBounds.max.x - 0.001f, playerBounds.max.y - 0.001f);

        collisions.clear();

        ////////////////////////////////
        // down
        grounded = false;
        RaycastHit2D downHit = spawnRaycasts(bottomLeft, bottomRight, Vector2.down);
        collisions.down = downHit;
        if(downHit) {
            if(rb.velocity.y <= 0f) {
                grounded = true;
            }

            collisions.downCollisionObj = downHit.collider.gameObject;

            // prevent the player from phasing through the ground this frame by only doing this if the collision point and the current transform's 
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

        // update on wall state
        isOnWall = !grounded && (collisions.left || collisions.right);
    }
    
}
