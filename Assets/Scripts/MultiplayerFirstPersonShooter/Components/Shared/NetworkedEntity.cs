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

        public bool isNetworkedPlayer = false;

        #if SERVER

            

            public static NetworkedEntity Create(string entityName, Vector3 position, Quaternion rotation, ushort ownerId = 0){
                    
                GameObject newEntity = (GameObject)Instantiate(Resources.Load($"NetworkedEntities/{entityName}"), position, rotation);
                NetworkedEntity networkedEntity = newEntity.GetComponent<NetworkedEntity>();

                if( networkedEntity == null ){
                    Destroy(newEntity);
                    Debug.Log("Entity destroyed - Missing component [NetworkedEntity]");
                    return null;
                }

                Debug.Log($"[SERVER] Entity spawned at[{position}] arrived at[{newEntity.transform.position}]");

                autoIncrementId++;
                networkedEntity.id = autoIncrementId;
                networkedEntity.resourceName = entityName;
                networkedEntity.ownerId = ownerId;
                //networkedEntity.movementController = networkedEntity.GetComponent<MovementController>();

                Message spawnEntityMessage = Message.Create(MessageSendMode.reliable, MessageID.NetworkedEntitySpawned);
                spawnEntityMessage.AddUInt(autoIncrementId);
                spawnEntityMessage.AddString(entityName);
                spawnEntityMessage.AddVector3(position);
                spawnEntityMessage.AddQuaternion(rotation);
                spawnEntityMessage.AddUShort(ownerId);

                NetworkManager.GameServer.Server.SendToAll(spawnEntityMessage);

                Entities.Add(autoIncrementId, networkedEntity);

                networkedEntity.OnSpawn();

                return networkedEntity;
     
            }

            public static void Broadcast(ushort to){
                
                Debug.Log($"Broadcasting active entities to player[{to}]");
                foreach( KeyValuePair<uint, NetworkedEntity> entity in Entities ){

                    Message message = Message.Create(MessageSendMode.reliable, MessageID.NetworkedEntitySpawned);
                    message.AddUInt(entity.Key);
                    message.AddString(entity.Value.resourceName);
                    message.AddVector3(entity.Value.transform.position);
                    message.AddQuaternion(entity.Value.transform.rotation);
                    message.AddUShort(entity.Value.ownerId);

                    NetworkManager.GameServer.Server.Send(message, to);

                }

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
                //networkedEntity.movementController = networkedEntity.GetComponent<MovementController>();
                Debug.Log($"Spawned at[{entityInfo.position}].. Arrived at[{networkedEntity.transform.position}]");
                
                networkedEntity.OnSpawn();

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

        public static void CleanupPlayerOwnedEntities(ushort id){
            List<NetworkedEntity> playerOwnedEntities = GetEntitiesOfOwner(id);
            foreach( NetworkedEntity entity in playerOwnedEntities ){
                entity.DestroyNetworkedEntity();
                #if SERVER
                    SimulatedObjectHandler.clientInputs.Remove(entity.id);
                    SimulatedObjectHandler.simulationStates.Remove(entity.id);
                #endif
            }
        }

        public Player GetOwner(){
            return Player.GetById(this.ownerId);
        }

        public static void CleanupEntities(){
            foreach( KeyValuePair<uint, NetworkedEntity> ent in Entities ){
                ent.Value.DestroyNetworkedEntity();
            }
        }

        public virtual void OnSpawn(){
        }

        public void DestroyNetworkedEntity(){
            Entities.Remove(id);
            Destroy(this.gameObject);
        }

        
    }

}
