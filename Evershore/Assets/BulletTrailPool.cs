using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletTrailPool : MonoBehaviour
{
    private GameObject bulletTrailPrefab;
    [SerializeField] private int defaultCapacity = 32;
    [SerializeField] private int maxSize = 256;
    [SerializeField] private int prewarm = 32;

    private ObjectPool<bulletTrail> pool;

}
