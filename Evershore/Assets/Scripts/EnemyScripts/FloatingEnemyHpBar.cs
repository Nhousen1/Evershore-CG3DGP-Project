using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingEnemyHpBar : MonoBehaviour
{
    [SerializeField] private Slider hpBarSlider;
   
    public void UpdateHpBar(float currentHp, float maxHp)
    {
        hpBarSlider.value = currentHp / maxHp;
    }
    void Update()
    {
        
    }
}
