using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameDebugger : MonoBehaviour
{
    public ForkController forkController;
    public BeanController beanController;
    public BarController barController;
    public InGameUIManager uiManager;

    public TMP_Text ForkPickTimeIntervalPlaceholder;
    public TMP_Text BeanBounceFloorRotationPlaceholder;
    public TMP_Text BeanCreateHeightPlaceholder;
    public TMP_Text BeanCreateInSecPlaceholder;
    public TMP_Text BeanImpulsePlaceholder;
    public TMP_Text BeanMassPlaceholder;
    public TMP_Text BarReducePercentPlaceholder;
    public TMP_Text BarReduceOnBreakComboPlaceholder;
    public TMP_Text BarHealOnGoingComboPlaceholder;
    
    public TMP_InputField ForkPickTimeIntervalText;
    public TMP_InputField BeanBounceFloorRotationText;
    public TMP_InputField BeanCreateHeightText;
    public TMP_InputField BeanCreateInSecText;
    public TMP_InputField BeanImpulseText;
    public TMP_InputField BeanMassText;
    public TMP_InputField BarReducePercentText;
    public TMP_InputField BarReduceOnBreakComboText;
    public TMP_InputField BarHealOnGoingComboText;

    public TMP_Text highestComboText;

    public TMP_Text timerText;

    public Toggle repeatToggle;

    private void Start()
    {
        InitializePlaceholder();
        SaveOriginSettings();
    }

    private void Update()
    {
        UpdateHighestComboValue();

        if (GameManager.IsGameOver)
        {
            if (repeatToggle.isOn)
            {
                GameManager.IsGameOver = false;
                BarController.BarValue = 1.0f;
                barController.timer = 0.0f;
            }
        }

        TimerLogic();
    }

    private float timer;
    private bool timerOn;
    private void TimerLogic()
    {
        if (timerOn)
        {
            timer += Time.deltaTime;
            timerText.text = $"{(int)(timer / 60.0f)}min {(int)(timer % 60.0f)}sec";
        }
    }

    public void TimerRestart()
    {
        timer = 0.0f;
        timerOn = true;
    }

    public void TimerStop()
    {
        timerOn = false;
    }

    private void InitializePlaceholder()
    {
        ForkPickTimeIntervalPlaceholder.text = forkController.DebugClickIntervalTime.ToString();

        BeanBounceFloorRotationPlaceholder.text = beanController.DebugFloorRotation.ToString();
        BeanCreateHeightPlaceholder.text = beanController.DebugBeanCreateHeight.ToString();
        BeanCreateInSecPlaceholder.text = beanController.DebugBeanCreateInSec.ToString();
        BeanImpulsePlaceholder.text = beanController.DebugBeanImpulseForce.ToString();
        BeanMassPlaceholder.text = beanController.DebugBeanMass.ToString();
        
        BarReducePercentPlaceholder.text = barController.reducePercentInSecond.ToString() + "%";
        BarReduceOnBreakComboPlaceholder.text = barController.reducePercentOnBreakCombo.ToString() + "%";
        BarHealOnGoingComboPlaceholder.text = barController.healPercentOnGoingCombo.ToString() + "%";
    }

    [SerializeField] private SettingData originData = new SettingData();
    private void SaveOriginSettings()
    {
        originData.forkClickIntervalTime = forkController.DebugClickIntervalTime;
        originData.beanFloorRotation = beanController.DebugFloorRotation;
        originData.beanCreateHeight = beanController.DebugBeanCreateHeight;
        originData.beanCreateInSec = beanController.DebugBeanCreateInSec;
        originData.beanImpulseForce = beanController.DebugBeanImpulseForce;
        originData.beanMass = beanController.DebugBeanMass;
        originData.barReducePercentInSecond = barController.reducePercentInSecond;
        originData.barReducePercentOnBreakCombo = barController.reducePercentOnBreakCombo;
        originData.barHealPercentOnGoingCombo = barController.healPercentOnGoingCombo;
    }

    public void LoadOriginSettings()
    {
        forkController.DebugClickIntervalTime = originData.forkClickIntervalTime;
        beanController.DebugFloorRotation = originData.beanFloorRotation;
        beanController.DebugBeanCreateHeight = originData.beanCreateHeight;
        beanController.DebugBeanCreateInSec = originData.beanCreateInSec;
        beanController.DebugBeanImpulseForce = originData.beanImpulseForce;
        beanController.DebugBeanMass = originData.beanMass;
        barController.reducePercentInSecond = originData.barReducePercentInSecond;
        barController.reducePercentOnBreakCombo = originData.barReducePercentOnBreakCombo;
        barController.healPercentOnGoingCombo = originData.barHealPercentOnGoingCombo;
        
        ForkPickTimeIntervalText.text = originData.forkClickIntervalTime.ToString();
        BeanBounceFloorRotationText.text = originData.beanFloorRotation.ToString();
        BeanCreateHeightText.text = originData.beanCreateHeight.ToString();
        BeanCreateInSecText.text = originData.beanCreateInSec.ToString();
        BeanImpulseText.text = originData.beanImpulseForce.ToString();
        BeanMassText.text = originData.beanMass.ToString();
        BarReducePercentText.text = originData.barReducePercentInSecond.ToString();
        BarReduceOnBreakComboText.text = originData.barReducePercentOnBreakCombo.ToString();
        BarHealOnGoingComboText.text = originData.barHealPercentOnGoingCombo.ToString();
    }
    
    public void ReturnBarFull()
    {
        BarController.BarValue = 1.0f;
    }

    public void ReturnScoreZero()
    {
        ScoreManager.TotalScore = 0.0F;
        
        uiManager.SetUITotalScore(ScoreManager.TotalScore);
    }

    public void ReturnComboZero()
    {
        ScoreManager.ComboScore = 0;
        ScoreManager.HighestCombo = 0;
        tempCombo = 0;
        
        highestComboText.text = $"콤보 최고기록 : {ScoreManager.HighestCombo}";
        uiManager.SetUIComboScore(ScoreManager.ComboScore);
    }

    private int tempCombo = 0;
    private void UpdateHighestComboValue()
    {
        if (tempCombo < ScoreManager.HighestCombo)
        {
            tempCombo = ScoreManager.HighestCombo;
            highestComboText.text = $"콤보 최고기록 : {tempCombo}";
        }
    }

    public void SetForkPickTimeInterval(string value)
    {
        if (value == null)
            forkController.DebugClickIntervalTime = originData.forkClickIntervalTime;
        forkController.DebugClickIntervalTime = Convert.ToSingle(value);
    }

    public void SetBeanCreateHeight(string value)
    {
        if (value == null)
            beanController.DebugBeanCreateHeight = originData.beanCreateHeight;
        beanController.DebugBeanCreateHeight = Convert.ToSingle(value);
    }
    public void SetFloorRotation(string value)
    {
        if (value == null)
            beanController.DebugFloorRotation = originData.beanFloorRotation;
        beanController.DebugFloorRotation = Convert.ToSingle(value);
    }
    public void SetBeanCreateInSec(string value)
    {
        if (value == null)
            beanController.DebugBeanCreateInSec = originData.beanCreateInSec;
        beanController.DebugBeanCreateInSec = Convert.ToSingle(value);
    }
    public void SetBeanImpulse(string value)
    {
        if (value == null)
            beanController.DebugBeanImpulseForce = originData.beanImpulseForce;
        beanController.DebugBeanImpulseForce = Convert.ToSingle(value);
    }
    public void SetBeanMass(string value)
    {
        if (value == null)
            beanController.DebugBeanImpulseForce = originData.beanMass;
        beanController.DebugBeanMass = Convert.ToSingle(value);
    }

    public void SetBarReducePercent(string value)
    {
        if (value == null)
            barController.reducePercentInSecond = originData.barReducePercentInSecond;
        barController.reducePercentInSecond = ParsePerNConvertToValue(value);
    }
    public void SetBarReduceOnBreakCombo(string value)
    {
        if (value == null)
            barController.reducePercentOnBreakCombo = originData.barReducePercentOnBreakCombo;
        barController.reducePercentOnBreakCombo = ParsePerNConvertToValue(value);
    }
    public void SetBarHealOnGoingCombo(string value)
    {
        if (value == null)
            barController.healPercentOnGoingCombo = originData.barHealPercentOnGoingCombo;
        barController.healPercentOnGoingCombo = ParsePerNConvertToValue(value);
    }

    // % 문자를 파싱해 제거하고 value를 float로 return
    public float ParsePerNConvertToValue(string value)
    {
        string[] values = value.Split('%');
        return Convert.ToSingle(values[values.Length - 1]);
    }
}
