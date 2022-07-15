using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ShardStudios {

    public class weapon_pistol : EquipmentItem
    {

        bool hasShot = false;

        public override void OnPrimaryAttack(bool isAttacking){
            if( isAttacking ){

                if( lastShotTime <= 0f && canShoot && !hasShot ){
                    Bullet shot = new Bullet(owner, eyePosition.position, eyePosition.forward, damageAmount);
                    #if !SERVER
                        PlaySound(EquipmentSound.Shoot);
                    #endif
                    lastShotTime = shotDelay;
                    hasShot = true;
                }

                return;
            
            }

            hasShot = false;
        }

        void Update(){
            if( equipTime > 0f ){
                equipTime -= Time.deltaTime;
            }else{
                canShoot = true;
            }

            if( lastShotTime <= 0f ){
                lastShotTime -= Time.deltaTime;
            }
        }

        public override void OnEquip(){
            #if !SERVER
                PlaySound(EquipmentSound.Equip);
            #endif
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
