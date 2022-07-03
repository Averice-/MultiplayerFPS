using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

namespace ShardStudios {

    public struct EntityReader {

        public uint id;
        public string resourceName;
        public Vector3 position;
        public Quaternion rotation;
        public ushort ownerId;

        public EntityReader(Message message){
            id = message.GetUInt();
            resourceName = message.GetString();
            position = message.GetVector3();
            rotation = message.GetQuaternion();
            ownerId = message.GetUShort();
        }

    }

    public class NetworkedEntity : MonoBehaviour
    {

        public static Dictionary<uint, NetworkedEntity> Entities = new Dictionary<uint, NetworkedEntity>();
        public static uint autoIncrementId = 0;

        public uint id { get; private set; }
        public string resourceName { get; private set; }
        public ushort ownerId { get; private set; }

        public uint lastSimulatedTick = 0;

        public MovementController movementController;

        #if SERVER

            

            public static GameObject Create(string entityName, Vector3 position, Quaternion rotation, ushort ownerId = 0){
                    
                GameObject newEntity = (GameObject)Instantiate(Resources.Load($"NetworkedEntities/{entityName}"), position, rotation);
                NetworkedEntity networkedEntity = newEntity.GetComponent<NetworkedEntity>();

                if( networkedEntity == null ){
                    Destroy(newEntity);
                    Debug.Log("Entity destroyed - Missing component [NetworkedEntity]");
                    return null;
                }

                autoIncrementId++;
                networkedEntity.id = autoIncrementId;
                networkedEntity.resourceName = entityName;
                networkedEntity.ownerId = ownerId;
                networkedEntity.movementController = networkedEntity.GetComponent<MovementController>();

                Message spawnEntityMessage = Message.Create(MessageSendMode.reliable, MessageID.NetworkedEntitySpawned);
                spawnEntityMessage.AddUInt(autoIncrementId);
                spawnEntityMessage.AddString(entityName);
                spawnEntityMessage.AddVector3(position);
                spawnEntityMessage.AddQuaternion(rotation);
                spawnEntityMessage.AddUShort(ownerId);

                NetworkManager.GameServer.Server.SendToAll(spawnEntityMessage);

                // OnSpawn event.

                Entities.Add(autoIncrementId, networkedEntity);

                return newEntity;
     
            }
            
            
        #else

            [MessageHandler((ushort)MessageID.NetworkedEntitySpawned)]
            private static void Spawn(Message message){
                    
                EntityReader entityInfo = new EntityReader(message);

                GameObject newEntity = (GameObject)Instantiate(Resources.Load($"NetworkedEntities/{entityInfo.resourceName}"), entityInfo.position, entityInfo.rotation);
                NetworkedEntity networkedEntity = newEntity.GetComponent<NetworkedEntity>();

                networkedEntity.id = entityInfo.id;
                networkedEntity.resourceName = entityInfo.resourceName;
                networkedEntity.ownerId = entityInfo.ownerId;
                networkedEntity.movementController = networkedEntity.GetComponent<MovementController>();

                // OnSpawn event.

                Entities.Add(entityInfo.id, networkedEntity);

            }

        #endif

        public static List<NetworkedEntity> GetEntitiesOfOwner(ushort ownerId){
            List<NetworkedEntity> result = new List<NetworkedEntity>();
            foreach( KeyValuePair<uint, NetworkedEntity> ent in Entities ){
                if( ent.Value.ownerId == ownerId ) {
                    result.Add(ent.Value);
                }
            }
            return result;
        }

        public static NetworkedEntity GetEntityById(uint id){
            if( Entities.ContainsKey(id) ){
                return Entities[id];
            }
            return null;
        }

        public virtual void OnSpawn(){

        }

        public void DestroyNetworkedEntity(){
            Entities.Remove(id);
            Destroy(this.gameObject);
        }

        // broadcast already made entities.
        // function to clear all networked entities.
        // remove players that have left the game from networkedEntities and playerList;
        // remove users that leave the master server
        // remove players inputstates&simulationstates from simulatedobjecthandler.
        // create gamemode class so you can get spawning players when ready finished for base gamemode.

        
    }

}
