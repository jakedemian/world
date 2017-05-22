using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private Rigidbody2D rb;
    private bool grounded = false;
    private CollisionDirections collisions = new CollisionDirections();
    private Bounds playerBounds;
    private Vector2 playerDimensions;

    private const float MAX_PLAYER_FALL_SPEED = -15f;
    private const float PLAYER_MOVE_SPEED_FACTOR = 8f;
    private const int LAYER_TERRAIN = 1 << 9;
    private const float GRAVITY_VELOCITY = -40f;
    private const float JUMP_FORCE = 16f;
    
    
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
            if(Input.GetAxisRaw("Horizontal") > 0) {
                Vector2 moveDir = Vector2.right;
                transform.Translate(moveDir * PLAYER_MOVE_SPEED_FACTOR * Time.deltaTime);
            } else if(Input.GetAxisRaw("Horizontal") < 0) {
                transform.Translate(-Vector2.right * PLAYER_MOVE_SPEED_FACTOR * Time.deltaTime);
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
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / 1.5f);
            }
        }
    }

    void capPlayerFallSpeed() {
        if(rb.velocity.y < MAX_PLAYER_FALL_SPEED) {
            rb.velocity = new Vector2(rb.velocity.x, MAX_PLAYER_FALL_SPEED);
        }
    }

    void applyGravity() {
        if(!grounded) {
            rb.AddForce(new Vector2(0f, GRAVITY_VELOCITY));
        }
    }

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
    /// Update the player's collision states in all directions
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
        RaycastHit2D hit = spawnRaycasts(bottomLeft, bottomRight, Vector2.down);
        if(hit) {
            collisions.down = true;
            grounded = true;

            if(rb.velocity.y < 0) {
                Debug.Log("down");
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                transform.position = new Vector2(transform.position.x, hit.collider.gameObject.transform.position.y + 1f);
            }
        }

        ////////////////////////////////
        // up
        hit = spawnRaycasts(topLeft, topRight, Vector2.up);
        if(hit) {
            collisions.up = true;

            if(rb.velocity.y > 0) {
                Debug.Log("up");
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                transform.position = new Vector2(transform.position.x, hit.collider.gameObject.transform.position.y - 1f);
            }
        }

        ////////////////////////////////
        // right
        hit = spawnRaycasts(bottomRight, topRight, Vector2.right);
        if(hit) {
            Debug.Log("right?");
            collisions.right = true;

            if(rb.velocity.x > 0) {
                Debug.Log("right");
                rb.velocity = new Vector2(0f, rb.velocity.y);
                transform.position = new Vector2(hit.collider.gameObject.transform.position.x - 1f, transform.position.y);
            }
        }

        ////////////////////////////////
        // left
        hit = spawnRaycasts(bottomLeft, topLeft, Vector2.left);
        if(hit) {
            collisions.left = true;

            if(rb.velocity.x < 0) {
                Debug.Log("left");
                rb.velocity = new Vector2(0f, rb.velocity.y);
                transform.position = new Vector2(hit.collider.gameObject.transform.position.x + 1f, transform.position.y);
            }
        }
    }
}
