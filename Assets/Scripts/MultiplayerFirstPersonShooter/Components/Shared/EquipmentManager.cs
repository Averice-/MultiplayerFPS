using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public enum EquipmentSlot {
        Primary = 0,
        Secondary,
        Melee,
        Utility1,
        Utility2,
        Utility3,
        Utility4,
        Objective,
        Misc1,
        Misc2,
        Misc3
    }

    public class EquipmentManager : MonoBehaviour
    {
        // Equipment Slots;
        Dictionary<EquipmentSlot, EquipmentItem> equipment = new Dictionary<EquipmentSlot, EquipmentItem>();

        int _selectedEquipment = 0;
        int _previousSelectedEquipment;
        public int selectedEquipment {
            get => _selectedEquipment;
            set {
                _previousSelectedEquipment = _selectedEquipment;
                _selectedEquipment = value;
            }
        }

        public Player owner;

        [SerializeField] Transform hands;
        [SerializeField] Transform[] attachmentSlots;
        
        void Start(){
            owner = Player.GetById(GetComponent<NetworkedPlayer>().ownerId);
        }

        // ItemInSlot(EquipmentSlot.Primary);
        public EquipmentItem ItemInSlot(EquipmentSlot slot){
            if( equipment.ContainsKey(slot) ){
                return equipment[slot];
            }
            return null;
        }

        public EquipmentItem RemoveFromSlot(EquipmentSlot slot){
            if( equipment.ContainsKey(slot) ){
                EquipmentItem item = equipment[slot];
                equipment.Remove(slot);
                return item;
            }
            return null;
        }

        public bool GiveItem(EquipmentItem item){
            if( ItemInSlot(item.slotType) != null ){
                return false;
            }
            equipment.Add(item.slotType, item);
            item.OnPickup(owner);

            if( (int)item.slotType == selectedEquipment ){
                EquipItem(item.slotType);
            }else{
                HolsterItem(item);
            }
            
            return true;
        }

        public EquipmentItem GetSelectedItem(){
            EquipmentItem selected = ItemInSlot((EquipmentSlot)selectedEquipment);
            if( selected != null ){
                return selected;
            }
            return null;
        }

        public void HolsterItem(EquipmentItem item){
            Transform slot = attachmentSlots[(int)item.slotType];
            if( slot != null ){
                item.transform.position = slot.position;
                item.transform.rotation = slot.rotation;
            }
            if( GetEquippedItem() == item ){
                item.OnUnequip();
            }
        }

        public EquipmentItem GetEquippedItem(){
            EquipmentItem slottedItem = ItemInSlot((EquipmentSlot)selectedEquipment);
            if( slottedItem != null ){
                return slottedItem;
            }
            return null;       
        }

        public bool EquipItem(EquipmentSlot slot){

            EquipmentItem equipped = GetEquippedItem();
            EquipmentItem itemToEquip = ItemInSlot(slot);

            if( itemToEquip != null ){

                if( equipped != null ){
                    HolsterItem(equipped);
                }
                itemToEquip.transform.position = hands.position;
                itemToEquip.transform.rotation = hands.rotation;
                itemToEquip.OnEquip();

                return true;

            }

            return false;

        }

        public void ChangeSelectedItem(EquipmentSlot nextItem){
            if( (int)nextItem == selectedEquipment || (int)nextItem == -1 )
                return;

            if( EquipItem(nextItem) ){
                selectedEquipment = (int)nextItem;
            }
        }

        public EquipmentItem GetNextBestSelectedItem(){
            // 7 being useable equipment slot count;
            for( int i = selectedEquipment; i < 7; i++ ){
                EquipmentItem nextBestItem = ItemInSlot((EquipmentSlot)i);
                if( nextBestItem != null ){
                    selectedEquipment = i;
                    return nextBestItem;
                }
            }

            return null;
        }

    }

}
