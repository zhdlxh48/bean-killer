using System;
using System.Collections;
using UnityEngine;

public class PhaseTimer : MonoBehaviour
{
    public float[] phaseTimes;
    public SettingData[] phaseData;

    public int totalPhaseSize;
    public int currentPhase = 0;
    
    public ForkController forkController;
    public BeanController beanController;
    public BarController barController;

    private void Awake()
    {
        phaseTimes = new float[totalPhaseSize];
        phaseData = new SettingData[totalPhaseSize];
    }

    private void Start()
    {
        StartCoroutine(PhaseChanger());
    }

    private IEnumerator PhaseChanger()
    {
        yield return null;
    }
}