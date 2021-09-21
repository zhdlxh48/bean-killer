using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

using SimpleJSON;

[System.Serializable]
class RoomData {
    public string room = "";
}

public class GameSceneManager : MonoBehaviour {
    public string url = "http://mudev.cc:5000/";
    public GameObject room1, room2, room3, room4;
    public GameObject roomListIndexer;
    public GameObject roomListPrevBtn, roomListNextBtn;
    public GameObject createRoomEasy, createRoomNormal, createRoomHard;
    public int currentPage = 0;
    public int roomCount = 0;
    public int maxPage = 0;
    public JSONNode roomListRequestResult = null;

    private static readonly string nameBeanTestScene = "BeanTestScene";
    private static readonly string nameRoomSelectScene = "RoomSelectScene";
    private static readonly string startScene = "StartScene";

    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);
    }

    void Start() {
        UnityWebRequest.ClearCookieCache();
        OnRefreshBtnClick();
    }

    // UI EVENT RELATED METHODS
    public void OnCreateAndEnterClick() {
        createRoomEasy.SetActive(true);
        createRoomNormal.SetActive(true);
        createRoomHard.SetActive(true);
    }

    public void onCreateRoomEasy() {
        StartCoroutine(CreateAndEnterRoom(0));
    }

    public void onCreateRoomNormal() {
        StartCoroutine(CreateAndEnterRoom(1));
    }

    public void onCreateRoomHard() {
        StartCoroutine(CreateAndEnterRoom(2));
    }

    public void OnRefreshBtnClick() {
        StartCoroutine(HTTP_GetRoomList());
        currentPage = 0;
    }

    public void OnPrevBtnClicks() {
        if (currentPage <= 0)
            return;

        currentPage--;
        UpdateRoomListUI();
    }
    public void OnNextBtnClicks() {
        if (maxPage <= currentPage)
            return;

        currentPage++;
        UpdateRoomListUI();
    }

    // I HATE THESE BUTTON EVENT METHODS, THESE THINGS SEEMS TOO DIRTY TO ME.
    // by MUsoftware
    public void OnFirstRoomSelectionClick() {
        GameObject roomNameGO = room1.transform.Find("Room Name").gameObject;
        GameObject roomPeopleCountGO = room1.transform.Find("Room People Count").gameObject;
        GameObject roomEnterableGO = room1.transform.Find("Room Enterable").gameObject;
        string targetRoomID = roomNameGO.GetComponent<TextMeshProUGUI>().text;
        string targetRoomPeopleCount = roomPeopleCountGO.GetComponent<TextMeshProUGUI>().text;
        string targetRoomEnterable = roomEnterableGO.GetComponent<TextMeshProUGUI>().text;

        // Should check people count string,
        // because we use first room id field for showing status messages like
        // 네트워크 상태가 양호하지 않습니다 or 방이 없습니다.
        // If room id field is used for showing status messages,
        // then room enterable status text field must be blank.
        if (!String.IsNullOrEmpty(targetRoomEnterable)
         && String.Compare(targetRoomEnterable, "입장 가능!") == 0) {
            if (!String.IsNullOrEmpty(targetRoomID)) {
                Debug.Log("Entering room " + targetRoomID);
                StartCoroutine(EnterRoom(targetRoomID));
            }
        }
    }
    public void OnSecondRoomSelectionClick() {
        GameObject roomNameGO = room2.transform.Find("Room Name").gameObject;
        GameObject roomPeopleCountGO = room2.transform.Find("Room People Count").gameObject;
        GameObject roomEnterableGO = room2.transform.Find("Room Enterable").gameObject;
        string targetRoomID = roomNameGO.GetComponent<TextMeshProUGUI>().text;
        string targetRoomPeopleCount = roomPeopleCountGO.GetComponent<TextMeshProUGUI>().text;
        string targetRoomEnterable = roomEnterableGO.GetComponent<TextMeshProUGUI>().text;

        // Should check people count string,
        // because we use first room id field for showing status messages like
        // 네트워크 상태가 양호하지 않습니다 or 방이 없습니다.
        // If room id field is used for showing status messages,
        // then room enterable status text field must be blank.
        if (!String.IsNullOrEmpty(targetRoomEnterable)
         && String.Compare(targetRoomEnterable, "입장 가능!") == 0) {
            if (!String.IsNullOrEmpty(targetRoomID)) {
                Debug.Log("Entering room " + targetRoomID);
                StartCoroutine(EnterRoom(targetRoomID));
            }
        }
    }
    public void OnThirdRoomSelectionClick() {
        GameObject roomNameGO = room3.transform.Find("Room Name").gameObject;
        GameObject roomPeopleCountGO = room3.transform.Find("Room People Count").gameObject;
        GameObject roomEnterableGO = room3.transform.Find("Room Enterable").gameObject;
        string targetRoomID = roomNameGO.GetComponent<TextMeshProUGUI>().text;
        string targetRoomPeopleCount = roomPeopleCountGO.GetComponent<TextMeshProUGUI>().text;
        string targetRoomEnterable = roomEnterableGO.GetComponent<TextMeshProUGUI>().text;

        // Should check people count string,
        // because we use first room id field for showing status messages like
        // 네트워크 상태가 양호하지 않습니다 or 방이 없습니다.
        // If room id field is used for showing status messages,
        // then room enterable status text field must be blank.
        if (!String.IsNullOrEmpty(targetRoomEnterable)
         && String.Compare(targetRoomEnterable, "입장 가능!") == 0) {
            if (!String.IsNullOrEmpty(targetRoomID)) {
                Debug.Log("Entering room " + targetRoomID);
                StartCoroutine(EnterRoom(targetRoomID));
            }
        }
    }
    public void OnFourthRoomSelectionClick() {
        GameObject roomNameGO = room4.transform.Find("Room Name").gameObject;
        GameObject roomPeopleCountGO = room4.transform.Find("Room People Count").gameObject;
        GameObject roomEnterableGO = room4.transform.Find("Room Enterable").gameObject;
        string targetRoomID = roomNameGO.GetComponent<TextMeshProUGUI>().text;
        string targetRoomPeopleCount = roomPeopleCountGO.GetComponent<TextMeshProUGUI>().text;
        string targetRoomEnterable = roomEnterableGO.GetComponent<TextMeshProUGUI>().text;

        // Should check people count string,
        // because we use first room id field for showing status messages like
        // 네트워크 상태가 양호하지 않습니다 or 방이 없습니다.
        // If room id field is used for showing status messages,
        // then room enterable status text field must be blank.
        if (!String.IsNullOrEmpty(targetRoomEnterable)
         && String.Compare(targetRoomEnterable, "입장 가능!") == 0) {
            if (!String.IsNullOrEmpty(targetRoomID)) {
                Debug.Log("Entering room " + targetRoomID);
                StartCoroutine(EnterRoom(targetRoomID));
            }
        }
    }

    // UI UPDATE RELATED METHODS
    public void UpdateRoomListUI() {
        GameObject[] targetRoomUI = new GameObject[4]{
            room1, room2, room3, room4,
        };

        // Force change page to last if currentPage is more than max
        if (maxPage <= currentPage) currentPage = maxPage;

        if (roomCount == 0) {
            for (int z = 0; z <= 3; z++) {
                RoomLabelTextUpdater(
                    targetRoomUI[z],
                    (z == 0) ? "방이 하나도 없어요,\n방을 만들어주세요!" : "",
                    "", "", ""
                );
            }
        } else {
            int z = 0;
            for (z = 0; z <= 3; z++) {
                GameObject targetUI = targetRoomUI[z];
                int targetIndex = currentPage * 4 + z;
                if (targetIndex >= roomCount) {
                    RoomLabelTextUpdater(targetUI, "", "", "", "");
                } else {
                    string targetRoomID = roomListRequestResult["sort"][targetIndex];
                    JSONNode targetRoomData = roomListRequestResult["data"][targetRoomID];
                    int levelDifficultyInt = targetRoomData["difficulty"];
                    string levelDifficulty = "난이도 : ";
                    switch (levelDifficultyInt) {
                        case 0:
                            levelDifficulty += "쉬움";
                            break;
                        case 1:
                            levelDifficulty += "보통";
                            break;
                        case 2:
                            levelDifficulty += "어려움";
                            break;
                        default:
                            levelDifficulty += "보통";
                            break;
                    }

                    RoomLabelTextUpdater(
                        targetUI,
                        targetRoomData["id"],
                        targetRoomData["score"].Count.ToString(),
                        (targetRoomData["isPlaying"]
                          || targetRoomData["score"].Count >= 4
                          || targetRoomData["isGameOver"]) ?
                            "입장 불가" : "입장 가능!",
                        levelDifficulty
                    );
                }
            }
        }

        roomListNextBtn.GetComponent<Button>().interactable = !(currentPage >= maxPage || maxPage == 0);
        roomListPrevBtn.GetComponent<Button>().interactable = !(currentPage <= 0 || maxPage == 0);

        string roomListIndexString = "<" + (currentPage+1).ToString() + "/" + (maxPage+1).ToString() + ">";
        roomListIndexer.GetComponent<TextMeshProUGUI>().text = roomListIndexString;
    }

    void RoomLabelTextUpdater(GameObject targetGO, string name, string peopleCount, string enterable, string difficulty) {
        GameObject roomNameGO = targetGO.transform.Find("Room Name").gameObject;
        roomNameGO.GetComponent<TextMeshProUGUI>().text = name;

        GameObject roomPeopleCount = targetGO.transform.Find("Room People Count").gameObject;
        roomPeopleCount.GetComponent<TextMeshProUGUI>().text = peopleCount;

        GameObject roomEnterable = targetGO.transform.Find("Room Enterable").gameObject;
        roomEnterable.GetComponent<TextMeshProUGUI>().text = enterable;

        GameObject roomDifficulty = targetGO.transform.Find("Room Difficulty").gameObject;
        roomDifficulty.GetComponent<TextMeshProUGUI>().text = difficulty;
    }

    // ASYNCHRONOUS ROOM ENTERING RELATED HTTP METHODS
    IEnumerator EnterRoom(string targetRoom) {
        yield return null;
        HTTP_EnterRoom(targetRoom);
    }

    IEnumerator CreateAndEnterRoom(int level) {
        yield return null;
        HTTP_CreateAndEnterRoom(level);
    }

    // ROOM HANDLING RELATED HTTP METHODS
    IEnumerator HTTP_GetRoomList() {
        GameObject[] targetRoomUI = new GameObject[4]{
            room1, room2, room3, room4,
        };

        using (UnityWebRequest resp = UnityWebRequest.Get(url)) {
            yield return resp.SendWebRequest();

            if (resp.isNetworkError) {
                // error raised while communicating server
                for (int z = 0; z <= 3; z++) {
                    RoomLabelTextUpdater(
                        targetRoomUI[z],
                        (z == 0) ? "서버와 연결할 수 없습니다,\n인터넷 연결을 한번 더 확인해주세요!" : "",
                        "", "", ""
                    );
                }
            } else if (resp.isHttpError) {
                // Server responded error
                for (int z = 0; z <= 3; z++) {
                    RoomLabelTextUpdater(
                        targetRoomUI[z],
                        (z == 0) ? "목록을 가져오는 도중에 에러가 발생했습니다..." : "",
                        "", "", ""
                    );
                }
            } else {
                Debug.Log(resp.downloadHandler.text);
                roomListRequestResult = JSON.Parse(resp.downloadHandler.text);
                currentPage = 0;
                roomCount = roomListRequestResult["sort"].Count;
                maxPage = roomCount / 4;

                UpdateRoomListUI();
            }
        }
    }

    void HTTP_EnterRoom(string targetRoom) {
        GameObject[] targetRoomUI = new GameObject[4]{
            room1, room2, room3, room4,
        };

        RoomData targetData = new RoomData { room=targetRoom };
        string json = JsonUtility.ToJson(targetData);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

        var resp = new UnityWebRequest(url, "POST");
        resp.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        resp.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        resp.SetRequestHeader("Content-Type", "application/json");

        resp.SendWebRequest();

        if (resp.isNetworkError) {
            // error raised while communicating server
            for (int z = 0; z <= 3; z++) {
                RoomLabelTextUpdater(
                    targetRoomUI[z],
                    (z == 0) ? "서버와 연결할 수 없습니다,\n인터넷 연결을 한번 더 확인해주세요!" : "",
                    "", "", ""
                );
            }
        } else if (resp.isHttpError) {
            JSONNode respJson = JSON.Parse(resp.downloadHandler.text);
            // Server responded error
            for (int z = 0; z <= 3; z++) {
                RoomLabelTextUpdater(
                    targetRoomUI[z],
                    (z == 0) ? (string)respJson["message"] : "",
                    "", "", ""
                );
            }
        } else {
            Debug.Log(resp.downloadHandler.text);
            MoveBeanTestScene();
        }
    }

    void HTTP_CreateAndEnterRoom(int roomDifficulty) {
        GameObject[] targetRoomUI = new GameObject[4]{
            room1, room2, room3, room4,
        };

        string jsonStr = "{\"difficulty\" : " + roomDifficulty.ToString() + " }";
        byte[] dataToPut = System.Text.Encoding.UTF8.GetBytes(jsonStr);
        UnityWebRequest resp = UnityWebRequest.Put(url, dataToPut);
        resp.SendWebRequest();

        if (resp.isNetworkError) {
            // error raised while communicating server
            for (int z = 0; z <= 3; z++) {
                RoomLabelTextUpdater(
                    targetRoomUI[z],
                    (z == 0) ? "서버와 연결할 수 없습니다,\n인터넷 연결을 한번 더 확인해주세요!" : "",
                    "", "", ""
                );
            }
        } else if (resp.isHttpError) {
            JSONNode respJson = JSON.Parse(resp.downloadHandler.text);
            // Server responded error
            for (int z = 0; z <= 3; z++) {
                RoomLabelTextUpdater(
                    targetRoomUI[z],
                    (z == 0) ? (string)respJson["message"] : "",
                    "", "", ""
                );
            }
        } else {
            Debug.Log("Received: " + resp.downloadHandler.text);
            MoveBeanTestScene();
        }
    }

    // SCENE MOVEMENT RELATED METHODS
    public void MoveBeanTestScene() {
        StartCoroutine(beanTestSceneRoutine());
    }
    private IEnumerator beanTestSceneRoutine()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(nameBeanTestScene, LoadSceneMode.Single);

        operation.allowSceneActivation = true;
        while (!operation.isDone)
        {
            print(operation.progress);
            print(operation.isDone);
            yield return null;
        }

        if (operation.isDone)
        {
            print(operation.isDone);
            Destroy(gameObject);
        }
    }

    public void MoveStartScene()
    {
        StartCoroutine(moveRoomSceneRoutine());
    }
    private IEnumerator moveRoomSceneRoutine()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(startScene, LoadSceneMode.Single);

        operation.allowSceneActivation = true;
        while (!operation.isDone)
        {
            print(operation.progress);
            print(operation.isDone);
            yield return null;
        }

        if (operation.isDone)
        {
            print(operation.isDone);
            Destroy(gameObject);
        }
    }
}
