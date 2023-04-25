using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomData : MonoBehaviour
{
    private RoomInfo _roomInfo;
    private Text roomInfoText;
    private PhotonManager photonManager;

    // 프로퍼티 정의
    public RoomInfo RoomInfo
    {
        get
        {
            return _roomInfo;
        }
        set
        {
            _roomInfo = value;
            // 룸 정보 표시
            roomInfoText.text = $"{_roomInfo.Name} ({_roomInfo.PlayerCount} / {_roomInfo.MaxPlayers})";
            // 버튼 클릭 이벤트에 함수 연결
            GetComponent<Button>().onClick.AddListener(() => OnEnterRoom(_roomInfo.Name));            
        }
    }

    private void Awake()
    {        
        roomInfoText = GetComponentInChildren<Text>();
        photonManager = GameObject.Find("PhotonManager").GetComponent<PhotonManager>();
    }

    void OnEnterRoom(string roomName)
    {
        Debug.Log("야 들어온다!!!");
        // 유저명 설정
        photonManager.SetUserId();

        // 룸의 속성 정의
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 20;
        ro.IsOpen = true;
        ro.IsVisible = true;

        // 룸 접속
        PhotonNetwork.JoinOrCreateRoom(roomName, ro, TypedLobby.Default);
    }
}
