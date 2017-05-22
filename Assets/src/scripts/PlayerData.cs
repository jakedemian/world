using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour {
    public int health = 5;
    public float stamina = 100;

    public void useStamina(float amount) {
        stamina -= amount;
        if(stamina < 0f) {
            stamina = 0f;
        }
    }
}
