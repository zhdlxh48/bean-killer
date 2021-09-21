using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class BeanController : MonoBehaviour
{
    #region DEBUG Properties

    public float DebugFloorRotation
    {
        get => floorRotation;
        set
        {
            floorRotation = value;
            FloorTransform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, floorRotation));
        }
    }
    
    public float DebugBeanCreateHeight
    {
        get => createHeight;
        set
        {
            createHeight = value;
            var position = GeneratePosTransform.localPosition;
            GeneratePosTransform.localPosition = new Vector3(position.x, createHeight, position.z);
        }
    }
    
    public float DebugBeanCreateInSec
    {
        get => SpawnNumberInSecond;
        set
        {
            SpawnNumberInSecond = value;
        }
    }
    
    public float DebugBeanImpulseForce
    {
        get => ImpulseForce;
        set => ImpulseForce = value;
    }

    public float DebugBeanMass
    {
        get => BeanMass;
        set => BeanMass = value;
    }

    #endregion
    
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private InGameUIManager uiManager;
    
    [SerializeField] private GameObject BeanPrefab;
    [SerializeField] private int poolObjectNum;

    [SerializeField] private float floorRotation;
    [SerializeField] private float createHeight;
    [SerializeField] private float SpawnNumberInSecond;
    [SerializeField] private float ImpulseForce;
    [SerializeField] private float BeanMass;
    [SerializeField] private Transform FloorTransform;
    [SerializeField] private Transform GeneratePosTransform;
    [Range(1, 10)] [SerializeField] private int GenerateMaxRange;

    private static List<BeanObject> allBeanList = new List<BeanObject>();
    private List<BeanObject>.Enumerator beanIterator;
    private List<BeanObject> spawnBeanPoolList = new List<BeanObject>();

    private WaitForSeconds spawnInterval;

    [SerializeField] private Transform pickedBeansStorage;

    private void Awake()
    {
        spawnInterval = new WaitForSeconds(1 / SpawnNumberInSecond);

        StartCoroutine(BeanPooling(poolObjectNum));
    }

    private void Start()
    {
        //StartCoroutine(BeanSpawn());
    }
    
    private IEnumerator BeanPooling(int count)
    {
        BeanObject tempObj;
        for (int i = 0; i < count; i++)
        {
            tempObj = Instantiate(BeanPrefab).GetComponent<BeanObject>();
            tempObj.gameObject.SetActive(false);
            
            allBeanList.Add(tempObj);
            spawnBeanPoolList.Add(tempObj);
        }
        beanIterator = spawnBeanPoolList.GetEnumerator();

        yield return null;
    }
    
    public IEnumerator BeanSpawn()
    {
        while (true)
        {
            for (int i = 0; i < poolObjectNum; i++)
            {
                if (beanIterator.MoveNext())
                {
                    BeanObject tempObj = beanIterator.Current;

                    tempObj.transform.position = 
                        VectorUtil.RandomVector3(GeneratePosTransform.position, -GenerateMaxRange, GenerateMaxRange, VectorAxis.Z);
                    tempObj.beanRigid.mass = BeanMass;
                    tempObj.gameObject.SetActive(true);
                    tempObj.beanRigid.AddForce(ImpulseOnSphere(), ForceMode.Impulse);
                }
                
                //yield return spawnInterval;
                yield return new WaitForSeconds(1 / SpawnNumberInSecond);
            }
            spawnBeanPoolList.Clear();
            StartCoroutine(BeanPooling(poolObjectNum));
        }
    }

    private Vector3 ImpulseOnSphere()
    {
        Vector3 powerVec = Random.onUnitSphere * ImpulseForce;
        powerVec.x = Mathf.Abs(powerVec.x);
        powerVec.z = powerVec.z > ImpulseForce/2 ? ImpulseForce/2 : powerVec.z;
        powerVec.z = powerVec.z < -ImpulseForce/2 ? -ImpulseForce/2 : powerVec.z;

        return powerVec;
    }

    public void GetPickedBean(Transform beanTransform)
    {
        BeanObject tempObj = beanTransform.GetComponent<BeanObject>();

        tempObj.beanCollider.enabled = false;
        tempObj.beanRigid.isKinematic = true;
        tempObj.animator.speed = 0.0f;
        
        // 점수 얻기 전에 애니메이션 추가
        scoreManager.AddBean(tempObj);
    }

    public static void DestroyBean(BeanObject beanObj)
    {
        BeanObject tempObj = allBeanList.Find((listBean) => listBean == beanObj);
        GameObject tempGameObj = tempObj.gameObject;

        allBeanList.Remove(tempObj);
        Destroy(tempGameObj);
    }
}
