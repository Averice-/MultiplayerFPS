using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public class Item : MonoBehaviour
    {

        public string itemName;

        public virtual void OnUse(Player player){
            Debug.Log($"Using item[{itemName}]");
        }
        
    }

}
