using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingEnemyHpBar : MonoBehaviour
{
    [SerializeField] private Slider hpBarSlider;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found");
        }
    }
    
    public void UpdateHpBar(float currentHp, float maxHp)
    {
        hpBarSlider.value = currentHp / maxHp;
    }
    void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
        }
    }
}
