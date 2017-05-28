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
        if(!combatCtrl.activeShield) {
            if(rb.velocity.x < 0) {
                GetComponent<SpriteRenderer>().flipX = true;
                //transform.localScale = new Vector3(-startingXScale, transform.localScale.y, 1f);
            } else if(rb.velocity.x > 0) {
                GetComponent<SpriteRenderer>().flipX = false;
                //transform.localScale = new Vector3(startingXScale, transform.localScale.y, 1f);
            }
        }

        // update animation state
        if(playerMovement.grounded) {
            if(Mathf.Abs(rb.velocity.x) >= PlayerMovement.PLAYER_MIN_MOVE_SPEED) {
                an.SetTrigger("run");

                if(Input.GetAxis("Sprint") != 0 && playerData.stamina > 0f) {
                    an.SetFloat("runSpeed", RUN_SPEED_SPRINT);
                } else {
                    an.SetFloat("runSpeed", RUN_SPEED_NORMAL);
                }
            } else {
                an.SetTrigger("stop");
            }
        } else {
            if(!playerMovement.isOnWall) {
                an.SetTrigger("jump");
            } else {
                an.SetTrigger("wall");
            }
        }

        
    }

}
