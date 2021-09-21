using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarController : MonoBehaviour
{
    public Image barImage;
    public TMP_Text barPercent;
    public TMP_Text changePercent;

    public GameObject gameOverIndicator;

    [SerializeField] private InGameNetworkManager networkManager;

    private static float barValue = 1.0f;
    private bool gameOverEventRaised = false;

    public static float BarValue
    {
        get => barValue;
        set => barValue = Mathf.Clamp(value, 0.0f, 1.0f);
    }

    public float reducePercentInSecond;
    public float reducePercentOnBreakCombo;
    public float healPercentOnGoingCombo;

    public float timer = 0.0f;

    private void Start()
    {
        BarInit();
    }

    private void Update()
    {
        if (GameManager.IsGameStart)
        {
            BarUpdate();
        }
    }

    public void BarInit()
    {
        barValue = 1.0f;
        barImage.fillAmount = BarValue;
        barPercent.text = String.Format("{0:0.00}", BarValue * 100.0f);
    }

    private void BarUpdate()
    {
        if (!gameOverEventRaised) {
            BarValue -= ConvertValueToPercent(reducePercentInSecond) * Time.deltaTime;
            barImage.fillAmount = BarValue;
            barPercent.text = String.Format("{0:0.00}", BarValue * 100.0f);
            
            timer += Time.deltaTime;

            if (BarValue <= 0.0f)
            {
                gameOverIndicator.SetActive(true);
                gameOverEventRaised = true;
                GameManager.IsGameOver = true;
                networkManager.OnGameOver();
                //print($"Ended Time : {timer}");
            }
        }
    }

    public void ReduceBarOnBreak()
    {
        BarValue -= ConvertValueToPercent(reducePercentOnBreakCombo);
        barImage.fillAmount = BarValue;
        ShowChangePercent(-reducePercentOnBreakCombo);
    }

    public void HealBarOnGoingCombo()
    {
        BarValue += ConvertValueToPercent(healPercentOnGoingCombo);
        barImage.fillAmount = BarValue;
        ShowChangePercent(healPercentOnGoingCombo);
    }

    private Color tempColor;
    public void ShowChangePercent(float value)
    {
        changePercent.text = $"{value}%";
        
        if (value < 0)
            tempColor = Color.red;
        else if (value > 0)
            tempColor = Color.green;
        else
            tempColor = Color.blue;
        
        changePercent.DOColor(tempColor, 0.3f).OnComplete(() =>
        {
            tempColor.a = 0.0f;
            changePercent.DOColor(tempColor, 0.8f);
        });
    }

    private float ConvertValueToPercent(float value) => value / 100.0f;
}
