using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public InputField roomNameInputField; // For custom room names
    public Button createRoomButton;
    public Button joinRoomButton;
    public TextMeshProUGUI errortxt;

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();

        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);

        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;

        errortxt.gameObject.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
    }

    public void CreateRoom()
    {
        string roomName = roomNameInputField.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "QuizRoom"; 
        }

        errortxt.gameObject.SetActive(false);

        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 4 });
    }

    public void JoinRoom()
    {
        string roomName = roomNameInputField.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "QuizRoom"; 
        }

        errortxt.gameObject.SetActive(false);

        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Successfully joined room.");
        PhotonNetwork.LoadLevel("QuizRoom"); 
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join Room Failed: " + message);
        errortxt.gameObject.SetActive(true);
        errortxt.text = "Room joining failed: " + message;

        if (message.Contains("Game does not exist"))
        {
            errortxt.text = "Room does not exist! Please check the room name.";
        }
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room created successfully.");
    }
}
