using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Marcus King
//Date created: 11/2/25
//Date last modified: 11/2/25
//Summary: handles the blood drriping effect when player hits something. Effectively a counter.
public class BloodDripper : MonoBehaviour
{
    public float decay;
    public ParticleSystem bloodDroplets;
    private float bloodAmount = 1;
    public void Reset()
    {
        bloodDroplets.gameObject.SetActive(true);
        bloodDroplets.Play();
        bloodAmount = 1;
    }
    void Update()
    {
        if(bloodAmount > 0)
        {
            bloodAmount -= Time.deltaTime * decay;
            var emmision = bloodDroplets.emission;
            emmision.rateOverTime = 3 * bloodAmount;
        }
        else
        {
            var emmision = bloodDroplets.emission;
            emmision.rateOverTime = 0;
        }
    }
}
