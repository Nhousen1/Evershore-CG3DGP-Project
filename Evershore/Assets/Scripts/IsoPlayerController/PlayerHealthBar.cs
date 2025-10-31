using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Author: Marcus King
 * Date created: 10/30/2025
 * Date last updated: 10/30/2025
 * Summary: handles UI events for player health
 */
public class PlayerHealthBar : Singleton<PlayerHealthBar>
{
    public override bool defineScenePersistence()
    {
        return false;
    }

    private float maxAmount; //injected
    private float currentAmount; //injected

    [SerializeField]
    public Slider uiSlider;
    public void ChangeValue(float newAmount)
    {
        currentAmount = newAmount;
        uiSlider.value = currentAmount / maxAmount;
    }
    public void ChangeValue(float newAmount, float newMaxAmount)
    {
        currentAmount = newAmount;
        maxAmount = newMaxAmount;
        uiSlider.value = currentAmount / maxAmount;
    }
}
