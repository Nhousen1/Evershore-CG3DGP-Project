using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

/* Author: Marcus King
 * Date created: 11/14/2025
 * Date last updated: 11/14/2025
 * Summary: placeholder test melee weapon item which damages everything within range once per attack.
 */
public class MachineGun : Weapon
{
    [Header("Gameplay Variables")]
    [SerializeField]
    private float damage;
    [SerializeField]
    private float range;
    public int MaxAmmo;
    public int currentAmmo;
    [SerializeField]
    private float ammoRegenTime;
    private float ammoRegenCounter;

    [Header("Particles")]
    public ParticleSystem muzzleFlash;
    public GameObject bulletDust;
    [Header("Collision Info")]
    [SerializeField]
    private Transform shootPoint;
    [SerializeField] 
    private LayerMask damageLayers = 0; 
    public UnityEvent onHit;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fireSound;
    private void Start()
    {
        ammoRegenCounter = ammoRegenTime;
    }
    private void Update()
    {
        if(currentAmmo < MaxAmmo)
        {
            if (ammoRegenCounter <= 0)
            {
                currentAmmo++;
                ammoRegenCounter = ammoRegenTime;
            }
            else
            {
                ammoRegenCounter -= Time.deltaTime;
            }
        }
    }
    //Detect colliders (or triggers) in range of weapon on the first frame of the attack cycle. This is the most simple form of melee
    public override void DoAttack()
    {
        if(currentAmmo <= 0)
        {
            return;
        }
        ammoRegenCounter = ammoRegenTime;
        currentAmmo--;

        muzzleFlash.Play();
        audioSource.PlayOneShot(fireSound);
        if (Physics.Raycast(shootPoint.position, shootPoint.transform.forward, out RaycastHit hit, range, damageLayers))
        {
            Debug.DrawLine(shootPoint.position, hit.point, Color.red, 0.1f);
            GameObject impact = Instantiate(bulletDust, hit.collider.transform.position, hit.collider.transform.rotation);
            var life = hit.collider.GetComponent<EnemyLife>();

            if (life != null)
            {
                life.amount -= damage;
                onHit.Invoke();
            }
        }
        
    }
    public override void StopAttack()
    {
        muzzleFlash.Stop();
    }
}
