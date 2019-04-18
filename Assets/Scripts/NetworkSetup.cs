﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;

public class NetworkSetup : MonoBehaviourPunCallbacks
{

    public int versionNumber = 0;
    public GameObject initialWholeScene;
    public GameObject loadingScene;
    public GameObject tutorialScene;
    [Tooltip("This prefab needs to be in the Resources folder")]
    public PlayerSync playerPrefab;
    public VoiceConnection voiceConnection;

    private void Start()
    {
        initialWholeScene.gameObject.SetActive(true);
        loadingScene.gameObject.SetActive(false);
        tutorialScene.gameObject.SetActive(true);
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = versionNumber.ToString();
        }
    }

    void ConnectNow()
    {
        Debug.Log("Connecting to server");
        loadingScene.gameObject.SetActive(true);
        tutorialScene.gameObject.SetActive(false);
        PhotonNetwork.JoinRandomRoom(null, 2);
    }

    void DisconnectNow() {
        Debug.Log("Leaving room");
        loadingScene.gameObject.SetActive(false);
        tutorialScene.gameObject.SetActive(true);
        if (PlayerSync.localPlayer != null && PlayerSync.localPlayer.gameObject != null) PhotonNetwork.Destroy(PlayerSync.localPlayer.gameObject);
        PhotonNetwork.LeaveRoom();
        joinedRoom = false;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Calling: PhotonNetwork.JoinRandomRoom();");
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby(). This client is connected and does get a room-list, which gets stored as PhotonNetwork.GetRoomList(). This script now calls: PhotonNetwork.JoinRandomRoom();");
        //Debug.Log(PhotonNetwork.IsConnected);
        //PhotonNetwork.JoinRandomRoom(null, 0);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(returnCode + " "+message);
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2 }, null);
    }

    // the following methods are implemented to give you some context. re-implement them as needed.
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("OnDisconnected(" + cause + ")");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room. From here on, your game would be running.");
        joinedRoom = true;
    }

    bool joinedRoom = false;
    bool init = false;

    void SetupScene()
    {
        Debug.Log("Setting Up!");

        if (voiceConnection.PrimaryRecorder == null)
        {
            voiceConnection.PrimaryRecorder = voiceConnection.gameObject.AddComponent<Recorder>();
        }
        voiceConnection.PrimaryRecorder.TransmitEnabled = true;
        voiceConnection.PrimaryRecorder.Init(voiceConnection.VoiceClient);

        initialWholeScene.gameObject.SetActive(false);
        PlayerSync localPlayer = PhotonNetwork.Instantiate(playerPrefab.gameObject.name, Vector3.zero, Quaternion.identity, 0).GetComponent<PlayerSync>();

        localPlayer.Init();
        init = true;
    }

    private void Update()
    {
        if (init)
        {
            if (PhotonNetwork.PlayerList.Length < 2)
            {
                init = false;
                DisconnectNow();
                Debug.Log("Reloading scene..");
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
        else {
            if (joinedRoom)
            {
                if (PhotonNetwork.PlayerList.Length > 1)
                {
                    SetupScene();
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    DisconnectNow();
                }
            }
            else {
                if (Input.GetKeyDown(KeyCode.Space) && PhotonNetwork.IsConnected)
                {
                    ConnectNow();
                }
            }
        }
    }
}
