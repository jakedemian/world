using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private Rigidbody2D rb;
    private bool grounded = false;
    private CollisionDirections collisions = new CollisionDirections();
    private Bounds playerBounds;
    private Vector2 playerDimensions;
    private bool isOnWall = false;

    private const float MAX_PLAYER_FALL_SPEED = -18f;
    private const float PLAYER_MOVE_SPEED_FACTOR = 8f;
    private const int LAYER_TERRAIN = 1 << 9;
    private const float GRAVITY_VELOCITY = -40f;
    private const float JUMP_FORCE = 16f;
    private const float JUMP_FORCE_RELEASE_DIVIDER = 1.5f;
    
    
	void Start () {
        rb = GetComponent<Rigidbody2D>();

        // update player dimensions
        playerBounds = GetComponent<BoxCollider2D>().bounds;
        playerDimensions = new Vector2(playerBounds.max.x - playerBounds.min.x, playerBounds.max.y - playerBounds.min.y);
	}

    void FixedUpdate() {
        applyGravity();
    }

	void Update () {
        updateCollisions();

        handleMoveInput();

        handleJumpInput();

        capPlayerFallSpeed();
    }

    void handleMoveInput() {
        if(Input.GetAxisRaw("Horizontal") != 0) {
            if(Input.GetAxisRaw("Horizontal") > 0 && !collisions.right) {
                transform.Translate(Vector2.right * PLAYER_MOVE_SPEED_FACTOR * Time.deltaTime);
            } else if(Input.GetAxisRaw("Horizontal") < 0 && !collisions.left) {
                transform.Translate(Vector2.left * PLAYER_MOVE_SPEED_FACTOR * Time.deltaTime);
            }
        }
    }

    void handleJumpInput() {
        if(Input.GetButtonDown("Jump")) {
            if(grounded) {
                grounded = false;
                rb.AddForce(Vector2.up * JUMP_FORCE, ForceMode2D.Impulse);
            }
        } else if(Input.GetButtonUp("Jump")) {
            if(!grounded && rb.velocity.y > 0) {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / JUMP_FORCE_RELEASE_DIVIDER);
            }
        }
    }

    void capPlayerFallSpeed() {
        if(rb.velocity.y < MAX_PLAYER_FALL_SPEED) {
            rb.velocity = new Vector2(rb.velocity.x, MAX_PLAYER_FALL_SPEED);
        }

        if(collisions.down && rb.velocity.y < 0f) {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }

    void applyGravity() {
        if(!grounded) {
            rb.AddForce(new Vector2(0f, GRAVITY_VELOCITY));
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

            // prevent the player from phasing through the ground this frame by only doing this if the collision point and the current transform's 
            BoxCollider2D otherCollider = (BoxCollider2D)downHit.collider;
            float otherColliderVerticalRadius = otherCollider.size.y / 2f;
            float collisionPointToColliderCenterVerticalDistance = Mathf.Abs(downHit.point.y - downHit.collider.gameObject.transform.position.y);
            if(collisionPointToColliderCenterVerticalDistance <= 0.51f) {
                transform.position = new Vector2(transform.position.x, downHit.collider.gameObject.transform.position.y + 1f);
            }
        }

        // update up, right, left collision states
        collisions.up = spawnRaycasts(topLeft, topRight, Vector2.up);
        collisions.right = spawnRaycasts(bottomRight, topRight, Vector2.right);
        collisions.left = spawnRaycasts(bottomLeft, topLeft, Vector2.left);

        // update on wall state
        isOnWall = !grounded && (collisions.left || collisions.right);
    }
}
