using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkController : MonoBehaviour {
    public GameObject chunkContainer;

    private GameObject[] allChunks;
    private GameObject player;
    private PlayerInputController playerMovement;

    private const float CHUNK_LOAD_DISTANCE = 40f;

    /// <summary>
    ///     START
    /// </summary>
    void Start() {
        // get the player
        player = GameObject.FindWithTag("Player");
        if(player != null) {
            playerMovement = player.GetComponent<PlayerInputController>();
        } else {
            Debug.LogError("Unable to find the player.  ChunkController will not work.");
        }

        // load all chunks
        Component[] children = chunkContainer.GetComponentsInChildren(typeof(Transform), true);
        List<GameObject> tempChunks = new List<GameObject>();
        foreach(Transform t in children) {
            if(t.gameObject.tag == "Chunk") {
                tempChunks.Add(t.gameObject);
            }
        }
        allChunks = tempChunks.ToArray();
    }
	
	/// <summary>
    ///     UPDATE
    /// </summary>
    void Update () {
		for(int i = 0; i < allChunks.Length; i++) {
            GameObject c = allChunks[i];
            float distanceFromPlayer = Utilities.getDistanceBetweenTwoPoints(player.transform.position, c.transform.position);

            bool chunkActive = distanceFromPlayer <= CHUNK_LOAD_DISTANCE ? true : false;
            c.SetActive(chunkActive);
        }
    }
}
