using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour {

    public List<GameObject> swordSlashPrefabs;
    private GameObject currentSwordSlash = null;

    private PlayerData playerData;
    private PlayerMovement playerMovement;

    ///// TODO ///////////////////////////////////
    private float swordPreSwingDelay = 0.5f;
    private float swordPostSwingDelay = 0.5f;
    private float swingTime = 0.2f;
    private float swingReach = 1f;
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
        if(Input.GetButtonDown("Swing") && playerData.stamina > 0f && currentSwordSlash == null) {

            // TODO make the player input-locked and make them freeze horizontally.  This state will last for
            //      swordPreSwingDelay, swordPostSwingDelay, and swingTime.  Big swords will do more damage but
            //      eat up lots of time during the swing, while small swords to little damage while having very
            //      quick turnover of recovery time

            int slashIdx = Random.Range(0, swordSlashPrefabs.Count);

            playerData.useStamina(20f);
            if(playerMovement.currentFacingDirection == 1) {
                currentSwordSlash = Instantiate(swordSlashPrefabs[slashIdx], new Vector2(transform.position.x + 1, transform.position.y), Quaternion.identity);
            } else {
                currentSwordSlash = Instantiate(swordSlashPrefabs[slashIdx], new Vector2(transform.position.x - 1, transform.position.y), Quaternion.identity);
                currentSwordSlash.transform.localScale = new Vector2(-1f, currentSwordSlash.transform.localScale.y);
            }

        }
        updateSlashPosition();
    }

    /// <summary>
    ///     Keep the slash object the same distance from the player at all times.
    /// </summary>
    void updateSlashPosition() {
        if(currentSwordSlash != null) {
            if(playerMovement.currentFacingDirection == 1) {
                currentSwordSlash.transform.position = new Vector2(transform.position.x + 1.5f, transform.position.y);
            } else {
                currentSwordSlash.transform.position = new Vector2(transform.position.x - 1.5f, transform.position.y);
            }
        }
    }

	
}
