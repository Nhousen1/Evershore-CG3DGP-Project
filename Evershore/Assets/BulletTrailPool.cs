using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletTrailPool : MonoBehaviour
{
    [SerializeField] private GameObject trailPrefab;
    [SerializeField] private int defaultCapacity = 32;
    [SerializeField] private int maxSize = 256;
    [SerializeField] private int prewarm = 32;

    private ObjectPool<bulletTrail> pool;

    private void Awake()
    {
        pool = new ObjectPool<bulletTrail>(
            createFunc: () =>
            {
                GameObject trailObject = Instantiate(trailPrefab, transform);
                trailObject.SetActive(false);
                bulletTrail t = trailObject.GetComponent<bulletTrail>();
                t.pool = this.pool;
                return t;
            },
            actionOnGet: t => t.gameObject.SetActive(true),
            actionOnRelease: t => t.gameObject.SetActive(false),
            actionOnDestroy: t => Destroy(t.gameObject),
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
        //Prewarming
        for (int i = 0; i < prewarm; i++)
        {
            bulletTrail trail = pool.Get();
            pool.Release(trail);
        }
    }
    public bulletTrail Get() => pool.Get();
}
