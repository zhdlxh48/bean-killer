using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;

public class BeanObject : MonoBehaviour
{
    public Rigidbody beanRigid;
    public Collider beanCollider;
    
    public SpriteRenderer renderer;
    public Animator animator;
    
    public BeanStatScript script;
}
