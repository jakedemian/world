using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundController : MonoBehaviour {
    public List<AudioClip> snowFootsteps;

    private Transform playerTransform;

    public static float FOOTSTEP_TYPE_JOG = 0.35f;
    public static float FOOTSTEP_TYPE_SPRINT = 0.25f; 

    private float footstepTimer = 0f;
    private string footstepBlockTag = "";
    private float footstepType = 0f;
    private bool footsteps = false;

    public void startFootsteps(float type, string blockTag, Transform playerTrans) {
        footstepType = type;
        footstepBlockTag = blockTag;
        playerTransform = playerTrans;
        footsteps = true;
    }

    public void stopFootsteps() {
        footsteps = false;
        footstepTimer = 0f;
    }

    public void playJumpSound(string blockTag, Transform playerTrans) {
        footstepBlockTag = blockTag;
        playerTransform = playerTrans;
        playSingleFootstep();
    }

    public void playLandSound(string blockTag, Transform playerTrans) {
        footstepBlockTag = blockTag;
        playerTransform = playerTrans;
        playSingleFootstep();
    }

    void FixedUpdate() {
        if(footsteps) {
            footstepTimer -= Time.deltaTime;

            if(footstepTimer <= 0f) {
                // reset the timer and play a footstep
                footstepTimer = footstepType;
                playSingleFootstep();
            }
        }
    }
    
    void playSingleFootstep() {
        List<AudioClip> footstepSounds = new List<AudioClip>();

        if(footstepBlockTag.Equals("Snow")) {
            footstepSounds = snowFootsteps;
        } else {
            // no match, so just leave the function
            return;
        }

        int soundIdx = Random.Range(0, footstepSounds.Count);
        AudioSource.PlayClipAtPoint(footstepSounds[soundIdx], playerTransform.position);
    }
}
