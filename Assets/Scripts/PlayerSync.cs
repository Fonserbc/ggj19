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

    float maxDeltaTime = Mathf.NegativeInfinity;
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

            float deltaTime = Mathf.Max(0, (float) ((PhotonNetwork.Time - info.SentServerTime)/1000d));
            maxDeltaTime = Mathf.Max(maxDeltaTime, deltaTime);
            //Debug.Log(deltaTime + "<= "+maxDeltaTime);
            lastOrbitPos = receivedState.currentOrbit.z + Mathf.Lerp(receivedState.currentSpeed, receivedState.speedGoal, 0.5f) * deltaTime;
        }
    }

    public void UpdateState()
    {
        if (isLocal)
        {
            ownState.currentOrbit.x = Mathf.Lerp(ownState.currentOrbit.x, ownState.orbitGoal.x, Time.deltaTime * localLerpFactor);
            ownState.currentOrbit.y = Mathf.Lerp(ownState.currentOrbit.y, ownState.orbitGoal.y, Time.deltaTime * localLerpFactor);

            float orbitFactor = Mathf.Lerp(1f, 0.3f, Mathf.Abs(Mathf.Sin((ownState.currentOrbit.y % 180f) * Mathf.Deg2Rad)));
            ownState.currentSpeed = Mathf.Lerp(ownState.currentSpeed, ownState.speedGoal, Time.deltaTime * localLerpFactor * orbitFactor);

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

            if (ownState.won || Logic.won) {
                dummyPlayerObject.transform.localPosition = Vector3.Slerp(dummyPlayerObject.transform.localPosition, new Vector3(0.8f, 0, 0), Time.deltaTime);
            }
            else {
                dummyPlayerObject.transform.localPosition = Vector3.Slerp(dummyPlayerObject.transform.localPosition, Vector3.zero, Time.deltaTime);
            }
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
