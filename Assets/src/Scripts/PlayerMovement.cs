using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private Rigidbody2D rb;
    private bool grounded = false;
    private CollisionDirections collisions = new CollisionDirections();
    private Vector2 playerDimensions;

    private const float PLAYER_MOVE_SPEED_FACTOR = 5f;
    private const int LAYER_TERRAIN = 1 << 9;
    private const float GRAVITY_VELOCITY = -20f;
    
    
	void Start () {
        rb = GetComponent<Rigidbody2D>();

        // update player dimensions
        Bounds b = GetComponent<Renderer>().bounds;
        playerDimensions = new Vector2(b.max.x - b.min.x, b.max.y - b.min.y);
	}

    void FixedUpdate() {
        applyGravity();
    }

	void Update () {
        updateCollisions();

        if(Input.GetKey(KeyCode.D)) {
            transform.Translate(Vector2.right * PLAYER_MOVE_SPEED_FACTOR  * Time.deltaTime);
        }

        if(Input.GetKey(KeyCode.A)) {
            transform.Translate(-Vector2.right * PLAYER_MOVE_SPEED_FACTOR  * Time.deltaTime);
        }

        if(Input.GetKeyDown(KeyCode.Space)) {
            grounded = false;
            rb.AddForce(Vector2.up * 10f, ForceMode2D.Impulse);
        }
    }

    void applyGravity() {
        if(!grounded) {
            rb.AddForce(new Vector2(0f, GRAVITY_VELOCITY));
        } else {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }    

    /// <summary>
    /// Update the player's collision states in all directions
    /// </summary>
    void updateCollisions() {
        int raycastCount = 7;

        // down
        collisions.down = false;
        grounded = false;
        for(int i = 0; i < raycastCount; i++) {
            float subDivDistance = playerDimensions.x / (float)(raycastCount - 1);
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
                    transform.position = new Vector2(transform.position.x, hit.collider.gameObject.transform.position.y + 1f);
                }
            }
        }

        // up
        collisions.up = false;
        for(int i = 0; i < raycastCount; i++) {
            float subDivDistance = playerDimensions.x / (float)(raycastCount - 1);
            Vector2 rayStart = new Vector2(
                (transform.position.x - (playerDimensions.x / 2f)) + (subDivDistance * i),
                transform.position.y + (playerDimensions.y / 2f));

            RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.up, 0.1f, LAYER_TERRAIN);
            Debug.DrawRay(rayStart, Vector2.up * 0.1f, Color.green);

            if(hit) {
                collisions.up = true;

                //transform.position = new Vector2(transform.position.x, hit.collider.gameObject.transform.position.y - 1f);
            }
        }

        // right
        collisions.right = false;
        for(int i = 0; i < raycastCount; i++) {
            float subDivDistance = playerDimensions.x / (float)(raycastCount - 1);
            Vector2 rayStart = new Vector2(
                (transform.position.x + (playerDimensions.x / 2f)),
                transform.position.y - (playerDimensions.y / 2f) + (subDivDistance * i));

            RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.right, 0.1f, LAYER_TERRAIN);
            Debug.DrawRay(rayStart, Vector2.right * 0.1f, Color.green);

            if(hit) {
                collisions.right = true;

                //transform.position = new Vector2(hit.collider.gameObject.transform.position.x - 1f, transform.position.y);
            }
        }

        // left
        collisions.left = false;
        for(int i = 0; i < raycastCount; i++) {
            float subDivDistance = playerDimensions.x / (float)(raycastCount - 1);
            Vector2 rayStart = new Vector2(
                (transform.position.x - (playerDimensions.x / 2f)),
                transform.position.y - (playerDimensions.y / 2f) + (subDivDistance * i));

            RaycastHit2D hit = Physics2D.Raycast(rayStart, -Vector2.right, 0.1f, LAYER_TERRAIN);
            Debug.DrawRay(rayStart, -Vector2.right * 0.1f, Color.green);

            if(hit) {
                collisions.left = true;

                //transform.position = new Vector2(hit.collider.gameObject.transform.position.x + 1f, transform.position.y);
            }
        }

    }
}
