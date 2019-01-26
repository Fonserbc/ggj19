using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class PlayerSync : MonoBehaviour, IPunObservable
{

    public static PlayerSync localPlayer, otherPlayer;

    public struct PlayerSate {
        public Vector3 currentOrbit;
        public Vector2 orbitGoal;
        public float currentSpeed;
        public float speedGoal;
        public bool won;
    }

    PhotonView ownView;

    public float localLerpFactor = 3f;

    public PlayerSate ownState;
    PlayerSate receivedState;

    // references
    public GameSettings settings;
    public GameObject localPlayerObject;
    public GameObject dummyPlayerObject;
    public AudioController audioController;
    float lastOrbitPos;
    [HideInInspector]
    public int playerID = 0;

    [HideInInspector]
    public bool isLocal = false;

    bool initialized = false;

    private void Start()
    {
        if (!initialized) Init();
    }

    public void Init() {
        ownView = GetComponent<PhotonView>();

        if (ownView.IsMine) {
            localPlayer = this;
            isLocal = true;
            audioController.enabled = true;
        }
        else
        {
            otherPlayer = this;
            isLocal = false;
        }

        playerID = PhotonNetwork.LocalPlayer.ActorNumber;

        localPlayerObject.SetActive(isLocal);
        dummyPlayerObject.SetActive(!isLocal);

        initialized = true;
        Debug.Log("Player initialized!");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(ownState.orbitGoal);
            stream.SendNext(ownState.currentOrbit);
            stream.SendNext(ownState.currentSpeed);
            stream.SendNext(ownState.speedGoal);
            stream.SendNext(ownState.won);
        }
        else {
            receivedState.orbitGoal = (Vector2)stream.ReceiveNext();
            receivedState.currentOrbit = (Vector3)stream.ReceiveNext();
            receivedState.currentSpeed = (float)stream.ReceiveNext();
            receivedState.speedGoal = (float)stream.ReceiveNext();
            ownState.won = receivedState.won = (bool)stream.ReceiveNext();

            double deltaTime = (PhotonNetwork.Time - info.SentServerTime)/1000d;
            lastOrbitPos = receivedState.currentOrbit.z + Mathf.Lerp(receivedState.currentSpeed, receivedState.speedGoal, 0.5f) * (float)deltaTime;
        }
    }

    public void UpdateState()
    {
        if (isLocal)
        {
            ownState.currentOrbit.x = Mathf.Lerp(ownState.currentOrbit.x, ownState.orbitGoal.x, Time.deltaTime * localLerpFactor);
            ownState.currentOrbit.y = Mathf.Lerp(ownState.currentOrbit.y, ownState.orbitGoal.y, Time.deltaTime * localLerpFactor);
            ownState.currentSpeed = Mathf.Lerp(ownState.currentSpeed, ownState.speedGoal, Time.deltaTime * localLerpFactor);

            ownState.currentOrbit.z += ownState.currentSpeed * Time.deltaTime;
        }
        else
        {
            // Lerp state here (?)
            ownState.currentOrbit.x = Mathf.Lerp(ownState.currentOrbit.x, receivedState.orbitGoal.x, Time.deltaTime * 4f);
            ownState.currentOrbit.y = Mathf.Lerp(ownState.currentOrbit.y, receivedState.orbitGoal.y, Time.deltaTime * 4f);

            ownState.currentSpeed = Mathf.Lerp(ownState.currentSpeed, receivedState.speedGoal, Time.deltaTime * 4f);
            receivedState.currentSpeed = Mathf.Lerp(receivedState.currentSpeed, receivedState.speedGoal, Time.deltaTime * 4f);
            lastOrbitPos += Time.deltaTime * receivedState.currentSpeed;
            ownState.currentOrbit.z = Mathf.Lerp(ownState.currentOrbit.z + ownState.currentSpeed * Time.deltaTime, lastOrbitPos, Time.deltaTime * 4f);
        }
    }

    private void OnDestroy()
    {
        if (localPlayer == this) {
            localPlayer = null;
        }
        if (otherPlayer == this) {
            otherPlayer = null;
        }
    }
}
