using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public class NetworkedPlayer : NetworkedEntity
    {

        public uint lastSimulatedTick = 0;
        public MovementController movementController;
        public Player player; 


        public void Start(){
            isNetworkedPlayer = true;
            movementController = GetComponent<MovementController>();

            player = GetOwner();
        }

        public override void OnSpawn(){

        }

    }

}
