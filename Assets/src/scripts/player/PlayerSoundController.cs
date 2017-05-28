using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundController : MonoBehaviour {
    public List<AudioClip> snowFootsteps;

    private Transform playerTransform;

    public static float FOOTSTEP_TYPE_JOG = 0.35f;
    public static float FOOTSTEP_TYPE_SPRINT = 0.28f; 

    private float footstepTimer = 0f;
    private string footstepBlockTag = "";
    private float footstepType = 0f;
    private bool footsteps = false;

    /// <summary>
    ///     FIXED UPDATE
    /// </summary>
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

    /// <summary>
    ///     Begin playing footstep sounds, if they are not already playing.
    /// </summary>
    /// <param name="type">Type of footstep (jog, sprint, etc)</param>
    /// <param name="blockTag">The tag of the block the character is colliding with.</param>
    /// <param name="playerTrans">The Transform of the character we are playing the sound for.</param>
    public void startFootsteps(float type, string blockTag, Transform playerTrans) {
        footstepType = type;
        footstepBlockTag = blockTag;
        playerTransform = playerTrans;
        footsteps = true;
    }

    /// <summary>
    ///     Stop the looping of footstep sounds.
    /// </summary>
    public void stopFootsteps() {
        footsteps = false;
        footstepTimer = 0f;
    }

    /// <summary>
    ///     Play a jump sound.
    /// </summary>
    /// <param name="blockTag">The tag of the block jumping from.</param>
    /// <param name="playerTrans">The Transform of the character that is jumping.</param>
    public void playJumpSound(string blockTag, Transform playerTrans) {
        footstepBlockTag = blockTag;
        playerTransform = playerTrans;
        playSingleFootstep();
    }

    /// <summary>
    ///     Play a landing sound.
    /// </summary>
    /// <param name="blockTag">The tag of the block we are landing on.</param>
    /// <param name="playerTrans">The Transform of the character that is landing.</param>
    public void playLandSound(string blockTag, Transform playerTrans) {
        footstepBlockTag = blockTag;
        playerTransform = playerTrans;
        playSingleFootstep();
    }

    /// <summary>
    ///     Play a single footstep sound.
    /// </summary>
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
