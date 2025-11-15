using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponWidgetManager : MonoBehaviour
{
    //TODO: make this scale with different items
    public WeaponManager manager;
    public GameObject oarWidget;
    public GameObject flareWidget;

    // Update is called once per frame
    void Update()
    {
        if (manager.activeWeapon.GetComponent<OarWeapon>())
        {
            oarWidget.SetActive(true);
            flareWidget.SetActive(false);
        }
        if (manager.activeWeapon.GetComponent<MachineGun>())
        {
            flareWidget.SetActive(true);
            oarWidget.SetActive(false);
        }
    }
}
