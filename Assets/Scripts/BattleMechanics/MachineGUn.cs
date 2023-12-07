using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGUn : MonoBehaviour
{
    [SerializeField] GunData gunData;
    [SerializeField] Transform Muzzle;

    private float timeSinceLastShot;


    private void Update()
    {
        timeSinceLastShot += Time.deltaTime;

        Debug.DrawRay(Muzzle.position, Muzzle.forward);
    }
    private bool CanShoot() => timeSinceLastShot > 1f / (gunData.FireRate / 60f);
    private void Shoot()
    {
        if(gunData.CurrentAmmo > 0)
        {
            if(CanShoot())
            {
                if(Physics.Raycast(Muzzle.position, transform.forward, out RaycastHit hitInfo, gunData.MaxDistance)) 
                {
                    Debug.Log(hitInfo.transform.name);
                }
                gunData.CurrentAmmo--;
                timeSinceLastShot = 0f;
            }
        }
    }
}
