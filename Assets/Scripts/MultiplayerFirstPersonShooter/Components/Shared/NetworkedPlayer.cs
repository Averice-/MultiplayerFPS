using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public class NetworkedPlayer : NetworkedEntity
    {

        public uint lastSimulatedTick = 0;
        public MovementController movementController;
        public Player player; 
        public Transform playerModel;


        public void Start(){
            isNetworkedPlayer = true;
            movementController = GetComponent<MovementController>();

            player = GetOwner();
            
            #if !SERVER

                if( player.isLocalPlayer )
                    playerModel.gameObject.SetActive(false);
                    
            #endif
        }

        public override void OnSpawn(){

        }

    }

}
