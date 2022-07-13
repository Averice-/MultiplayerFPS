using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ShardStudios {

    public class weapon_m4a1 : EquipmentItem
    {
        
        #if !SERVER

            AudioSource audioSource;

            void Awake(){
                audioSource = GetComponent<AudioSource>();
            }

            void PlayShootSFX(){
                audioSource.Stop();
                audioSource.Play();
            }

        #endif

        public override void OnPrimaryAttack(bool isAttacking = true){
            isShooting = isAttacking;
        }

        void Update(){

            if( equipTime > 0f ){
                equipTime -= Time.deltaTime;
            }else{
                canShoot = true;
            }

            if( isShooting && lastShotTime <= 0f && canShoot ){
                Bullet shot = new Bullet(owner, eyePosition.position, eyePosition.forward, damageAmount);
                #if !SERVER
                    PlayShootSFX();
                #endif
                lastShotTime = shotDelay;
            }else if( lastShotTime > 0f){
                lastShotTime -= Time.deltaTime;
            }

        }

        public override void OnEquip(){
            equipTime = equipShootDelay;
            canShoot = false;
        }

        public override void OnUnequip(){
            canShoot = false;
            isShooting = false;
            lastShotTime = 0f;
        }
    }

}
