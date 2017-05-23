﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour {
    public int maxHealth = 5;
    public float maxStam = 100;

    public int health;
    public float stamina;

    private float stamRechargeTimer = 0f;

    private float STAMINA_USE_RECHARGE_DELAY = 1.5f;
    private float STAMINA_RECHARGE_SPEED = 40f;

    void Start() {
        health = maxHealth;
        stamina = maxStam;
    }

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

    public void useStamina(float amount) {
        if(stamina > 0f) {
            stamRechargeTimer = STAMINA_USE_RECHARGE_DELAY;
            stamina -= amount;
            if(stamina < 0f) {
                stamina = 0f;
            }
        }
    }
}