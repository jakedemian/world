using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceOrbCtrl : MonoBehaviour {

    private GameObject player;
    bool nearPlayer = false;

    private int experienceValue = 10;

    private const float DISTANCE_PLAYER_PLAYER_TO_ACTIVATE = 5f;
    private const float DISTANCE_PLAYER_PLAYER_TO_CAPTURE = 0.5f;


    private const float MAX_MOVE_SPEED = 1f;
    private const float MOVE_SPEED_ACCELERATION = 0.003f;
    private float moveSpeed = 0f;

    /// <summary>
    ///     START
    /// </summary>
    void Start() {
        player = GameObject.FindWithTag("Player");
        if(player == null) {
            Debug.LogError("Experience orb was unable to find Player object.");
        }
    }

    /// <summary>
    ///     UPDATE
    /// </summary>
    void Update() {
        if(!nearPlayer) {
            if(orbIsWithinXUnitsOfPlayer(DISTANCE_PLAYER_PLAYER_TO_ACTIVATE)) {
                nearPlayer = true;
            }
        } else if(orbIsWithinXUnitsOfPlayer(DISTANCE_PLAYER_PLAYER_TO_CAPTURE)) {
            player.GetComponent<PlayerData>().experience += experienceValue;
            Destroy(gameObject); // destroy self after rewarding xp to player
        }
    }

    /// <summary>
    ///     FIXED UPDATE
    /// </summary>
    void FixedUpdate() {
        if(nearPlayer) {
            moveTowardsPlayer();
        }
    }

    /// <summary>
    ///     Detects if an orb is within a certain number of units of the player
    /// </summary>
    /// <param name="units"></param>
    /// <returns></returns>
    private bool orbIsWithinXUnitsOfPlayer(float units) {
        return Mathf.Sqrt(Mathf.Pow(transform.position.x - player.transform.position.x, 2) + Mathf.Pow(transform.position.y - player.transform.position.y, 2)) < units;
    }

    /// <summary>
    ///     Move/accelerate towards the player.
    /// </summary>
    private void moveTowardsPlayer() {
        Vector2 myPos = transform.position;
        Vector2 playerPos = player.transform.position;

        moveSpeed += MOVE_SPEED_ACCELERATION;
        if(moveSpeed > MAX_MOVE_SPEED) {
            moveSpeed = MAX_MOVE_SPEED;
        }

        transform.position = Vector2.MoveTowards(myPos, playerPos, moveSpeed);
    }
}
