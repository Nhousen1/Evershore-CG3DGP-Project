using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OarWeapon : Weapon
{
    [SerializeField]
    private Collider hitbox;
    private void Start()
    {
        hitbox.isTrigger = true;
        hitbox.gameObject.SetActive(false);
    }
    public override void DoAttack()
    {
        hitbox.gameObject.SetActive(true);
        Invoke(nameof(DisableHitbox), active);

    }
    private void DisableHitbox()
    {
        hitbox.gameObject.SetActive(false);
    }
}
