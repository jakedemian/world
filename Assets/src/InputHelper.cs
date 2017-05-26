using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHelper {
    
    /// <summary>
    ///     Obtain the primary analog stick's current facing direction.
    /// </summary>
    /// <returns>Empty string if no primary direction, "up", "down", "left", or "right" otherwise.</returns>
    public static string getPrimaryAnalogStickDirection() {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        string dir = "";
        if(Mathf.Abs(h) > Mathf.Abs(v)) {
            // horizontal primary
            if(h > 0) {
                dir = "right";
            } else if(h < 0) {
                dir = "left";
            }
        } else if(Mathf.Abs(h) < Mathf.Abs(v)) {
            // vertical primary
            if(v > 0) {
                dir = "up";
            } else if(v < 0) {
                dir = "down";
            }
        }

        return dir;
    }

}
