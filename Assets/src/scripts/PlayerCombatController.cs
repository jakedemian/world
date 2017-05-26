using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour {
    public List<GameObject> swordSlashPrefabs;
    public GameObject equippedShieldPrefab;

    [HideInInspector]
    public GameObject isShieldActive;

    private GameObject currentSwordSlash = null;
    private Vector2 slashRelativePosToPlayer = new Vector2(0f, 0f);

    private PlayerData playerData;
    private PlayerMovement playerMovement;
    private bool shieldIsUp = false;

    private int swingState = 0;
    private float swingTimer = 0f;
    private SwordData swordData = new SwordData();
    private ShieldData shieldData = new ShieldData();
    
    // TODO FIXME do the same thing with shields as i did with swords


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
                    isShieldActive = Instantiate(equippedShieldPrefab, new Vector2(transform.position.x + shieldData.getDistanceFromPlayer(), transform.position.y), Quaternion.identity);
                } else {
                    isShieldActive = Instantiate(equippedShieldPrefab, new Vector2(transform.position.x - shieldData.getDistanceFromPlayer(), transform.position.y), Quaternion.identity);
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
        if(Input.GetButtonDown("Swing") && playerData.stamina > 0f && currentSwordSlash == null && !isShieldActive && swingState == SwordData.SWING_STATE_NONE) {
            swingState = SwordData.SWING_STATE_PRESWING;
            swingTimer = swordData.getSwordPreSwingDelay();
            playerMovement.playerLocked = true;
        }

        // TODO move this to swing object
        updateSlashPosition();
    }

    /// <summary>
    ///     FIXED UPDATE
    /// </summary>
    void FixedUpdate() {
        if(swingState != SwordData.SWING_STATE_NONE) {
            swingTimer -= Time.deltaTime;
            manageSwingState();
        }
    }

    /// <summary>
    ///     Manage the different states of a sword swing.
    /// </summary>
    private void manageSwingState() {
        if(swingTimer <= 0) {
            if(swingState == SwordData.SWING_STATE_PRESWING) {
                swingState = SwordData.SWING_STATE_SWING;
                swingTimer = swordData.getSwordSwingTime();

                swingSword();
            } else if(swingState == SwordData.SWING_STATE_SWING) {
                swingState = SwordData.SWING_STATE_POSTSWING;
                swingTimer = swordData.getSwordPostSwingDelay();

            } else if(swingState == SwordData.SWING_STATE_POSTSWING) {
                swingState = SwordData.SWING_STATE_NONE;
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
                isShieldActive.transform.position = new Vector2(transform.position.x + shieldData.getDistanceFromPlayer(), transform.position.y);
            } else {
                isShieldActive.transform.position = new Vector2(transform.position.x - shieldData.getDistanceFromPlayer(), transform.position.y);
            }
        }
    }

    /// <summary>
    ///     Create a sword swing.
    /// </summary>
    private void swingSword() {
        int slashIdx = Random.Range(0, swordSlashPrefabs.Count);

        playerData.useStamina(20f);
        string analogPrimaryDir = InputHelper.getPrimaryAnalogStickDirection();
        if(analogPrimaryDir.Equals("up") || analogPrimaryDir.Equals("down")) {
            if(analogPrimaryDir.Equals("up")) {
                slashRelativePosToPlayer = new Vector2(0f, swordData.getSwordSwingDistanceFromPlayer());
                currentSwordSlash = Instantiate(swordSlashPrefabs[slashIdx], new Vector2(transform.position.x, transform.position.y + swordData.getSwordSwingDistanceFromPlayer()), Quaternion.Euler(0, 0, 90));
            } else {
                slashRelativePosToPlayer = new Vector2(0f, -swordData.getSwordSwingDistanceFromPlayer());
                currentSwordSlash = Instantiate(swordSlashPrefabs[slashIdx], new Vector2(transform.position.x, transform.position.y - swordData.getSwordSwingDistanceFromPlayer()), Quaternion.Euler(0, 0, -90));
            }
        } else {
            if(playerMovement.currentFacingDirection == 1) {
                slashRelativePosToPlayer = new Vector2(swordData.getSwordSwingDistanceFromPlayer(), 0f);
                currentSwordSlash = Instantiate(swordSlashPrefabs[slashIdx], new Vector2(transform.position.x + swordData.getSwordSwingDistanceFromPlayer(), transform.position.y), Quaternion.identity);
            } else {
                slashRelativePosToPlayer = new Vector2(-swordData.getSwordSwingDistanceFromPlayer(), 0f);
                currentSwordSlash = Instantiate(swordSlashPrefabs[slashIdx], new Vector2(transform.position.x - swordData.getSwordSwingDistanceFromPlayer(), transform.position.y), Quaternion.identity);
                currentSwordSlash.transform.localScale = new Vector2(-1f, currentSwordSlash.transform.localScale.y);
            }
        }
    }

    /// <summary>
    ///     Keep the slash object the same distance from the player at all times.
    /// </summary>
    private void updateSlashPosition() {
        if(currentSwordSlash != null) {
            currentSwordSlash.transform.position = new Vector2(transform.position.x + slashRelativePosToPlayer.x, transform.position.y + slashRelativePosToPlayer.y);
        }
    }	
}
