using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnimationObjectController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Destroy(gameObject, this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
    }
}
