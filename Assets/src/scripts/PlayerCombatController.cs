using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour {

    public List<GameObject> swordSlashPrefabs;
    public GameObject equippedShieldPrefab;

    [HideInInspector]
    public GameObject isShieldActive;

    private GameObject currentSwordSlash = null;

    private PlayerData playerData;
    private PlayerMovement playerMovement;
    private bool shieldIsUp = false;



    // swing states ////////////////////////////////////////
    private int swingState = 0;
    private float swingTimer = 0f;

    private const int SWING_STATE_NONE = 0;
    private const int SWING_STATE_PRESWING = 1;
    private const int SWING_STATE_SWING = 2;
    private const int SWING_STATE_POSTSWING = 3;

    private float swordPreSwingDelay = 0.1f;
    private float swingTime = 0.1f;
    private float swordPostSwingDelay = 0.1f;
    private float swingWidth = 1f;
    private float swingDistanceFromPlayer = 1.5f;
    //////////////////////////////////////////////




    ///// TODO ///////////////////////////////////
    private float shieldDistanceFromPlayer = 1f;
    //////////////////////////////////////////////

    


    /// <summary>
    ///     START
    /// </summary>
    void Start() {
        playerData = GetComponent<PlayerData>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    /// <summary>
    ///     UPDATE
    /// </summary>
    void Update() {
        // shield
        if(Input.GetAxis("Shield") != 0) {
            if(isShieldActive == null && !shieldIsUp) {
                shieldIsUp = true;

                if(playerMovement.currentFacingDirection == 1) {
                    isShieldActive = Instantiate(equippedShieldPrefab, new Vector2(transform.position.x + shieldDistanceFromPlayer, transform.position.y), Quaternion.identity);
                } else {
                    isShieldActive = Instantiate(equippedShieldPrefab, new Vector2(transform.position.x - shieldDistanceFromPlayer, transform.position.y), Quaternion.identity);
                    isShieldActive.transform.localScale = new Vector2(-1f, isShieldActive.transform.localScale.y);
                }
            }

            // TODO move this to shield object
            updateShieldPosition();
        } else if(shieldIsUp) {
            shieldIsUp = false;
            Destroy(isShieldActive);
            isShieldActive = null; // explicit set to null
        }

        // sword
        if(Input.GetButtonDown("Swing") && playerData.stamina > 0f && currentSwordSlash == null && !isShieldActive && swingState == SWING_STATE_NONE) {
            swingState = SWING_STATE_PRESWING;
            swingTimer = swordPreSwingDelay;
            playerMovement.playerLocked = true;
        }

        // TODO move this to swing object
        updateSlashPosition();
    }

    /// <summary>
    ///     FIXED UPDATE
    /// </summary>
    void FixedUpdate() {
        if(swingState != SWING_STATE_NONE) {
            swingTimer -= Time.deltaTime;
            manageSwingState();
        }
    }

    /// <summary>
    ///     Manage the different states of a sword swing.
    /// </summary>
    private void manageSwingState() {
        if(swingTimer <= 0) {
            if(swingState == SWING_STATE_PRESWING) {
                swingState = SWING_STATE_SWING;
                swingTimer = swingTime;

                swingSword();
            } else if(swingState == SWING_STATE_SWING) {
                swingState = SWING_STATE_POSTSWING;
                swingTimer = swordPostSwingDelay;

            } else if(swingState == SWING_STATE_POSTSWING) {
                swingState = SWING_STATE_NONE;
                swingTimer = 0f;
                playerMovement.playerLocked = false;
            }
        }
    }

    /// <summary>
    ///     Keep the shield object the same distance from the player at all times.
    /// </summary>
    private void updateShieldPosition() {
        if(isShieldActive != null) {
            if(playerMovement.currentFacingDirection == 1) {
                isShieldActive.transform.position = new Vector2(transform.position.x + shieldDistanceFromPlayer, transform.position.y);
            } else {
                isShieldActive.transform.position = new Vector2(transform.position.x - shieldDistanceFromPlayer, transform.position.y);
            }
        }
    }

    /// <summary>
    ///     Create a sword swing.
    /// </summary>
    private void swingSword() {
        int slashIdx = Random.Range(0, swordSlashPrefabs.Count);

        playerData.useStamina(20f);
        if(playerMovement.currentFacingDirection == 1) {
            currentSwordSlash = Instantiate(swordSlashPrefabs[slashIdx], new Vector2(transform.position.x + swingDistanceFromPlayer, transform.position.y), Quaternion.identity);
        } else {
            currentSwordSlash = Instantiate(swordSlashPrefabs[slashIdx], new Vector2(transform.position.x - swingDistanceFromPlayer, transform.position.y), Quaternion.identity);
            currentSwordSlash.transform.localScale = new Vector2(-1f, currentSwordSlash.transform.localScale.y);
        }
    }

    /// <summary>
    ///     Keep the slash object the same distance from the player at all times.
    /// </summary>
    private void updateSlashPosition() {
        if(currentSwordSlash != null) {
            if(playerMovement.currentFacingDirection == 1) {
                currentSwordSlash.transform.position = new Vector2(transform.position.x + swingDistanceFromPlayer, transform.position.y);
            } else {
                currentSwordSlash.transform.position = new Vector2(transform.position.x - swingDistanceFromPlayer, transform.position.y);
            }
        }
    }

	
}
