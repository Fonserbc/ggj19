using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Voice.Unity;
using Photon.Realtime;

[RequireComponent(typeof(VoiceConnection))]
public class VoiceSetup : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks
{
    private string roomName = "ggj19";
    private RoomOptions roomOptions;
    private TypedLobby typedLobby = TypedLobby.Default;

    VoiceConnection voiceConnection;

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
        Debug.Log("Joined room!");
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Joined room failed " + message);
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
            new EnterRoomParams
            {
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

}
