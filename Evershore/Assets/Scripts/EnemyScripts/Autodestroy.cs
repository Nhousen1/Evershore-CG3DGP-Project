using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
* Author: Liam Housenbold
* Date Created: 9-30-2025
* Date Modified: 10-1-2025
* Summary: Destroys the game object after a set delay.
*/
public class Autodestroy : MonoBehaviour
{
    [SerializeField] private float delay;


    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, delay);
    }
}