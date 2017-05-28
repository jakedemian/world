using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSaveController : MonoBehaviour {
    private PlayerData playerData;
    private PlayerMovement playerMovement;
    private StandardPhysicsController physicsCtrl;

    private struct SaveData {
        public Vector2 pos;
        public int maxHealth;
        public float maxStam;
        
        // TODO items in inventory
        // spells you have acquired


        // might have to store some enemy / boss kill info 
        //      here so that they can be respawned.
    }
    private SaveData save;

    private const float MAX_DISTANCE_FROM_LAMP_TO_SAVE = 4f;

	/// <summary>
    ///     START
    /// </summary>
    void Start () {
        playerData = GetComponent<PlayerData>();
        playerMovement = GetComponent<PlayerMovement>();
        physicsCtrl = GetComponent<StandardPhysicsController>();

        // try to load save data from file
        if(false) { // we can load save data
            // load save data
        } else {
            initSaveData(); // used at start of the game of if there is a problem loading
        }
	}
	
	/// <summary>
    ///     UPDATE
    /// </summary>
    void Update () {
        if(playerData.health <= 0) {
            goToLastSave();
        }

        if(physicsCtrl.grounded && isPlayerNearLamp() && Input.GetButtonDown("Interact")) {            
            updateSave();
        }
        
	}

    /// <summary>
    ///     Determines if the player is close to a lamp object.
    /// </summary>
    /// <returns>True if any lamp is close to the player, false otherwise.</returns>
    private bool isPlayerNearLamp() {
        bool res = false;

        GameObject[] activeLamps = GameObject.FindGameObjectsWithTag("Lamp");
        for(int i = 0; i < activeLamps.Length; i++) {
            if(Utilities.getDistanceBetweenTwoPoints(activeLamps[i].transform.position, transform.position) < MAX_DISTANCE_FROM_LAMP_TO_SAVE) {
                res = true;
                break;
            }
        }

        return res;
    }

    /// <summary>
    ///     Save the game.
    /// </summary>
    private void updateSave() {
        Debug.Log("saved game!");

        save.pos = transform.position;
        save.maxHealth = playerData.maxHealth;
        save.maxStam = playerData.maxStam;
    }

    /// <summary>
    ///     Initialize the save object.
    /// </summary>
    private void initSaveData() {
        save = new SaveData();
        updateSave();
    }

    /// <summary>
    ///     Load the last save.
    /// </summary>
    private void goToLastSave() {
        // TODO show black/load screen
        // NOTE: if max health/stam can only be updated at save point, then these attributes arent even needed
        playerData.maxHealth = save.maxHealth;
        playerData.maxStam = save.maxStam;
        playerData.health = playerData.maxHealth;
        playerData.stamina = playerData.maxStam;
        playerMovement.rb.velocity = Vector2.zero;


        // last, we must update player position and load world around him
        transform.position = save.pos;
        // TODO update chunk load info and load chunks near player
        // NOTE actually the way i program chunk controller might make this automatically happen

        // TODO remove black/load screen
    }
}
