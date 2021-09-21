using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    private static readonly string nameBeanTestScene = "BeanTestScene";
    private static readonly string nameRoomSelectScene = "RoomSelectScene";
    private static readonly string startScene = "StartScene";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // SCENE MOVEMENT RELATED METHODS
    public void MoveBeanTestScene()
    {
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
    
    public void MoveRoomSelectScene()
    {
        StartCoroutine(moveRoomSceneRoutine());
    }
    private IEnumerator moveRoomSceneRoutine()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(nameRoomSelectScene, LoadSceneMode.Single);

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

    public void GameExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }
}
