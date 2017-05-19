using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private Rigidbody2D rb;
    private bool grounded = false;

    private const float gravityVelocity = -9.8f;
    
	void Start () {
        rb = GetComponent<Rigidbody2D>();
	}

    void FixedUpdate() {
        applyGravity();
    }

	void Update () {

	}

    void applyGravity() {
        if(!grounded) {
            rb.AddForce(new Vector2(0f, gravityVelocity));
        }
    }

    void updateRaycasts() {
        
    }
}
