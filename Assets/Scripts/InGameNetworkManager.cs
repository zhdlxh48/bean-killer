using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

using socket.io;
using SimpleJSON;

public class InGameNetworkManager : MonoBehaviour {
    public string socketServerURL = "http://mudev.cc:5000/play";
    public string userInfoURL = "http://mudev.cc:5000/whoami";

    public string userID = "", roomID = "", userColor = "";

    public GameObject inGameUI, resultUI;

    public GameObject roomIDGO;
    public GameObject playerProfile1, playerProfile2, playerProfile3;
    public GameObject playerScore1, playerScore2, playerScore3;
    public GameObject readyToPlayBtn;

    public GameObject gameOverIndicator;

    public SpriteRenderer myProfileImage;

    private GameObject[] playerProfileUIs;
    private GameObject[] playerScoreUIs;

    public GameObject playerResultProfile1, playerResultProfile2, playerResultProfile3, playerResultProfile4;
    public TextMeshProUGUI playerResultText1, playerResultText2, playerResultText3, playerResultText4;

    public Image myResultImage;

    private GameObject[] playerResultProfileUIs;
    private TextMeshProUGUI[] playerResultTextUIs;

    public int currentUserInRoom = 0;
    public Hashtable playerProfileGOIndexTable = new Hashtable();
    public Socket socket;

    public IEnumerator beanSpawnCoroutine;

    [SerializeField] public BeanController beanController;
    [SerializeField] public BarController barController;
    [SerializeField] public GameManager gameManager;

    public GameObject BGMManager;

    void Start() {
        BGMManager.SetActive(true);
        playerProfileUIs = new GameObject[3]{
            playerProfile1, playerProfile2, playerProfile3,
        };
        playerScoreUIs = new GameObject[3]{
            playerScore1, playerScore2, playerScore3,
        };

        playerResultProfileUIs = new GameObject[4]{
            playerResultProfile1, playerResultProfile2, playerResultProfile3, playerResultProfile4
        };
        playerResultTextUIs = new TextMeshProUGUI[4]{
            playerResultText1, playerResultText2, playerResultText3, playerResultText4
        };

        for (int z = 0; z <= 2; z++) {
            playerProfileUIs[z].SetActive(false);
            GameObject targetGameOverGO = playerProfileUIs[z].transform.Find("GameOver").gameObject;
            targetGameOverGO.SetActive(false);
        }

        StartCoroutine(ConnectToServer());
    }

    void Update() {
    }

    private int GetUnusedPlayerProfileIndex() {
        for (int z = 0; z <= 2; z++) {
            if (!playerProfileUIs[z].activeSelf)
                return z;
        }
        return -1;
    }

    IEnumerator ConnectToServer() {
        yield return null;
        GetUserInfo();

        socket = Socket.Connect(socketServerURL);
        SocketManager.Instance.Reconnection = false;

        socket.On(SystemEvents.connect, () => {
            Debug.Log("SOCKETIO/ROOM CONNECTED");
        });

        socket.On(SystemEvents.reconnect, (int reconnectAttempt) => {
            Debug.Log("SOCKETIO/ROOM RE-CONNECTED COUNT:" + reconnectAttempt);
        });

        socket.On(SystemEvents.disconnect, () => {
            Debug.Log("SOCKETIO/ROOM DISCONNECTED");
            Debug.Log("SOCKETIO END");
        });
        socket.On("forcedisconnect", (string data) => {
            Debug.Log("SOCKETIO/ROOM FORCE DISCONNECTED");
            OnSceneChangeToRoomSelect();
        });

        socket.On(SystemEvents.connectTimeOut , () => {
            Debug.Log("Socket.io Connection Timeout");
        });
        socket.On(SystemEvents.connectError , (Exception e) => {
            Debug.Log("Socket.io Connection Error: " + e.ToString());
        });
        socket.On(SystemEvents.reconnectError , (Exception e) => {
            Debug.Log("Socket.io ReConnection Error: " + e.ToString());
        });


        socket.On("playerenter", (string data) => {
            Debug.Log("SOCKETIO/PLAYER ENTER");
            Debug.Log(data);
            JSONNode recvData = JSON.Parse(data);

            // 만약 방 접속자가 자기 자신이면 무시합니다.
            if (String.Compare(recvData["user"], userID) == 0) return;

            // 만약 방 접속자가 현재 플레이어 포함 4명 이상이면 무시합니다.
            if (currentUserInRoom >= 3) return;

            playerProfileGOIndexTable[recvData["user"]] = GetUnusedPlayerProfileIndex();
            int targetIndex = (int)playerProfileGOIndexTable[recvData["user"]];

            playerProfileUIs[targetIndex].SetActive(true);
            GameObject targetUserProfileImageGO = playerProfileUIs[targetIndex]
                                                      .transform.Find("PlayerAvatar").gameObject;
            Image targetUserProfileImage = targetUserProfileImageGO.GetComponent<Image>();

            Color tmpColor;
            if (ColorUtility.TryParseHtmlString(recvData["data"]["color"], out tmpColor))
                targetUserProfileImage.color = tmpColor;

            if (recvData["data"]["playStartAgreed"])
                playerScoreUIs[targetIndex].GetComponent<TextMeshProUGUI>().text = "준비 완료!";
            currentUserInRoom++;
        });
        socket.On("playerleave", (string data) => {
            Debug.Log("SOCKETIO/PLAYER LEAVE");
            Debug.Log(data);
            JSONNode recvData = JSON.Parse(data);

            // 만약 방 접속자가 현재 플레이어를 제외하고 아무도 없으면 무시합니다.
            if (currentUserInRoom <= 0) return;

            int targetIndex = (int)playerProfileGOIndexTable[recvData["user"]];
            playerProfileGOIndexTable.Remove(recvData["user"]);

            if (targetIndex >= 0) {
                playerProfileUIs[targetIndex].SetActive(false);
                playerScoreUIs[targetIndex].GetComponent<TextMeshProUGUI>().text = "기다리는 중...";

                currentUserInRoom--;
            }
        });


        socket.On("playergetscore", (string data) => {
            Debug.Log("SOCKETIO/PLAYER SCORE UPDATE");
            Debug.Log(data);
            JSONNode recvData = JSON.Parse(data);

            // 만약 점수 획득자가 자기 자신이면 무시합니다.
            if (String.Compare(recvData["user"], userID) == 0) return;

            int targetIndex = (int)playerProfileGOIndexTable[recvData["user"]];
            playerScoreUIs[targetIndex].GetComponent<TextMeshProUGUI>().text = recvData["data"]["score"] + " 점";
        });
        socket.On("playergameover", (string data) => {
            Debug.Log("SOCKETIO/PLAYER GAME OVER");
            Debug.Log(data);
            JSONNode recvData = JSON.Parse(data);

            if (String.Compare(recvData["user"], userID) == 0) {
                return;
            }

            int targetIndex = (int)playerProfileGOIndexTable[recvData["user"]];
            GameObject targetGameOverGO = playerProfileUIs[targetIndex].transform.Find("GameOver").gameObject;
            targetGameOverGO.SetActive(true);
        });
        socket.On("roomgameover", (string data) => {
            Debug.Log("SOCKETIO/ALL PLAYER'S GAME IN THE ROOM OVER");
            Debug.Log(data);
            JSONNode recvData = JSON.Parse(data);

            for (int z = 0; z < recvData["sort"].Count; z++) {
                playerResultProfileUIs[z].SetActive(true);
                string playerName = recvData["sort"][z];
                string color = recvData["data"][playerName]["color"];
                string score = recvData["data"][playerName]["score"];

                string resultScoreText = (z+1).ToString() + "등 : " + score + "점";
                if (String.Compare(playerName, userID) == 0)
                    resultScoreText += " (나)";
                playerResultTextUIs[z].text = resultScoreText;
                Image targetProfileImage = playerResultProfileUIs[z].transform.Find("ProfileImage").GetComponent<Image>();

                Color tmpColor;
                if (ColorUtility.TryParseHtmlString(color, out tmpColor))
                    targetProfileImage.color = tmpColor;
            }

            BGMManager.SetActive(false);
            gameOverIndicator.SetActive(false);
            inGameUI.SetActive(false);
            resultUI.SetActive(true);

            // TODO Do something when all players' game in room is over
        });


        socket.On("playerstartagree", (string data) => {
            Debug.Log("SOCKETIO/PLAYER AGREED TO START");
            Debug.Log(data);
            JSONNode recvData = JSON.Parse(data);

            if (String.Compare(recvData["user"], userID) == 0) return;

            int targetIndex = (int)playerProfileGOIndexTable[recvData["user"]];
            playerScoreUIs[targetIndex].GetComponent<TextMeshProUGUI>().text = "준비 완료!";
        });
        socket.On("roomstarted", (string data) => {
            Debug.Log("SOCKETIO/ROOM GAME START(ALL PLAYERS IN ROOM AGREED TO START)");
            Debug.Log(data);
            JSONNode recvData = JSON.Parse(data);

            for (int z = 0; z <= 2; z++) {
                if (playerProfileUIs[z].activeSelf)
                    playerScoreUIs[z].GetComponent<TextMeshProUGUI>().text = "0 점";
            }

            GameManager.IsGameStart = true;
            beanSpawnCoroutine = beanController.BeanSpawn();
            StartCoroutine(beanSpawnCoroutine);
        });

        roomIDGO.GetComponent<TextMeshProUGUI>().text = "방 ID : " + roomID;

        Color myColor;
        if (ColorUtility.TryParseHtmlString(userColor, out myColor))
            myProfileImage.color = myColor;
    }

    void GetUserInfo() {
        UnityWebRequest resp = UnityWebRequest.Get(userInfoURL);
        resp.SendWebRequest();

        // I hate this code, but had to because
        // this network request should be block next jobs
        while (resp.downloadHandler.text == null
            || resp.downloadHandler.text == "");

        if (resp.isNetworkError) {
            // TODO do message popup!
            // TODO get out this room!
            Debug.Log("Failed to get my data - Network");
        } else if (resp.isHttpError) {
            // TODO get out this room!
            Debug.Log("Failed to get my data - Server");
        } else {
            Debug.Log(resp.downloadHandler.text);
            JSONNode respJson = JSON.Parse(resp.downloadHandler.text);
            userID = respJson["username"];
            roomID = respJson["room"];
            userColor = respJson["color"];

            GameManager.gameDifficulty = respJson["difficulty"];
            gameManager.SetDifficulty(respJson["difficulty"]);
        }
    }


    public void OnReadyToPlayClick() {
        socket.Emit("playerstartagree", "");
        readyToPlayBtn.SetActive(false);
    }

    public void BroadcastScore(int score) {
        string scoreJson = "{\\\"data\\\": { \\\"score\\\": "+ score.ToString() +" }}";
        Debug.Log(scoreJson);
        socket.Emit("playergetscore", scoreJson);
    }
    public void OnGameOver() {
        string msgJson = "DUMMY";
        socket.Emit("playergameover", msgJson);
    }

    public void OnSceneChangeToRoomSelect() {
        gameOverIndicator.SetActive(false);

        Socket.Disconnect(socket);

        foreach (Transform child in SocketManager.Instance.transform) {
            Debug.Log("REMOVED GO - " + child.gameObject.name);
            GameObject.Destroy(child.gameObject);
        }
        GameObject.Destroy(GameObject.Find(string.Format("socket.io - {0}", socketServerURL)));
        // GameObject.Destroy(SocketManager.Instance);
        // SocketManager._instance = null;

        SceneManager.LoadScene("RoomSelectScene", LoadSceneMode.Single);
    }
}
