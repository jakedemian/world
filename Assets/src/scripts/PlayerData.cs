using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour {
    public AudioClip levelUpSound;

    public int maxHealth = 5;
    public float maxStam = 100;

    public int health;
    public float stamina;
    public int level;
    public int experience;

    // TODO this needs to come from an xml file somewhere, maybe even have a separate class for it
    private int experienceToNextLevel = 100;



    private float stamRechargeTimer = 0f;

    private float STAMINA_USE_RECHARGE_DELAY = 1.5f;
    private float STAMINA_RECHARGE_SPEED = 40f;

    public static float PLAYER_SPRINT_STAMINA_DRAIN_RATE = 12f;

    /// <summary>
    ///     START
    /// </summary>
    void Start() {
        health = maxHealth;
        stamina = maxStam;

        level = 1;
        experience = 0;        
    }

    void Update() {
        if(experience >= experienceToNextLevel) {
            levelUp();
        }
    }

    /// <summary>
    ///     FIXED UPDATE
    /// </summary>
    void FixedUpdate() {
        if(stamRechargeTimer > 0f) {
            stamRechargeTimer -= Time.deltaTime;

            if(stamRechargeTimer <= 0f) {
                stamRechargeTimer = 0f;
            }
        } else if(stamina < maxStam) {
            stamina += STAMINA_RECHARGE_SPEED * Time.deltaTime;

            if(stamina > maxStam) {
                stamina = maxStam;
            }
        }
    }

    private void levelUp() {
        experience -= experienceToNextLevel;

        // TODO fix this, or come up with some actual mathematical function for it
        experienceToNextLevel = (int) (experienceToNextLevel * 1.2f);
        level++;

        // OTHER STUFF, assign attribute points or health/stam or whatever i decide later

        // play the level up sound
        AudioSource.PlayClipAtPoint(levelUpSound, transform.position);
    }

    /// <summary>
    ///     Use some stamina, if any is available.
    /// </summary>
    /// <param name="amount"></param>
    public void useStamina(float amount) {
        if(stamina > 0f) {
            stamRechargeTimer = STAMINA_USE_RECHARGE_DELAY;
            stamina -= amount;
            if(stamina < 0f) {
                stamina = 0f;
            }
        } else {
            // ??? TODO ???
            // should i punish the player for over-using stam?
        }
    }
}
