using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logic : MonoBehaviour {

    public GameObject winObject;

    public static bool won = false;

    // Update is called once per frame
    void Update () {
        if (PlayerSync.localPlayer != null && PlayerSync.otherPlayer != null) {
            if (PlayerSync.localPlayer.ownState.won && PlayerSync.otherPlayer.ownState.won) {
                won = true;
                winObject.SetActive(true);
                enabled = false;

                PlayerSync.otherPlayer.transform.SetParent(PlayerSync.localPlayer.GetComponent<OrbitalMovement>().refOrbiter.transform);
            }
        }
    }
}
