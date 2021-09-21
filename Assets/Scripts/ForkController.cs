using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class ForkController : MonoBehaviour
{
    public float DebugClickIntervalTime
    {
        get => clickIntervalTime;
        set => clickIntervalTime = value;
    }
    
    [SerializeField] private Transform weaponTransform;

    [SerializeField] private float attackReadyDist;
    [SerializeField] private float notDetectFloorDist;
    [SerializeField] private float clickedDist;

    [SerializeField] private float clickIntervalTime;

    [SerializeField] private BeanController beanController;
    [SerializeField] private BarController barController;
    [SerializeField] private ScoreManager scoreManager;
    
    private Vector3 attackReadyVec;
    private Vector3 notDetectFloorVec;
    
    private bool isOkClick = true;
    private bool isMouseOnFloor = true;

    private Ray floorCheckRay;
    private RaycastHit floorCheckHit;
    private RaycastHit[] beanCheckHits;

    public AudioSource forkSoundGood;
    public AudioSource forkSoundBad;

    private void Awake()
    {
        if (!beanController) beanController = FindObjectOfType<BeanController>();
        
        attackReadyVec = new Vector3(0.0f, attackReadyDist, 0.0f);
        notDetectFloorVec = new Vector3(0.0f, notDetectFloorDist, 0.0f);
    }

    private void Update()
    {
        floorCheckRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        CheckFloorWithRaycast();
        OnClickLeftMouseButton();
    }
    
    private void OnClickLeftMouseButton()
    {
        if (GameManager.IsGameStart)
        {
            if (!GameManager.IsGameOver)
            {
                if (isOkClick)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (isMouseOnFloor)
                        {
                            CatchBeansWithRaycast();
                
                            StartCoroutine(ForkPickingAnimation());
                            StartCoroutine(ClickBlockTimer(clickIntervalTime));
                        }
                    }
                }
            }
        }
    }

    private void CheckFloorWithRaycast()
    {
        if (isMouseOnFloor = Physics.Raycast(floorCheckRay, out floorCheckHit, 200.0f, 1 << LayerMask.NameToLayer("Floor")))
        {
            weaponTransform.position = floorCheckHit.point + attackReadyVec;
            CursorSystem.CursorStateChange(CursorState.Targeted);
        }
        else if (Physics.Raycast(floorCheckRay, out floorCheckHit, 200.0f, 1 << LayerMask.NameToLayer("RaycastFloor")))
        {
            weaponTransform.position = floorCheckHit.point + notDetectFloorVec;
            CursorSystem.CursorStateChange(CursorState.UnTargeted);
        }
    }
    private void CatchBeansWithRaycast()
    {
        beanCheckHits = Physics.RaycastAll(floorCheckRay, 50.0f, 1 << LayerMask.NameToLayer("Bean"));
        
        if (beanCheckHits.Length != 0)
        {
            foreach (var bean in beanCheckHits)
            {
                scoreManager.SetCombo(1);
                barController.HealBarOnGoingCombo();
                beanController.GetPickedBean(bean.transform);
            }
            forkSoundGood.PlayOneShot(forkSoundGood.clip);
        }
        else
        {
            if (!scoreManager.IsZeroCombo)
            {
                scoreManager.SetCombo(0);
                barController.ReduceBarOnBreak();
            }
            forkSoundBad.PlayOneShot(forkSoundBad.clip);
        }
    }

    private IEnumerator ForkPickingAnimation()
    {
        attackReadyVec = new Vector3(0.0f, clickedDist, 0.0f);
        
        yield return new WaitForSeconds(0.1f);

        Vector3 tempVec = new Vector3(0.0f, attackReadyDist, 0.0f);
        while (Vector3.Distance(attackReadyVec, tempVec) > 0.1f)
        {
            attackReadyVec = Vector3.Lerp(attackReadyVec, tempVec, 0.25f);
            yield return new WaitForSeconds(0.01f);
        }

        attackReadyVec = tempVec;
    }

    private IEnumerator ClickBlockTimer(float time)
    {
        if (isOkClick)
        {
            isOkClick = false;
            yield return new WaitForSeconds(time);
            isOkClick = true;
        }
    }
}