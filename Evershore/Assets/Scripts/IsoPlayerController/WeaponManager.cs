using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/* Author: Marcus King
 * Date created: 10/1/2025
 * Date last updated: 10/6/2025
 * Summary: an "inventory" (list of weapons) giving the player weapon selection in scene and handling relevant input calls.
 */
public class WeaponManager : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField]
    private List<Weapon> weaponList;
    public Weapon activeWeapon;

    private int index = -1;
    public void OnAttack(InputValue value)
    {
        if (activeWeapon == null)
        {
            return;
        }
        //Events sent to weapon scripts, hold release used for Autohold weapons
        if (value.isPressed)
        {
            activeWeapon.onUsePressed();
        }
        else
        {
            activeWeapon.onUseReleased();
        }
    }
    public void OnNext()
    {
        //Index forward through weapon list
        if (weaponList == null || weaponList.Count == 0)
        {
            return;
        }
        SelectWeapon((index + 1) % weaponList.Count);
    }
    public void OnPrevious()
    {
        //Index backward, ensuring index is positive
        if (weaponList == null || weaponList.Count == 0)
        {
            return;
        }
        SelectWeapon((index - 1 + weaponList.Count) % weaponList.Count);
    }
    void Start()
    {
        //By default, the first weapon in hand is the first in the list
        if(weaponList != null && weaponList.Count != 0)
        {
            foreach (Weapon weapon in weaponList)
            {
                weapon.gameObject.SetActive(false);
            }
            SelectWeapon(0);
        }
    }
    public void addWeapon(Weapon weapon)
    {
        weaponList.Add(weapon);
        weapon.gameObject.SetActive(false);
    }
    public void removeWeapon(Weapon weapon)
    {
        //Removes weapon while handling case if that weapon is the active weapon
        int weaponIndex = weaponList.IndexOf(weapon);

        bool wasActive = (weapon == activeWeapon);
        
        weaponList.Remove(weapon);
        Destroy(weapon.gameObject);


        if (wasActive) 
        {
            activeWeapon = null; 
            index = -1;
            if (weaponList.Count > 0)
            {
                SelectWeapon(Mathf.Clamp(weaponIndex, 0, weaponList.Count - 1));
            }
        }
    }
    public void SelectWeapon(int i)
    {
        //Chagne the active weapon with checks for empty lists and redundant calls
        if (weaponList.Count == 0)
        {
            return;
        }
        i = Mathf.Clamp(i, 0, weaponList.Count - 1);
        if (i == index && activeWeapon != null)
        {
            return;
        }
        if(activeWeapon != null)
        {
            activeWeapon.gameObject.SetActive(false);
        }
        index = i;
        activeWeapon = weaponList[index];
        activeWeapon.gameObject.SetActive(true);
    }
}

