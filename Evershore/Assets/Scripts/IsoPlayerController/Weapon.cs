using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum FireMode { Single, AutoHold }
/* Author: Marcus King
 * Date created: 10/1/2025
 * Date last updated: 10/6/2025
 * Summary: enumerates through an attack cycle defined in inspector. Inherited scripts handle attack function.
 */
public abstract class Weapon : MonoBehaviour
{
    [Header("Cycle Timing")]
    [SerializeField] 
    private FireMode fireMode = FireMode.Single;
    [SerializeField]
    protected float windUp;
    [SerializeField]
    protected float active;
    [SerializeField]
    protected float recovery;


    private bool isCycling = false;
    private bool isAttackHeld = false;
    public virtual void onUsePressed()
    {
        isAttackHeld = true;
        TryStartCycle();
    }
    public virtual void onUseReleased()
    {
        isAttackHeld = false;
    }
    private void TryStartCycle()
    {
        if (!isCycling)
        {
            StartCoroutine(Cycle());
        }
    }
    System.Collections.IEnumerator Cycle()
    {
        isCycling = true;

        if (windUp > 0f) yield return new WaitForSeconds(windUp);
        DoAttack();
        if (active > 0f) yield return new WaitForSeconds(active);
        if (recovery > 0f) yield return new WaitForSeconds(recovery);

        isCycling = false;

        if (fireMode == FireMode.AutoHold && isAttackHeld)
            TryStartCycle();
    }
    //Specific inherited weapon behavior is all contained in this method
    public abstract void DoAttack();
}
