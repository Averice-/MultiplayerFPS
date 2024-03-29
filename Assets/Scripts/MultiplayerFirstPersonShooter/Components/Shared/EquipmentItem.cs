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
        public string weaponAnimType = "EquipAssaultRifle";
        public uint price = 3100;
        public float shotDelay = 0.1f;
        public float equipShootDelay = 0.3f;
        public float damageAmount = 35f;
        public string crosshairName = "CrosshairDefault";

        #if !SERVER
            [Space(10)]
            [Header("ViewModel Settings")]
            public GameObject viewModel;
            public Vector3 positionOffset = new Vector3(0.3f, 0.1f, 0.7f);
            public Quaternion rotationOffset = Quaternion.identity;

            [Space(10)]
            [Header("Sounds")]
            [SerializeField] AudioSource audioSource;
            [SerializeField] AudioClip[] audioClips = new AudioClip[5];

            [Space(10)]
            [Header("Icon Settings")]
            public Sprite weaponIcon;

        #endif

        protected bool isShooting = false;
        protected bool canShoot = true;
        protected float lastShotTime = 0f;
        protected float equipTime = 0f;
        protected Player owner;
        protected Crosshair crosshair;

        protected Transform eyePosition;

        public void SetEyePosition(Transform eyePos){
            eyePosition = eyePos;
        }

        #if !SERVER
            public void PlaySound(EquipmentSound equipmentSound){
                int soundIndex = (int)equipmentSound;
                if( audioClips[soundIndex] != null ){
                    audioSource.PlayOneShot(audioClips[soundIndex], 0.3f);
                }
            }

            public void PositionViewModel(){ // Eventually this will place weapon in viewmodel hands.
                if( viewModel != null ){
                    viewModel.SetActive(true);
                    //Transform cameraTransform = CameraController.GetCameraTransform();
                    if( viewModel.transform.parent != eyePosition ){

                        viewModel.transform.position = eyePosition.position;
                        viewModel.transform.localPosition += positionOffset;
                        viewModel.transform.rotation = eyePosition.rotation;// + rotationOffset;
                        viewModel.transform.parent = eyePosition; // First person camera controller.

                    }
                }
            }

            public void HideViewModel(){
                if( viewModel != null ){
                    viewModel.SetActive(false);
                }
            }
        #endif

        public void SetOwner(Player player){
            owner = player;
        }

        public Player GetOwner(){
            return owner;
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

        #if !SERVER
            public virtual void CreateCrosshair(){
                crosshair = PlayerHud.CreateCrosshair(crosshairName);
            }
        #endif

    }

}
