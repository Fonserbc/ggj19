using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Voice.Unity;
using Photon.Realtime;
using Photon.Pun;

[RequireComponent(typeof(VoiceConnection))]
public class NetworkSetup : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks
{

    public int versionNumber = 0;
    public GameObject loadingScene;
    [Tooltip("This prefab needs to be in the Resources folder")]
    public PlayerSync playerPrefab;

    private string roomName = "ggj19"; 
    private RoomOptions roomOptions;
    private TypedLobby typedLobby = TypedLobby.Default;

    VoiceConnection voiceConnection;

    void OnEnable()
    {
        if (voiceConnection == null)
        {
            voiceConnection = GetComponent<VoiceConnection>();
        }
        voiceConnection.Client.AddCallbackTarget(this);
        ConnectNow();
    }

    void OnDisable()
    {
        voiceConnection.Client.RemoveCallbackTarget(this);
    }

    void ConnectNow()
    {
        Debug.Log("Connecting to server");
        roomOptions = new RoomOptions
        {
            MaxPlayers = 2
        };

        voiceConnection.ConnectUsingSettings();
        voiceConnection.Client.AppVersion = versionNumber.ToString();
    }

#region MatchmakingCallbacks

    public void OnCreatedRoom()
    {

    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {

    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {

    }

    public void OnJoinedRoom()
    {
        SetupScene();
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        if (returnCode == ErrorCode.NoRandomMatchFound)
        {
            voiceConnection.Client.OpCreateRoom(new EnterRoomParams
            {
                RoomName = roomName,
                RoomOptions = roomOptions,
                Lobby = typedLobby
            });
        }
        else
        {
            Debug.LogErrorFormat("OnJoinRandomFailed errorCode={0} errorMessage={1}", returnCode, message);
        }
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogErrorFormat("OnJoinRoomFailed roomName={0} errorCode={1} errorMessage={2}", roomName, returnCode, message);
    }

    public void OnLeftRoom()
    {

    }

#endregion

#region ConnectionCallbacks

    public void OnConnected()
    {

    }

    public void OnConnectedToMaster()
    {
        voiceConnection.Client.OpJoinOrCreateRoom(
            new EnterRoomParams {
                RoomName = roomName,
                RoomOptions = roomOptions,
                Lobby = typedLobby
        });
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        if (cause == DisconnectCause.None || cause == DisconnectCause.DisconnectByClientLogic)
        {
            return;
        }
        Debug.LogErrorFormat("OnDisconnected cause={0}", cause);
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {

    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {

    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {

    }

    #endregion

    void SetupScene()
    {
        if (voiceConnection.PrimaryRecorder == null)
        {
            voiceConnection.PrimaryRecorder = this.gameObject.AddComponent<Recorder>();
        }
        voiceConnection.PrimaryRecorder.TransmitEnabled = true;
        voiceConnection.PrimaryRecorder.Init(voiceConnection.VoiceClient);

        loadingScene.SetActive(false);
        PlayerSync localPlayer = PhotonNetwork.Instantiate(playerPrefab.gameObject.name, Vector3.zero, Quaternion.identity, 0).GetComponent<PlayerSync>();

        localPlayer.Init();
    }
}
