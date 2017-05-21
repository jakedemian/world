using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private Rigidbody2D rb;
    private bool grounded = false;
    private CollisionDirections collisions = new CollisionDirections();
    private Vector2 playerDimensions;

    private const float MAX_PLAYER_FALL_SPEED = -15f;
    private const float PLAYER_MOVE_SPEED_FACTOR = 8f;
    private const int LAYER_TERRAIN = 1 << 9;
    private const float GRAVITY_VELOCITY = -40f;
    private const float JUMP_FORCE = 16f;
    
    
	void Start () {
        rb = GetComponent<Rigidbody2D>();

        // update player dimensions
        Bounds b = GetComponent<BoxCollider2D>().bounds;
        playerDimensions = new Vector2(b.max.x - b.min.x, b.max.y - b.min.y);
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
                if(collisions.right) {
                    // calculate our translate to move up the hill OR if it's too steep, stop the player's horizontal velocity and reset them
                }

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

    /// <summary>
    /// Update the player's collision states in all directions
    /// </summary>
    void updateCollisions() {
        int vertRaycasts = 7;
        int horizRaycasts = 3;

        // TODO FIXME refactor this method, up/down and right/left can probably be largely combined in some way.  the the repositioning inside the
        // if(hit) blocks, it needs to dynamically move the player ouside of the collision rather than assume a radius of 0.5 (movement of 1f).

        // down
        collisions.down = false;
        grounded = false;
        for(int i = 0; i < horizRaycasts; i++) {
            float subDivDistance = playerDimensions.x / (float)(horizRaycasts - 1);
            Vector2 rayStart = new Vector2(
                (transform.position.x - (playerDimensions.x / 2f)) + (subDivDistance * i),
                transform.position.y - (playerDimensions.y / 2f));

            RaycastHit2D hit = Physics2D.Raycast(rayStart, -Vector2.up, 0.1f, LAYER_TERRAIN);
            Debug.DrawRay(rayStart, -Vector2.up * 0.1f, Color.green);

            if(hit) {
                collisions.down = true;

                // special logic for downward collisions
                grounded = true;

                if(rb.velocity.y < 0) {
                    rb.velocity = new Vector2(rb.velocity.x, 0f);
                    transform.position = new Vector2(transform.position.x, hit.collider.gameObject.transform.position.y + 1f);
                }
            }
        }

        // up
        collisions.up = false;
        for(int i = 0; i < horizRaycasts; i++) {
            float subDivDistance = playerDimensions.x / (float)(horizRaycasts - 1);
            Vector2 rayStart = new Vector2(
                (transform.position.x - (playerDimensions.x / 2f)) + (subDivDistance * i),
                transform.position.y + (playerDimensions.y / 2f));

            RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.up, 0.1f, LAYER_TERRAIN);
            Debug.DrawRay(rayStart, Vector2.up * 0.1f, Color.green);

            if(hit) {
                collisions.up = true;
                transform.position = new Vector2(transform.position.x, hit.collider.gameObject.transform.position.y - 1f);
            }
        }

        // right
        collisions.right = false;
        for(int i = 0; i < vertRaycasts; i++) {
            float subDivDistance = playerDimensions.y / (float)(vertRaycasts - 1);
            Vector2 rayStart = new Vector2(
                (transform.position.x + (playerDimensions.x / 2f)),
                transform.position.y - (playerDimensions.y / 2f) + (subDivDistance * i) + 0.001f);

            RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.right, 0.1f, LAYER_TERRAIN);
            Debug.DrawRay(rayStart, Vector2.right * 0.1f, Color.green);

            if(hit) {
                collisions.right = true;
            }
        }

        // left
        collisions.left = false;
        for(int i = 0; i < vertRaycasts; i++) {
            float subDivDistance = playerDimensions.y / (float)(vertRaycasts - 1);
            Vector2 rayStart = new Vector2(
                (transform.position.x - (playerDimensions.x / 2f)),
                transform.position.y - (playerDimensions.y / 2f) + (subDivDistance * i) + 0.001f);

            RaycastHit2D hit = Physics2D.Raycast(rayStart, -Vector2.right, 0.1f, LAYER_TERRAIN);
            Debug.DrawRay(rayStart, -Vector2.right * 0.1f, Color.green);

            if(hit) {
                collisions.left = true;
            }
        }

    }
}
