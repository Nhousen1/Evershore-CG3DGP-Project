using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum FireMode { Single, AutoHold }
public abstract class Weapon : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] 
    private FireMode fireMode = FireMode.Single;
    [SerializeField]
    protected float windUp = 0f;
    [SerializeField]
    protected float active = 0.1f;
    [SerializeField]
    protected float recovery = 0.3f;


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
    //Specific weapon behavior here
    public abstract void DoAttack();
}
