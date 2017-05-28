using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDirections {
    public bool right = false;
    public bool left = false;
    public bool up = false;
    public bool down = false;

    public GameObject rightCollisionObj;
    public GameObject leftCollisionObj;
    public GameObject upCollisionObj;
    public GameObject downCollisionObj;

    /// <summary>
    ///     Reset all collision information to the default state.
    /// </summary>
    public void clear() {
        right = false;
        left = false;
        up = false;
        down = false;

        rightCollisionObj = null;
        leftCollisionObj = null;
        upCollisionObj = null;
        downCollisionObj = null;
    }
}
