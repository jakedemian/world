using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
    public GameObject player;
    public GameObject healthBlockPrefab;
    public GameObject stamBlockPrefab;
    public GameObject healthBar;

    private List<GameObject> playerHealthBlockList;
    private List<GameObject> playerStamBlockList;

    private int previousPlayerHealth = 0;
    private float previousPlayerStamina = 0f;

    private int playerHealth = 0;
    private float playerStamina = 0f;

    void Start() {
        playerHealthBlockList = new List<GameObject>();
        playerStamBlockList = new List<GameObject>();
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Q)) {
            Application.Quit();
        }

        if(Input.GetKeyDown(KeyCode.R)) {
            Application.LoadLevel("test_scene");
        }
    }

    void OnGUI() {
        playerHealth = player.GetComponent<PlayerData>().health;
        playerStamina = player.GetComponent<PlayerData>().stamina;

        // make sure we don't go below 0 health or 0 stam
        playerHealth = playerHealth < 0 ? 0 : playerHealth;
        playerStamina = playerStamina < 0f ? 0f : playerStamina;

        // draw health blocks
        if(playerHealth != previousPlayerHealth) {
            clearHealthUI();
            for(int i = 0; i < playerHealth; i++) {
                GameObject newHealthBlock = Instantiate(healthBlockPrefab, healthBar.transform);
                RectTransform rt = newHealthBlock.GetComponent<RectTransform>();
                rt.localPosition = new Vector2(rt.localPosition.x + (i * 0.85f * rt.rect.width), rt.localPosition.y);
                playerHealthBlockList.Add(newHealthBlock);

            }
        }

        // draw stam blocks
        if(playerStamina != previousPlayerStamina) {
            clearStaminaUI();
            int blockNum = 0;
            for(int i = 0; i < playerStamina; i++) {
                if(i % 20 == 0) {
                    GameObject newStamBlock = Instantiate(stamBlockPrefab, healthBar.transform);
                    RectTransform rt = newStamBlock.GetComponent<RectTransform>();
                    rt.localPosition = new Vector2(rt.localPosition.x + (blockNum * 0.85f * rt.rect.width), rt.localPosition.y - (1.5f * rt.rect.height));
                    playerStamBlockList.Add(newStamBlock);
                    blockNum++;
                }
            }
        }

        // finish up by setting the next round's "previous" values to this round's values
        previousPlayerHealth = playerHealth;
        previousPlayerStamina = playerStamina;
    }

    void clearHealthUI() {
        for(int i = 0; i < playerHealthBlockList.Count; i++) {
            Destroy(playerHealthBlockList[i]);
        }

        // TODO is this memory leak safe???
        playerHealthBlockList.Clear();
        playerHealthBlockList = new List<GameObject>();
    }

    void clearStaminaUI() {
        for(int i = 0; i < playerStamBlockList.Count; i++) {
            Destroy(playerStamBlockList[i]);
        }

        // TODO is this memory leak safe???
        playerStamBlockList.Clear();
        playerStamBlockList = new List<GameObject>();
    }
}
