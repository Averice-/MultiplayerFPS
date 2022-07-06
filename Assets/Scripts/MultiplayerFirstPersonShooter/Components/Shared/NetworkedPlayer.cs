using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public class NetworkedPlayer : NetworkedEntity
    {

        public uint lastSimulatedTick = 0;
        public MovementController movementController;

        public void Start(){
            isNetworkedPlayer = true;
            movementController = GetComponent<MovementController>();
        }

        public override void OnSpawn(){
            Debug.Log("SPAWNED");
            Player player = GetOwner();
            if( player != null ){
                player.hand = transform.GetChild(1);
                GameObject myGun = (GameObject)Instantiate(Resources.Load("Equipment/M4A1"), player.hand.position, player.hand.rotation);
                myGun.transform.parent = player.hand;
            }
        }

    }

}
