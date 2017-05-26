﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnimationObjectController : MonoBehaviour {

	/// <summary>
    ///     START
    /// </summary>
    void Start () {
        Destroy(gameObject, this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
    }
}
