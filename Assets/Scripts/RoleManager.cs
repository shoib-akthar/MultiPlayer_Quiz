using Photon.Pun;
using ExitGames.Client.Photon;

public class RoleManager : MonoBehaviourPunCallbacks
{
    public override void OnJoinedRoom()
    {
        AssignRole();
    }

    void AssignRole()
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        
        if (playerCount <= 2)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "Role", "Player" } });
        }
        else
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "Role", "Spectator" } });
        }
    }

    public string GetPlayerRole()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Role"))
        {
            return PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString();
        }
        return "Player"; // Default role if not set
    }
}
