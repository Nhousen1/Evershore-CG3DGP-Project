using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/* Author: Marcus King
 * Date created: 12/25/2025
 * Date last updated: 12/25/2025
 * Summary: placeholder UI manager for weapon widgets
 */
public class WeaponWidgetManager : MonoBehaviour
{
    public WeaponManager manager;
    public List<GameObject> weaponWidgets;//TODO: in future, inject from prefab references from weapons
    private int currentIndex = 0; //TODO: Have this sync with weapons manager index

    
    [SerializeField] private InputActionReference next;
    [SerializeField] private InputActionReference previous;

    private void Awake()
    {
        foreach (var weapon in weaponWidgets)
        {
            weapon.gameObject.SetActive(false);
        }
        currentIndex = manager.weaponList.IndexOf(manager.activeWeapon); //UNSAFE: assumes dev has perfectly synced the lists
        showWidgetAtIndex(currentIndex);
    }
    private void OnEnable()
    {
        next.action.performed += onNext;
        previous.action.performed += onPrevious;
    }
    private void OnDisable()
    {
        next.action.performed -= onNext;
        previous.action.performed -= onNext;
    }
    private void onNext(InputAction.CallbackContext ctx)
    {
        currentIndex = (currentIndex + 1) % weaponWidgets.Count;
        showWidgetAtIndex(currentIndex);
    }
    private void onPrevious(InputAction.CallbackContext ctx)
    {
        currentIndex = (currentIndex - 1 + weaponWidgets.Count) % weaponWidgets.Count;
        showWidgetAtIndex(currentIndex);
    }
    private void showWidgetAtIndex(int index)
    {
        if(index > weaponWidgets.Count)
        {
            Debug.LogError("Index out of list bounds");
            return;
        }
        foreach (GameObject widget in weaponWidgets) 
        { 
            if(widget == weaponWidgets[index]) 
            {
                widget.SetActive(true);
            }
            else
            {
                widget.SetActive(false);
            }
        }
    }
}
