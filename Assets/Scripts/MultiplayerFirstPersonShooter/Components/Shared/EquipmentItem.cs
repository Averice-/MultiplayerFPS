using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public enum EquipmentSound {
        Equip = 0,
        Shoot,
        Secondary,
        Reload,
        Unequip
    }

    public class EquipmentItem : MonoBehaviour
    {
        [Header("Equipment Settings")]
        public EquipmentSlot slotType = EquipmentSlot.Primary;
        public string equipmentName = "M4A1";
        public string codeName = "weapon_m4a1";
        public uint price = 3100;
        public float shotDelay = 0.1f;
        public float equipShootDelay = 0.3f;
        public float damageAmount = 35f;

        #if !SERVER
            [Space(10)]
            [Header("ViewModel Settings")]
            public GameObject viewModel;
            public Vector3 positionOffset;
            public Quaternion rotationOffset;

            [Space(10)]
            [Header("Sounds")]
            [SerializeField] AudioSource audioSource;
            [SerializeField] AudioClip[] audioClips = new AudioClip[5];
        #endif

        protected bool isShooting = false;
        protected bool canShoot = true;
        protected float lastShotTime = 0f;
        protected float equipTime = 0f;
        protected Player owner;

        protected Transform eyePosition;

        public void SetEyePosition(Transform eyePos){
            eyePosition = eyePos;
        }

        #if !SERVER
            public void PlaySound(EquipmentSound equipmentSound){
                int soundIndex = (int)equipmentSound;
                if( audioClips[soundIndex] != null ){
                    audioSource.PlayOneShot(audioClips[soundIndex]);
                }
            }
        #endif

        public void SetOwner(Player player){
            owner = player;
        }

        public virtual void OnPrimaryAttack(bool isAttacking = true){
            isShooting = isAttacking;
        }


        public virtual void OnSecondaryAttack(){
            Debug.Log("Bang Bang!");
        }

        public virtual void OnSecondaryAttackCancelled(){

        }

        public virtual void OnEquip(){
            Debug.Log($"Equipped[{equipmentName}]");
        }

        public virtual void OnUnequip(){
            Debug.Log($"Unequipped[{equipmentName}]");
        }

        public virtual void OnDrop(){
            Debug.Log($"Dropped[{equipmentName}]");
        }

        public virtual void OnPickup(Player player){
        }

    }

}
