
/*Author: Lansingh Freeman
 *Modified By:
*Amelia Payne: Added code to disable shooting when menu open
*Readded code to rotate reticle
 */


using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Runtime.ExceptionServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunMasterScript : MonoBehaviour, IEventListener
{
    [SerializeField] 
    LayerMask playerLayer;

    [Header("Gun:")]

    [SerializeField]
    GunPresetScriptableObject gpso;

    [SerializeField]
    Camera gunCam;
    [SerializeField]
    Transform gunBarrel;

    [SerializeField]
    Transform gun;

    //[SerializeField]
    //GameObject bullet;

    [Header("Lights:")]

    [SerializeField]
    Light flashLight;
    [SerializeField]
    MeshRenderer flashObject;

    //Gun properties
    bool automatic;
    float fireRate;
    float flashSize;
    float damage;
    //float recoil;
    int ammoCount;
    float reloadSpeed;
    float minBulletSpread;
    float maxBulletSpread;

    //local gun properties
    bool canFire = true;
    bool gamePaused = false;
    float currentSpread = 0.0f;

    int currentAmmo;
    bool reloading = false;

    float sfxVolume = 1f;

    [Header("Audio:")]

    [SerializeField]
    AudioSource audioSource;


    [SerializeField]
    AudioClip gunShotClip;
    [SerializeField] private float gunShotSoundLevel = 1f;

    [SerializeField]
    AudioClip reloadAudioClip;
    [SerializeField] private float reloadSoundLevel = .5f;


    //recoil system
    //float currentRecoil = 0.0f;
    //float recoilSpeed = 50f;
    //float recoilMod = 0.1f;

    [Header("Animation:")]

    public Animator recoilAnim;

    [SerializeField]
    GameObject brickWallEmmiter;

    [SerializeField]
    GameObject woodWallEmmiter;

    [SerializeField]
    GameObject explosiveEmmiter;

    [Header("Decals:")]

    [SerializeField]
    GameObject blackBulletDecal;

    [SerializeField]
    GameObject redBulletDecal;

    [SerializeField]
    float decalLifeSpan = 5f;


    void InitGun()
    {
        automatic = gpso.getAutomatic();
        fireRate = gpso.getFireRate();
        flashSize = gpso.getFlash();
        damage = gpso.getDamage();
        //recoil = gpso.getRecoil();
        ammoCount = gpso.getAmmoCount();
        reloadSpeed = gpso.getReloadSpeed();
        minBulletSpread = gpso.getMinBulletSpread();
        maxBulletSpread = gpso.getMaxBulletSpread();
    }



    // Start is called before the first frame update
    void Start()
    {
        InitGun();
        currentSpread = minBulletSpread;
        currentAmmo = ammoCount;

        EventQueue.instance.Subscribe(MessageType.MENU_OPENED, this);
        EventQueue.instance.Subscribe(MessageType.MENU_CLOSED, this);
        EventQueue.instance.Subscribe(MessageType.SFX_LEVEL_SET, this);
        EventQueue.instance.Subscribe(MessageType.PLAYER_KILLED, this);

        StartCoroutine(SendGunDataDelayed());

    }

    private void OnDestroy()
    {
        EventQueue.instance.Unsubscribe(MessageType.MENU_OPENED, this);
        EventQueue.instance.Unsubscribe(MessageType.MENU_CLOSED, this);
        EventQueue.instance.Unsubscribe(MessageType.SFX_LEVEL_SET, this);
        EventQueue.instance.Unsubscribe(MessageType.PLAYER_KILLED, this);

    }

    public void resetPlayerGun()
    {
        StopAllCoroutines();
        flashObject.enabled = false;
        canFire = true;
    }
    public void DisableGun()
    {
        gun.gameObject.SetActive(false);
        canFire = false;
    }

    public void EnableGun()
    {
        gun.gameObject.SetActive(true);
        canFire = true;
    }


    IEnumerator SendGunDataDelayed()
    {
        yield return new WaitForEndOfFrame();
        EventQueue.instance.DispatchEvent(new GunInformationMessage(minBulletSpread, currentAmmo, ammoCount));
    }

    // Update is called once per frame
    void Update()
    {
        FireWeapon();
        DecreaseFlash();
        Reload();

        
    }

    void FireWeapon()
    {
        //UpdateReticle(); replace with message
        DecreaseSpread();

        if (automatic)
        {
            if (Input.GetMouseButton(0) && canFire && currentAmmo > 0 && !reloading && !gamePaused)
            {
                StartCoroutine(FireLimiter());
                GunShotSound();
                currentAmmo -= 1;
                Shoot();
                //currentRecoil += recoilMod;
            }
            else if (Input.GetMouseButtonDown(0) && currentAmmo <= 0 && !reloading)
            {
                StartCoroutine(ReloadWait());
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && canFire && currentAmmo > 0 && !reloading && !gamePaused)
            {
                StartCoroutine(FireLimiter());
                GunShotSound();
                currentAmmo -= 1;
                Shoot();
                //currentRecoil += recoilMod;
            }
            else if(Input.GetMouseButtonDown(0) && currentAmmo <= 0 && !reloading)
            {
                StartCoroutine(ReloadWait());
            }
        }

        //RecoilHandler();
    }

    IEnumerator FireLimiter()
    {
        canFire = false;
        yield return new WaitForSecondsRealtime(fireRate);
        canFire = true;
    }

    void Shoot()
    {
        //Vector3 startPoint = gunBarrel.position;
        //Vector3 objectPoint;
        bool hitTarget = false;

        recoilAnim.Play("Recoil");

        IncreaseSpread();
        Flash();

        Vector3 spread = gunCam.transform.forward;
        spread.x += Random.Range(-currentSpread, currentSpread);
        spread.y += Random.Range(-currentSpread, currentSpread);
        spread.z += Random.Range(-currentSpread, currentSpread);

        //Debug.Log(currentSpread);

        RaycastHit hit;
        if (Physics.Raycast(gunCam.transform.position, spread, out hit, 100f, ~playerLayer))
        {
            hit.transform.SendMessage("Hit", damage, SendMessageOptions.DontRequireReceiver);

            if (!hit.collider.tag.Equals("Enemy"))
            {
                SpawnDecal(hit);
                SpawnEmmiter(hit);
            }
            else
            {
                hitTarget = true;
                EmbedDecal(hit);
            }

            //objectPoint = hit.point;
        }
        else
        {
            //objectPoint = gunCam.transform.position + spread * 100f;
        }
        Debug.DrawRay(gunCam.transform.position, spread, Color.green, 10f);

        //Vector3 direction = objectPoint - startPoint;

        //GameObject bulletObject = Instantiate(bullet);
        //bulletObject.GetComponent<BulletScript>().damage = damage;

        //bulletObject.transform.position = startPoint;
        //bulletObject.transform.rotation = Quaternion.LookRotation(direction);

        EventQueue.instance.DispatchEvent(new GunShootMessage(currentSpread, currentAmmo, hitTarget));
    }

    void IncreaseSpread()
    {
        currentSpread += 1f * Time.deltaTime;

        if (currentSpread > maxBulletSpread)
        {
            currentSpread = maxBulletSpread;
        }
    }

    void DecreaseSpread()
    {
        if (currentSpread > minBulletSpread)
        {
            currentSpread -= (0.05f) * Time.deltaTime;
        }
        else
        {
            currentSpread = minBulletSpread;
        }
    }

    void Flash()
    {
        flashLight.range = flashSize;
        StartCoroutine(Blink());
    }

    void GunShotSound()
    {
        audioSource.volume = sfxVolume * gunShotSoundLevel;
        audioSource.clip = gunShotClip;

        audioSource.PlayOneShot(audioSource.clip);
    }

    IEnumerator Blink()
    {
        Transform flashTransform = flashObject.gameObject.transform;
        flashTransform.localEulerAngles = new Vector3(
            flashTransform.localEulerAngles.x + 217.37f,
            flashTransform.localEulerAngles.y,
            flashTransform.localEulerAngles.z);

        flashObject.enabled = true;
        yield return new WaitForSecondsRealtime(0.05f);
        flashObject.enabled = false;
    }

    void DecreaseFlash()
    {
        flashLight.range = flashLight.range - 60f * Time.deltaTime;
    }

    void Reload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !reloading && !GameManager.Instance.isGamePaused && currentAmmo < 6) //This is probably more of a temporary fix, but should still do the job
        {
            StartCoroutine(ReloadWait());
        }
    }

    IEnumerator ReloadWait()
    {
        reloading = true;
        recoilAnim.Play("Reload");

        //Play the sound
        audioSource.volume = sfxVolume * reloadSoundLevel;
        audioSource.clip = reloadAudioClip;
        audioSource.PlayOneShot(audioSource.clip);

        yield return new WaitForSecondsRealtime(reloadSpeed);
        currentAmmo = ammoCount;
        EventQueue.instance.DispatchEvent(new GunShootMessage(currentSpread, currentAmmo, false));
        reloading = false;
    }

    public void ResetAmmo()
    {
        currentAmmo = ammoCount;
        EventQueue.instance.DispatchEvent(new GunShootMessage(currentSpread, currentAmmo, false));
        reloading = false;
    }

    /*
    void RecoilHandler()
    {
        if (currentRecoil > 0)
        {
            Quaternion maxRotation = Quaternion.Euler(recoil, 0f, 0f);
            gunCam.transform.localRotation = Quaternion.Slerp(gunCam.transform.localRotation, maxRotation, Time.deltaTime * recoilSpeed);
            currentRecoil -= Time.deltaTime;
        }
        else
        {
            recoil = 0f;
            Quaternion minRoation = Quaternion.Euler(0f, 0f, 0f);
            gunCam.transform.localRotation = Quaternion.Slerp(gunCam.transform.localRotation, minRoation, Time.deltaTime * recoilSpeed / 2f);
        }

        Debug.Log(currentRecoil);
    }
    */

    public void Receive(IMessage message)
    {
        switch (message.GetMessageType())
        {
            //Disable shooting when menu opened
            case MessageType.MENU_OPENED:
                gamePaused = true;
                break;
            case MessageType.MENU_CLOSED:
                gamePaused = false;
                break;
            case MessageType.SFX_LEVEL_SET:
                SetSFXLevel((SoundLevelChangedMessage)message);
                break;
            case MessageType.PLAYER_KILLED:
                canFire = false;
                break;
        }
    }

    void SetSFXLevel(SoundLevelChangedMessage message)
    {
        sfxVolume = message.vol;
    }

    //https://www.youtube.com/watch?v=VKP9APfsRAk
    void SpawnDecal(RaycastHit hitInfo)
    {
        GameObject decal = Instantiate(blackBulletDecal);
        Destroy(decal, decalLifeSpan);
        decal.transform.position = hitInfo.point;
        decal.transform.forward = hitInfo.normal * -1f;
    }

    void EmbedDecal(RaycastHit hitInfo)
    {
        
        GameObject decal = Instantiate(redBulletDecal);
        Destroy(decal, decalLifeSpan);
        decal.transform.position = hitInfo.point;
        decal.transform.forward = hitInfo.normal * -1f;

        decal.transform.SetParent(hitInfo.collider.gameObject.transform);

    }

    void SpawnEmmiter(RaycastHit hit)
    {
        float ttl = 2f;
        GameObject prefab = brickWallEmmiter;
        if (hit.collider.tag.Equals("Wood"))
        {
            prefab = woodWallEmmiter;
        }
        if (hit.collider.tag.Equals("Explosive"))
        {
            prefab = explosiveEmmiter;
            ttl = 5f;
        }


        GameObject emmiter = Instantiate(prefab);
        emmiter.transform.position = hit.point;
        emmiter.transform.forward = hit.normal;
        Destroy(emmiter, ttl);
    }
}

