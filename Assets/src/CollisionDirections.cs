using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDirections {
    public bool right = false;
    public bool left = false;
    public bool up = false;
    public bool down = false;

    public GameObject rightCollision;
    public GameObject leftCollision;
    public GameObject upCollision;
    public GameObject downCollision;

    public void clear() {
        right = false;
        left = false;
        up = false;
        down = false;

        rightCollision = null;
        leftCollision = null;
        upCollision = null;
        downCollision = null;
    }
}
