﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private Rigidbody2D rb;
    private bool grounded = false;
    private CollisionDirections collisions = new CollisionDirections();
    private Vector2 playerDimensions;

    private const int LAYER_TERRAIN = 1 << 9;
    private const float GRAVITY_VELOCITY = -9.8f;
    
    
	void Start () {
        rb = GetComponent<Rigidbody2D>();

        // update player dimensions
        Bounds b = GetComponent<Renderer>().bounds;
        playerDimensions = new Vector2(b.max.x - b.min.x, b.max.y - b.min.y);
	}

    void FixedUpdate() {
        //applyGravity();
    }

	void Update () {
        updateCollisions();
        Debug.Log(collisions.left);
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
        int raycastCount = 7;

        // down
        collisions.down = false;
        for(int i = 0; i < raycastCount; i++) {
            float subDivDistance = playerDimensions.x / (float)(raycastCount - 1);
            Vector2 rayStart = new Vector2(
                (transform.position.x - (playerDimensions.x / 2f)) + (subDivDistance * i),
                transform.position.y - (playerDimensions.y / 2f));

            RaycastHit2D hit = Physics2D.Raycast(rayStart, -Vector2.up, 0.1f, LAYER_TERRAIN);
            Debug.DrawRay(rayStart, -Vector2.up * 0.1f, Color.green);

            if(hit) {
                collisions.down = true;
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
            }
        }

    }
}
