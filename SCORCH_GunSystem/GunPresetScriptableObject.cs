using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GunPreset", menuName = "ScriptableObjects/Gun", order = 0)]
public class GunPresetScriptableObject : ScriptableObject
{
    [SerializeField, Tooltip("Is the gun preset for a automatic, or semiautomatic weapon?")]
    bool automatic = false;
    public bool getAutomatic() { return automatic; }

    [SerializeField, Tooltip("How fast a gun preset will fire")]
    float fireRate = 0.0f;
    public float getFireRate() { return fireRate; }

    [SerializeField, Tooltip("How Large is the flash when the gun fires")]
    float flash = 5.0f;
    public float getFlash() { return flash; }

    [SerializeField, Tooltip("How much damage a single bullet deals")]
    float damage = 1.0f;
    public float getDamage() { return damage; }

    //[SerializeField, Tooltip("How much recoil a gun has")]
    //float recoil = 0.0f;
    //public float getRecoil() { return recoil; }

    [SerializeField, Tooltip("How much ammo a gun has before it must be reloaded"), Range(1, 120)]
    int ammoCount = 60;
    public int getAmmoCount() { return ammoCount; }

    [SerializeField, Tooltip("How fast does the gun reload")]
    float reloadSpeed = 1f;
    public float getReloadSpeed() { return reloadSpeed; }

    [SerializeField, Tooltip("How scattered are the shots at a minimum"),Range(0f,0.3f)]
    float minBulletSpread = 0f;
    public float getMinBulletSpread() { return minBulletSpread; }

    [SerializeField, Tooltip("How scattered are the shots at a maximum"), Range(0f, 0.3f)]
    float maxBulletSpread = 2f;
    public float getMaxBulletSpread() { return maxBulletSpread; }

}
