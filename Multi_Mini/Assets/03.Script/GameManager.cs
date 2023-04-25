using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    
    public Image bomb_CoolTime;
    public GameObject bombCoolTime;
    public Image shield_CoolTime;
    public GameObject shieldCoolTime;
    public Slider hp_Slider;

    ///////////////////////////////////////////////

    public Text roomName;
    public Text connectInfo;
    public Text msgList;

    public Button exitBtn;

    private void Awake()
    {
        // 출현 위치 정보를 배열에 생성
        Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();
        int idx = Random.Range(1, points.Length);

        // 네트워크상에 캐릭터 생성
        PhotonNetwork.Instantiate("Player", points[idx].position, points[idx].rotation, 0);

        SetRoomInfo();
        exitBtn.onClick.AddListener(() => OnExitClick());
    }

    // 룸 접속 정보를 출력
    void SetRoomInfo()
    {
        Room room = PhotonNetwork.CurrentRoom;
        roomName.text = room.Name;
        connectInfo.text = $"({room.PlayerCount}/{room.MaxPlayers})";
    }

    private void OnExitClick()
    {
        PhotonNetwork.LeaveRoom();
    }

    // 포톤 룸에서 퇴장했을 때 호출되는 콜백 함수
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Menu");
    }

    // 룸으로 새로운 네트워크 유저가 접속했을 때 호출되는 콜백 함수
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SetRoomInfo();
        string msg = $"\n<color=#00ff00>{newPlayer.NickName}</color> 참가!";
        msgList.text += msg;
    }

    // 룸에서 네트워크 유저가 퇴장했을 때 호출되는 콜백 함수
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        SetRoomInfo();
        string msg = $"\n<color=#ff0000>{otherPlayer.NickName}</color> 이탈!";
        msgList.text += msg;
    }
}
