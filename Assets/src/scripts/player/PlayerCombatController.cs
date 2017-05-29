using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerCombatController : MonoBehaviour {
    public List<GameObject> swordSlashPrefabs;
    public GameObject equippedShieldPrefab;

    // FIXME does this actually need to be public??? not sure what i was thinking here
    [HideInInspector]
    public GameObject activeShield;

    [HideInInspector]
    public bool shieldIsUp = false;

    // FIXME ? do these need to be public??
    [HideInInspector]
    public GameObject equippedSword;

    [HideInInspector]
    public GameObject equippedShield;

    private PlayerData playerData;
    private PlayerInputController playerMovement;
    private SwordData swordData = new SwordData();
    private ShieldData shieldData = new ShieldData();

    private GameObject currentSwordSlash = null;
    private Vector2 slashRelativePosToPlayer = new Vector2(0f, 0f);   

    private int swingState = 0;
    private float swingTimer = 0f;    
    private SpriteRenderer equippedSwordSpriteRenderer;
    private Sprite equippedWeaponSprite;

    private SpriteRenderer equippedShieldSpriteRenderer;
    private Sprite equippedShieldSprite;
    

    /// <summary>
    ///     START
    /// </summary>
    void Start() {
        playerData = GetComponent<PlayerData>();
        playerMovement = GetComponent<PlayerInputController>();
        equippedSword = GameObject.FindWithTag("EquippedSword");
        equippedSwordSpriteRenderer = equippedSword.GetComponent<SpriteRenderer>();

        equippedShield = GameObject.FindWithTag("EquippedShield");
        equippedShieldSpriteRenderer = equippedShield.GetComponent<SpriteRenderer>();

        // TODO combine these into one method
        setEquippedSword();
        setEquippedShield();
    }

    /// <summary>
    ///     UPDATE
    /// </summary>
    void Update() {
        // shield
        if(Input.GetAxis("Shield") != 0) {
            if(activeShield == null && !shieldIsUp) {
                shieldIsUp = true;

                if(playerMovement.currentFacingDirection == 1) {
                    activeShield = Instantiate(equippedShieldPrefab, new Vector2(transform.position.x + shieldData.getDistanceFromPlayer(), transform.position.y), Quaternion.identity);
                } else {
                    activeShield = Instantiate(equippedShieldPrefab, new Vector2(transform.position.x - shieldData.getDistanceFromPlayer(), transform.position.y), Quaternion.identity);
                    activeShield.transform.localScale = new Vector2(-1f, activeShield.transform.localScale.y);
                }
                activeShield.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(shieldData.getSideImagePath());
            }

            // TODO move this to shield object
        } else if(shieldIsUp) {
            shieldIsUp = false;
            Destroy(activeShield);
            activeShield = null; // explicit set to null
        }

        // sword
        if(Input.GetButtonDown("Swing") && playerData.stamina > 0f && currentSwordSlash == null && !activeShield && swingState == SwordData.SWING_STATE_NONE) {
            swingState = SwordData.SWING_STATE_PRESWING;
            swingTimer = swordData.getSwordPreSwingDelay();
            playerMovement.playerLocked = true;
        }

        // TODO move this to swing object
        updateSlashPosition();
        updateShieldPosition();
        updateEquipmentOrientation();
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

    private void updateEquipmentOrientation() {
        // update sword and shield x scale based on facing direction
        if(playerMovement.currentFacingDirection == 1) {
            equippedSword.transform.localScale = new Vector2(1f, equippedSword.transform.localScale.y);
            equippedShield.transform.localScale = new Vector2(1f, equippedSword.transform.localScale.y);
        } else {
            equippedSword.transform.localScale = new Vector2(-1f, equippedSword.transform.localScale.y);
            equippedShield.transform.localScale = new Vector2(-1f, equippedSword.transform.localScale.y);
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
        if(activeShield != null) {
            if(equippedShield.activeInHierarchy) {
                equippedShield.SetActive(false);
            }

            if(playerMovement.currentFacingDirection == 1) {
                activeShield.transform.position = new Vector2(transform.position.x + shieldData.getDistanceFromPlayer(), transform.position.y);
            } else {
                activeShield.transform.position = new Vector2(transform.position.x - shieldData.getDistanceFromPlayer(), transform.position.y);
            }
        } else {
            if(!equippedShield.activeInHierarchy) {
                equippedShield.SetActive(true);
            }
        }
    }

    /// <summary>
    ///     Create a sword swing.
    /// </summary>
    private void swingSword() {
        int slashIdx = Random.Range(0, swordSlashPrefabs.Count);

        // TODO
        // TODO make this more efficient
        // TODO

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
            if(equippedSword.activeInHierarchy) {
                equippedSword.SetActive(false);
            }

            currentSwordSlash.transform.position = new Vector2(transform.position.x + slashRelativePosToPlayer.x, transform.position.y + slashRelativePosToPlayer.y);
        } else {
            if(!equippedSword.activeInHierarchy) {
                equippedSword.SetActive(true);
            }
        }
    }

    private void setEquippedSword() {
        // FIXME do i need to actually store this as a member var???
        equippedWeaponSprite = AssetDatabase.LoadAssetAtPath<Sprite>(swordData.getImagePath());
        equippedSwordSpriteRenderer.sprite = equippedWeaponSprite;
    }

    private void setEquippedShield() {
        // FIXME do i need to actually store this as a member var???
        equippedShieldSprite = AssetDatabase.LoadAssetAtPath<Sprite>(shieldData.getBackImagePath());
        equippedShieldSpriteRenderer.sprite = equippedShieldSprite;
    }

    private void changeSword(int swordId) {
        swordData = new SwordData(swordId);
        setEquippedSword();
    }

    private void changeShield(int shieldId) {
        shieldData = new ShieldData(shieldId);
        setEquippedShield();
    }
}
