using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class InGameUIManager : MonoBehaviour
{
    [SerializeField] public TMP_Text totalScoreText;
    [SerializeField] private TMP_Text comboScoreText;

    public Transform beanImage;

    private Color fadeOutColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);

    [SerializeField] private int beforeCombo = 0;

    public void SetUITotalScore(float score)
    {
        totalScoreText.text = $"집은 콩 : {score}개";
    }
    public void SetUIComboScore(int num)
    {
        if (num != 0)
        {
            comboScoreText.color = Color.white;
            comboScoreText.text = $"{num} 콤보!";
        }
        else
        {
            if (beforeCombo != 0)
            {
                comboScoreText.color = Color.grey;
                comboScoreText.text = $"콤보 Break..";
            }
        }
        beforeCombo = num;
    }
}
