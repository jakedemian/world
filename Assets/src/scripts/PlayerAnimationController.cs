using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour {
    private Animator an;
    private PlayerMovement playerMovement;
    private PlayerCombatController combatCtrl;
    private PlayerData playerData;
    private Rigidbody2D rb;

    //private bool playerInAir = false;

    private const float RUN_SPEED_NORMAL = 1.0f;
    private const float RUN_SPEED_SPRINT = 1.3f;

    private float startingXScale;

    void Start() {
        an = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        combatCtrl = GetComponent<PlayerCombatController>();
        playerData = GetComponent<PlayerData>();
        rb = GetComponent<Rigidbody2D>();

        startingXScale = transform.localScale.x;
    }

    void Update() {
        if(rb.velocity.x < 0) {
            transform.localScale = new Vector2(-startingXScale, transform.localScale.y);
        } else if(rb.velocity.x > 0){
            transform.localScale = new Vector2(startingXScale, transform.localScale.y);
        }


        if(playerMovement.grounded && (Mathf.Abs(rb.velocity.x) >= PlayerMovement.PLAYER_MIN_MOVE_SPEED)) {
            an.SetTrigger("run");

            if(Input.GetAxis("Sprint") != 0 && playerData.stamina > 0f) {
                an.SetFloat("runSpeed", RUN_SPEED_SPRINT);
            } else {
                an.SetFloat("runSpeed", RUN_SPEED_NORMAL);
            }
        } else {
            an.SetTrigger("stop");
        }

        if(playerMovement.grounded && !playerMovement.isOnWall) {
            Debug.Log("1");
            an.SetTrigger("land");
        } else if(!playerMovement.isOnWall) {
            Debug.Log("2");

            an.SetTrigger("jump");
        } else if(playerMovement.isOnWall) {
            Debug.Log("3");

            an.SetTrigger("wall");
        }
    }

}
