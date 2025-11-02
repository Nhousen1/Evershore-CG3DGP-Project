using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
* Author: Liam Housenbold
* Date Created: 9-30-2025
* Date Modified: 10-1-2025
* Summary: moves enemy projectile forward at a set speed.
*/
public class ProjectileForward : MonoBehaviour
{
    [SerializeField] private float speed;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0, 0, speed * Time.deltaTime); 
    }
}
