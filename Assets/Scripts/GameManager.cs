using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CursorSystem cursorSystem;

    public static bool IsGameStart = false;
    public static bool IsGameOver = false;

    public static int gameDifficulty = 0;

    [SerializeField] private SettingData[] difficultySettings;
    private SettingData currentDifficulty;

    public BeanController beanController;
    public ForkController forkController;
    public BarController barController;
    public BGMManager bgmManager;

    private void Awake()
    {
        difficultySettings = new SettingData[3];
        for (int i = 0; i < difficultySettings.Length; i++)
        {
            difficultySettings[i] = new SettingData();
        }
        currentDifficulty = new SettingData();

        IsGameStart = false;
        IsGameOver = false;
        cursorSystem.InitCursor();
        print("GameManager Awake");
    }

    private void Start()
    {
        InitDifficulty();
        print("Rotation After Start : " + beanController.DebugFloorRotation);
        print("GameManager Start");
    }

    private void InitDifficulty()
    {
        // easy
        difficultySettings[0].beanMass = 3.2f;
        difficultySettings[0].beanCreateHeight = 12.0f;
        difficultySettings[0].beanFloorRotation = -17.0f;
        difficultySettings[0].beanImpulseForce = 24.5f;
        difficultySettings[0].beanCreateInSec = 5.0f;
        difficultySettings[0].forkClickIntervalTime = 0.2f;
        difficultySettings[0].barReducePercentInSecond = 3.0f;
        difficultySettings[0].barHealPercentOnGoingCombo = 3.0f;
        difficultySettings[0].barReducePercentOnBreakCombo = 0.0f;

        // normal (DEBUG)
        difficultySettings[1].beanMass = 3.2f;
        difficultySettings[1].beanCreateHeight = 12.0f;
        difficultySettings[1].beanFloorRotation = -25.0f;
        difficultySettings[1].beanImpulseForce = 32.5f;
        difficultySettings[1].beanCreateInSec = 4.0f;
        difficultySettings[1].forkClickIntervalTime = 0.2f;
        difficultySettings[1].barReducePercentInSecond = 4.0f;
        difficultySettings[1].barHealPercentOnGoingCombo = 3.0f;
        difficultySettings[1].barReducePercentOnBreakCombo = 5.0f;

        // hard (DEBUG)
        difficultySettings[2].beanMass = 3.2f;
        difficultySettings[2].beanCreateHeight = 12.0f;
        difficultySettings[2].beanFloorRotation = -33.0f;
        difficultySettings[2].beanImpulseForce = 52.5f;
        difficultySettings[2].beanCreateInSec = 3.0f;
        difficultySettings[2].forkClickIntervalTime = 0.4f;
        difficultySettings[2].barReducePercentInSecond = 5.0f;
        difficultySettings[2].barHealPercentOnGoingCombo = 2.5f;
        difficultySettings[2].barReducePercentOnBreakCombo = 10.0f;

        // default setting
        currentDifficulty = difficultySettings[0];
    }

    public void SetDifficulty(int i)
    {
        InitDifficulty();
        bgmManager.PlayMusic(i);
        
        currentDifficulty = difficultySettings[i];

        beanController.DebugBeanMass = currentDifficulty.beanMass;
        beanController.DebugFloorRotation = currentDifficulty.beanFloorRotation;
        beanController.DebugBeanCreateHeight = currentDifficulty.beanCreateHeight;
        beanController.DebugBeanImpulseForce = currentDifficulty.beanImpulseForce;
        beanController.DebugBeanCreateInSec = currentDifficulty.beanCreateInSec;

        forkController.DebugClickIntervalTime = currentDifficulty.forkClickIntervalTime;

        barController.reducePercentInSecond = currentDifficulty.barReducePercentInSecond;
        barController.healPercentOnGoingCombo = currentDifficulty.barHealPercentOnGoingCombo;
        barController.reducePercentOnBreakCombo = currentDifficulty.barReducePercentOnBreakCombo;

        print("Rotation : " + beanController.DebugFloorRotation);
        Debug.Log("Successfully changed difficulty to " + i.ToString());
    }
}

[Serializable]
public class SettingData
{
    public float forkClickIntervalTime;
    public float beanCreateHeight;
    public float beanFloorRotation;
    public float beanCreateInSec;
    public float beanImpulseForce;
    public float beanMass;
    public float barReducePercentInSecond;
    public float barReducePercentOnBreakCombo;
    public float barHealPercentOnGoingCombo;
}
