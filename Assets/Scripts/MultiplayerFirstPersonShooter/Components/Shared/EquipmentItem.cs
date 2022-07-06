using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public class EquipmentItem : MonoBehaviour
    {
        [Header("Equipment Settings")]
        public EquipmentSlot slotType = EquipmentSlot.Primary;
        public string equipmentName = "M4A1";
        public uint price = 3100;

        [Space(10)]
        [Header("ViewModel Settings")]
        public GameObject viewModel;
        public Vector3 positionOffset;
        public Quaternion rotationOffset;

        public virtual void OnPrimaryAttack(){
            Debug.Log("Pew Pew!");
        }

        public virtual void OnSecondaryAttack(){
            Debug.Log("Bang Bang!");
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

    }

}
