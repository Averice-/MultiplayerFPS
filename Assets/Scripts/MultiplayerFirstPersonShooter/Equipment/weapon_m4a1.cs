using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ShardStudios {

    public class weapon_m4a1 : EquipmentItem
    {

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
                owner.handler.animator.SetTrigger("Shoot");
                #if !SERVER
                    PlaySound(EquipmentSound.Shoot);
                    if( owner.isLocalPlayer )
                        crosshair.SpreadCrosshair();
                #endif
                lastShotTime = shotDelay;
            }else if( lastShotTime > 0f){
                lastShotTime -= Time.deltaTime;
            }

        }

        public override void OnEquip(){
            #if !SERVER
                PlaySound(EquipmentSound.Equip);
                if( owner.isLocalPlayer )
                    CreateCrosshair();
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
