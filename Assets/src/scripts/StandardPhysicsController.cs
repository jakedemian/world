using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardPhysicsController : MonoBehaviour {

    [HideInInspector]
    public bool grounded = false;

    [HideInInspector]
    public bool isOnWall = false;

    [HideInInspector]
    public CollisionDirections collisions = new CollisionDirections();


    [HideInInspector]
    public Rigidbody2D rb;

    private Bounds bounds;
    private Vector2 dimensions;

    private const int LAYER_TERRAIN = 1 << 9;
    private const float COLLISION_RAYCAST_DISTANCE = 0.05f;

    private const float GRAVITY_VELOCITY = -40f;
    private const float WALL_SLIDE_GRAVITY_VELOCITY = -15f;
    private const float MAX_FALL_SPEED = -18f;
    private const float MAX_WALL_SLIDE_SPEED = -7f;

    private const float MAX_HORIZONTAL_MOVE_SPEED = 15f;
    private const float MAX_HORIZONTAL_MOVE_SPEED_AIR = 10f;


    void Start() {
        rb = GetComponent<Rigidbody2D>();
        bounds = GetComponent<BoxCollider2D>().bounds;
        dimensions = new Vector2(bounds.max.x - bounds.min.x, bounds.max.y - bounds.min.y);
    }

    void FixedUpdate() {
        applyGravity();
    }

    void Update() {
        updateCollisions();
        capSpeed();
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
    ///     Max out the player's speed
    /// </summary>
    private void capSpeed() {
        // trim fall speed
        float maxVertSpeed = isOnWall ? MAX_WALL_SLIDE_SPEED : MAX_FALL_SPEED;
        if(rb.velocity.y < maxVertSpeed) {
            rb.velocity = new Vector2(rb.velocity.x, maxVertSpeed);
        }

        // move speed
        float maxMoveSpeed = grounded ? MAX_HORIZONTAL_MOVE_SPEED : MAX_HORIZONTAL_MOVE_SPEED_AIR;
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
    private void preventPhaseThroughGround(RaycastHit2D downHit) {
        BoxCollider2D otherCollider = (BoxCollider2D)downHit.collider;
        float otherColliderVerticalRadius = otherCollider.size.y / 2f;
        float collisionPointToColliderCenterVerticalDistance = Mathf.Abs(downHit.point.y - downHit.collider.gameObject.transform.position.y);
        float collisionPointToColliderCenterHorizontalDistance = Mathf.Abs(downHit.point.x - downHit.collider.gameObject.transform.position.x);
        float arbitraryThresholdForVerticalPositionFix = -10f;

        if(collisionPointToColliderCenterVerticalDistance <= 0.51f
            && collisionPointToColliderCenterHorizontalDistance <= 0.51f
            && rb.velocity.y <= arbitraryThresholdForVerticalPositionFix) {
            transform.position = new Vector2(transform.position.x, downHit.collider.gameObject.transform.position.y + 1f);
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }

    /// <summary>
    ///     Check if both downward raycasts are colliding.
    /// </summary>
    /// <param name="startPoint1">The start point of the first raycast.</param>
    /// <param name="startPoint2">The start point of the second raycast.</param>
    /// <returns>True if both are colliding, false otherwise.</returns>
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
        bounds = GetComponent<BoxCollider2D>().bounds;
        grounded = false;
        isOnWall = false;

        Vector2 bottomLeft = new Vector2(bounds.min.x + 0.001f, bounds.min.y + 0.001f);
        Vector2 bottomRight = new Vector2(bounds.max.x - 0.001f, bounds.min.y + 0.001f);
        Vector2 topLeft = new Vector2(bounds.min.x + 0.001f, bounds.max.y - 0.001f);
        Vector2 topRight = new Vector2(bounds.max.x - 0.001f, bounds.max.y - 0.001f);

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
            preventPhaseThroughGround(downHit);
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
