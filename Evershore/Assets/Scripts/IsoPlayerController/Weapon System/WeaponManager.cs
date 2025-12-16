using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/* Author: Marcus King
 * Date created: 10/1/2025
 * Date last updated: 10/6/2025
 * Summary: an "inventory" (list of weapons) giving the player weapon selection in scene and handling relevant input calls. Also handles weapon animarion layer.
 */
public class WeaponManager : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField]
    private List<Weapon> weaponList;
    public Weapon activeWeapon;
    [Header("Animation")]
    public Transform weaponFollowPoint;
    public Animator animator;
    private RuntimeAnimatorController baseController;
    private int WeaponAnimLayerIndex;

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
        baseController = animator.GetComponent<RuntimeAnimatorController>();    
        WeaponAnimLayerIndex = animator.GetLayerIndex("Weapon");
        DisableWeaponAnimations();
        //By default, the first weapon in hand is the first in the list
        if (weaponList != null && weaponList.Count != 0)
        {
            foreach (Weapon weapon in weaponList)
            {
                weapon.animator = animator;
                weapon.gameObject.SetActive(false);
            }

            SelectWeapon(0);
            EnableWeaponAnimations();
        }
    }
    private void Update()
    {
        if(activeWeapon != null)
        {
            activeWeapon.transform.position = weaponFollowPoint.position;
            activeWeapon.transform.rotation = weaponFollowPoint.rotation;
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
            else
            {
                animator.SetLayerWeight(WeaponAnimLayerIndex, 0);
            }
        }
    }
    public void SelectWeapon(int i)
    {
        //Change the active weapon with checks for empty lists and redundant calls
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

        if (activeWeapon.overrideController != null)
        {
            animator.runtimeAnimatorController = activeWeapon.overrideController;
        }
        else
        {
            animator.runtimeAnimatorController = baseController;
        }
    }
    public void DisableWeaponAnimations()
    {
        animator.SetLayerWeight(WeaponAnimLayerIndex, 0);
    }
    public void EnableWeaponAnimations()
    {
        animator.SetLayerWeight(WeaponAnimLayerIndex, 1);
    }
}

