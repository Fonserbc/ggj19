using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class PlayerSync : MonoBehaviour
{

    public static PlayerSync localPlayer, otherPlayer;

    public struct PlayerSate {
        public Vector3 currentOrbit;
        public Vector2 orbitGoal;
        public float speed;
    }

    PhotonView ownView;

    public PlayerSate ownState;
    PlayerSate receivedState;

    // references
    public GameSettings settings;
    public GameObject localPlayerObject;
    public GameObject dummyPlayerObject;
    float lastOrbitPos;

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
            stream.SendNext(ownState.orbitGoal);
            stream.SendNext(ownState.currentOrbit);
            stream.SendNext(ownState.speed);
        }
        else {
            receivedState.orbitGoal = (Vector2)stream.ReceiveNext();
            receivedState.currentOrbit = (Vector3)stream.ReceiveNext();
            receivedState.speed = (float)stream.ReceiveNext();

            double deltaTime = (PhotonNetwork.Time - info.SentServerTime)/1000d;
            lastOrbitPos = receivedState.currentOrbit.z + receivedState.speed * (float)deltaTime;
        }
    }

    void Update()
    {
        if (isLocal)
        {

        }
        else
        {
            // Lerp state here (?)
            ownState.currentOrbit.x = Mathf.Lerp(ownState.currentOrbit.x, ownState.orbitGoal.x, Time.deltaTime * 4f);
            ownState.currentOrbit.y = Mathf.Lerp(ownState.currentOrbit.y, ownState.orbitGoal.y, Time.deltaTime * 4f);

            ownState.speed = Mathf.Lerp(ownState.speed, receivedState.speed, Time.deltaTime * 4f);
            lastOrbitPos += Time.deltaTime * receivedState.speed;
            ownState.currentOrbit.z = Mathf.Lerp(ownState.currentOrbit.z + ownState.speed * Time.deltaTime, lastOrbitPos, Time.deltaTime * 4f);
        }
    }
}
