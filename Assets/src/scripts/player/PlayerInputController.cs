using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputController : MonoBehaviour {

    [HideInInspector]
    public float currentFacingDirection = 1; // 1 = right, -1 = left.  cannot be 0

    [HideInInspector]
    public bool playerLocked = false;

    [HideInInspector]
    public const float PLAYER_MIN_MOVE_SPEED = 3f;    

    [HideInInspector]
    public Rigidbody2D rb;

    private PlayerSoundController soundCtrl;   
    private float inputLockTimer = 0f;
    private PlayerData playerData;
    private PlayerCombatController combatCtrl;
    private StandardPhysicsController physicsCtrl;

    private const float PLAYER_MOVE_SPEED = 7f;
    private const float PLAYER_SPRINT_SPEED = 10f;
    private const float PLAYER_ROLL_SPEED = 15f;
    private const float JUMP_FORCE = 16f;
    private const float WALL_JUMP_FORCE = 12f;
    private Vector2 WALL_JUMP_DIRECTION_WEIGHT = new Vector2(0.85f, 1.5f);
    private const float JUMP_FORCE_RELEASE_DIVIDER = 1.5f;
    private const float WALL_JUMP_DELAY_TIMER = 0.2f;
    private const float ROLL_DELAY_TIMER = 0.3f;
    private const float DAMAGE_TAKEN_INPUT_LOCK_TIMER = 0.4f;
    
	/// <summary>
    ///     START
    /// </summary>
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        playerData = GetComponent<PlayerData>();
        soundCtrl = GetComponent<PlayerSoundController>();
        combatCtrl = GetComponent<PlayerCombatController>();
        physicsCtrl = GetComponent<StandardPhysicsController>();
	}

    /// <summary>
    ///     FIXED UPDATE
    /// </summary>
    void FixedUpdate() {
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
        GameObject enemyCollision = hasCollisionWithEnemy();
        if(enemyCollision != null) {
            handleCollisionWithEnemy(enemyCollision);
        }

        handleMoveInput();
        handleJumpInput();
        handleRollInput();

        // update player direction, unless they are using their shield
        if(!combatCtrl.activeShield) {
            if(rb.velocity.x < 0) {
                currentFacingDirection = -1;
            } else if(rb.velocity.x > 0) {
                currentFacingDirection = 1;
            }
        }

        if(!physicsCtrl.grounded || rb.velocity.x == 0f) {
            soundCtrl.stopFootsteps();
        }
    }

    /// <summary>
    ///     Handle what happens when you collide with an enemy.
    ///     // TODO FIXME might make sense to make this a more general "takeDamage" function.
    /// </summary>
    /// <param name="enemyCollision">The collision point of the damage source.</param>
    private void handleCollisionWithEnemy(GameObject enemyCollision) {
        //knock the player back, deal damage to the player, if the damage timer is zero
        float xPosDiff = transform.position.x - enemyCollision.transform.position.x;
        float yPosDiff = transform.position.y - enemyCollision.transform.position.y;

        // gaurentees an x speed of 8, to help them get away from the enemy
        float xSpeed = ((xPosDiff) / Mathf.Abs(xPosDiff)) * 8f;
        float ySpeed = yPosDiff * 5f;
        rb.velocity = new Vector2(xSpeed, ySpeed);

        // lock the player's input
        if(inputLockTimer == 0f) {
            inputLockTimer = DAMAGE_TAKEN_INPUT_LOCK_TIMER;
            playerData.health--;
        }
    }

    /// <summary>
    ///     Determine if the player has a collision with the enemy.
    /// </summary>
    /// <returns></returns>
    private GameObject hasCollisionWithEnemy() {
        GameObject res = null;

        if(physicsCtrl.collisions.leftCollisionObj != null && physicsCtrl.collisions.leftCollisionObj.tag == "Enemy") {
            res = physicsCtrl.collisions.leftCollisionObj;
        } else if(physicsCtrl.collisions.rightCollisionObj != null && physicsCtrl.collisions.rightCollisionObj.tag == "Enemy") {
            res = physicsCtrl.collisions.rightCollisionObj;
        } else if(physicsCtrl.collisions.upCollisionObj != null && physicsCtrl.collisions.upCollisionObj.tag == "Enemy") {
            res = physicsCtrl.collisions.upCollisionObj;
        } else if(physicsCtrl.collisions.downCollisionObj != null && physicsCtrl.collisions.downCollisionObj.tag == "Enemy") {
            res = physicsCtrl.collisions.downCollisionObj;
        }

        return res;
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

                if(physicsCtrl.grounded) {
                    soundCtrl.startFootsteps(PlayerSoundController.FOOTSTEP_TYPE_SPRINT, physicsCtrl.collisions.downCollisionObj.tag, transform);
                }
            } else {
                // walk / jog
                speed = PLAYER_MOVE_SPEED * Input.GetAxis("Horizontal");
                if(Mathf.Abs(speed) < PLAYER_MIN_MOVE_SPEED) {
                    speed = speed > 0 ? PLAYER_MIN_MOVE_SPEED : -PLAYER_MIN_MOVE_SPEED;
                }

                if(physicsCtrl.grounded) {
                    soundCtrl.startFootsteps(PlayerSoundController.FOOTSTEP_TYPE_JOG, physicsCtrl.collisions.downCollisionObj.tag, transform);
                }
            }

            if(Input.GetAxisRaw("Horizontal") > 0 && !physicsCtrl.collisions.right 
            || Input.GetAxisRaw("Horizontal") < 0 && !physicsCtrl.collisions.left) {
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
            if(Input.GetButtonDown("Jump") && inputLockTimer == 0f && (physicsCtrl.grounded || physicsCtrl.isOnWall)) {
                triggerJump();
                playerData.useStamina(20f);

                if(physicsCtrl.collisions.down && !physicsCtrl.isOnWall) {
                    soundCtrl.playJumpSound(physicsCtrl.collisions.downCollisionObj.tag, transform);
                } else {
                    if(physicsCtrl.collisions.right) {
                        soundCtrl.playJumpSound(physicsCtrl.collisions.rightCollisionObj.tag, transform);
                    } else {
                        soundCtrl.playJumpSound(physicsCtrl.collisions.leftCollisionObj.tag, transform);
                    }
                }
            } else if(Input.GetButtonUp("Jump")) {
                if(!physicsCtrl.grounded && rb.velocity.y > 0) {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / JUMP_FORCE_RELEASE_DIVIDER);
                }
            }
        }
    }

    /// <summary>
    ///     Perform a regular jump or wall jump.
    /// </summary>
    private void triggerJump() {
        if(physicsCtrl.grounded) {
            physicsCtrl.grounded = false;
            rb.AddForce(Vector2.up * JUMP_FORCE, ForceMode2D.Impulse);
        } else if(physicsCtrl.isOnWall) {
            if(physicsCtrl.collisions.right) {
                rb.velocity = new Vector2(-WALL_JUMP_DIRECTION_WEIGHT.x, WALL_JUMP_DIRECTION_WEIGHT.y) * WALL_JUMP_FORCE;
                inputLockTimer = WALL_JUMP_DELAY_TIMER;
            } else if(physicsCtrl.collisions.left) {
                rb.velocity = WALL_JUMP_DIRECTION_WEIGHT * WALL_JUMP_FORCE;
                inputLockTimer = WALL_JUMP_DELAY_TIMER;
            }
        }
    }

    /// <summary>
    ///     Handle the player pressing the roll button.
    /// </summary>
    private void handleRollInput() {
        if(Input.GetButtonDown("Roll") && physicsCtrl.grounded && inputLockTimer == 0f && playerData.stamina > 0f && !playerLocked) {
            inputLockTimer = ROLL_DELAY_TIMER;
            rb.velocity = new Vector2(currentFacingDirection * PLAYER_ROLL_SPEED, rb.velocity.y);
            playerData.useStamina(20f);
            soundCtrl.stopFootsteps();
        }
    }

       
}
