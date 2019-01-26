using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class PlayerSync : MonoBehaviour
{

    public static PlayerSync localPlayer, otherPlayer;

    public struct PlayerSate {
        public int orbit;
    }

    PhotonView ownView;

    public PlayerSate ownState;
    PlayerSate receivedState;

    // references
    public GameObject localPlayerObject;
    public GameObject dummyPlayerObject;

    [HideInInspector]
    public bool isLocal = false;

    public void Init() {
        ownView = GetComponent<PhotonView>();

        if (ownView.IsMine) {
            localPlayer = this;
            isLocal = true;
        }
        else
        {
            otherPlayer = this;
            isLocal = false;
        }

        // Debug
        if (PhotonNetwork.LocalPlayer.ActorNumber == 0)
        {
            transform.position = new Vector3(0, 0, 0);
        }
        else
        {
            transform.position = new Vector3(2, 0, 0);
        }
        //

        localPlayerObject.SetActive(isLocal);
        dummyPlayerObject.SetActive(!isLocal);

        Debug.Log("Player initialized!");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(ownState.orbit);
        }
        else {
            receivedState.orbit = (int)stream.ReceiveNext();
        }
    }

    void Update()
    {
        // Lerp state here (?)
        ownState.orbit = receivedState.orbit;
    }
}
