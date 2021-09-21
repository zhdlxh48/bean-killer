using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private InGameUIManager uiManager;
    [SerializeField] private InGameNetworkManager networkManager;
    
    public static float TotalScore = 0.0f;
    public static int ComboScore = 0;
    public static int HighestCombo = 0;

    public bool IsZeroCombo => ComboScore == 0;

    private void Awake()
    {
        TotalScore = 0.0f;
        ComboScore = 0;
        HighestCombo = 0;
    }

    public void AddScore(float score)
    {
        TotalScore += score;
        uiManager.SetUITotalScore(TotalScore);
        networkManager.BroadcastScore((int)TotalScore);
    }
    
    public void SetCombo(int combo = 1)
    {
        ComboScore = combo != 0 ? ComboScore + 1 : 0;
        if (ComboScore > HighestCombo)
            HighestCombo = ComboScore;
        
        uiManager.SetUIComboScore(ComboScore);
    }
    
    public void AddBean(BeanObject beanObj){
        DOTween.Init();
        beanObj.gameObject.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 1.2f, 5);
        //beanObj.gameObject.transform.DOJump( uiManager.beanImage.transform.position, 0.1f, 1, 1.0f, false)
        beanObj.gameObject.transform.DOMove( uiManager.beanImage.transform.position, 1.0f, false)
            .SetEase(Ease.InQuint)
            .OnComplete(()=>{
                uiManager.beanImage.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 1.2f, 5);
                uiManager.totalScoreText.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.2f, 2);
                AddScore(beanObj.script.score);
                BeanController.DestroyBean(beanObj);
            });
    }
}
