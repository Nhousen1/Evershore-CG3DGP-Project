using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
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
        if (weaponList == null || weaponList.Count == 0)
        {
            return;
        }
        SelectWeapon((index + 1) % weaponList.Count);
    }
    public void OnPrevious()
    {
        if (weaponList == null || weaponList.Count == 0)
        {
            return;
        }
        SelectWeapon((index - 1 + weaponList.Count) % weaponList.Count);
    }
    void Start()
    {
        if(weaponList != null && weaponList.Count != 0)
        {
            foreach (Weapon weapon in weaponList)
            {
                weapon.gameObject.SetActive(false);
            }
            SelectWeapon(0); //select the first weapon by default
        }
    }
    public void addWeapon(Weapon weapon)
    {
        weaponList.Add(weapon);
        weapon.gameObject.SetActive(false);
    }
    public void removeWeapon(Weapon weapon)
    {
        bool wasActive = (weapon == activeWeapon);
        weaponList.Remove(weapon);
        Destroy(weapon.gameObject);
        int idx = weaponList.IndexOf(weapon);
        if (idx == -1) return;
        
        weaponList.RemoveAt(idx);
        Destroy(weapon.gameObject);

        if (wasActive) 
        {
            activeWeapon = null; 
            index = -1;
            if (weaponList.Count > 0)
            {
                SelectWeapon(Mathf.Clamp(idx, 0, weaponList.Count - 1));
            }
        }
    }
    public void SelectWeapon(int i)
    {
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

