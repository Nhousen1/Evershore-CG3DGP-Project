using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AmmoCounter : MonoBehaviour
{
    public TMP_Text maxCount;
    public TMP_Text currentCount;
    public MachineGun data;
    // Start is called before the first frame update
    void Start()
    {
        if(data == null)
        {
            Debug.LogWarning("Ammo counter with no machine gun.");
        }
        else
        {
            maxCount.text = data.MaxAmmo.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentCount.text = data.currentAmmo.ToString();
    }
}
