using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

namespace ShardStudios {

    public class SimulatedObjectHandler : MonoBehaviour
    {
        
        #if SERVER

            private const int CACHE_SIZE = 132; // 2 seconds worth of states.

            public static Dictionary<uint, Queue<InputState>> clientInputs = new Dictionary<uint, Queue<InputState>>();
            public static Dictionary<uint, SimulationState[]> simulationStates = new Dictionary<uint, SimulationState[]>();

            private void FixedUpdate(){

                foreach(KeyValuePair<uint, Queue<InputState>> inputs in clientInputs){
                    
                    NetworkedPlayer networkedEntity = (NetworkedPlayer)NetworkedEntity.Entities[inputs.Key];
                    MovementController movementController = networkedEntity.movementController;

                    Queue<InputState> queue = inputs.Value;
                    InputState inputState;

                    if( queue.Count > 0 ){

                        inputState = queue.Dequeue();

                        if( inputState.tick > networkedEntity.lastSimulatedTick ){

                            networkedEntity.lastSimulatedTick = inputState.tick;
                            if( inputState.jumping == (byte)1 ){
                                movementController.Jump();
                            }
                            
                            movementController.transform.rotation = inputState.rotation;
                            movementController.AddForce(new Vector3( inputState.input.x, 0f, inputState.input.y).normalized);
                            //movementController.Move();

                            //Physics.SyncTransforms();

                            UpdateOwnState(networkedEntity, inputState.tick);

                        }
                    }

                    SimulationState simulationState = new SimulationState {
                        position = networkedEntity.transform.position,
                        velocity = movementController.velocity,
                        rotation = networkedEntity.transform.rotation,
                        tick = NetworkManager.tick
                    };

                    simulationStates[inputs.Key][NetworkManager.tick % CACHE_SIZE] = simulationState;

                }

                if( NetworkManager.tick % 2 == 0 ){
                    UpdateSimulationStateAllPlayers();
                }


            }


            public void UpdateOwnState(NetworkedPlayer networkedEntity, uint tick){

                Message message = Message.Create(MessageSendMode.unreliable, MessageID.ReceiveOwnSimulationState);
                message.AddUInt(networkedEntity.id);
                message.AddVector3(networkedEntity.transform.position);
                message.AddVector3(networkedEntity.movementController.velocity);
                message.AddQuaternion(networkedEntity.transform.rotation);
                message.AddUInt(tick);

                NetworkManager.GameServer.Server.Send(message, networkedEntity.ownerId);
            }

            // This could be better, revise in the future.
            public void UpdateSimulationStateAllPlayers(){
                
                foreach( KeyValuePair<uint, NetworkedEntity> entity in NetworkedEntity.Entities ){

                    if( entity.Value.isNetworkedPlayer ){

                        NetworkedPlayer player = (NetworkedPlayer)entity.Value;
                        Message message = Message.Create(MessageSendMode.unreliable, MessageID.ReceiveSimulationState);
                        message.AddUInt(entity.Key);
                        message.AddVector3(player.transform.position);
                        message.AddVector3(player.movementController.velocity);
                        message.AddQuaternion(player.transform.rotation);
                        message.AddUInt(NetworkManager.tick);
                        
                        NetworkManager.GameServer.Server.SendToAll(message, player.ownerId);

                    }

                }

            }

            [MessageHandler((ushort)MessageID.ClientSendInput)]
            private static void ClientSendInput(ushort from, Message message){

                uint id = message.GetUInt();
                InputState receivedInputState = new InputState{
                    input = message.GetVector2(),
                    rotation = message.GetQuaternion(),
                    jumping = message.GetByte(),
                    primaryAttack = message.GetByte(),
                    secondaryAttack = message.GetByte(),
                    tick = message.GetUInt()
                };

                if( clientInputs.ContainsKey(id) == false ){
                    clientInputs.Add(id, new Queue<InputState>());
                    simulationStates.Add(id, new SimulationState[CACHE_SIZE]);
                }

                clientInputs[id].Enqueue(receivedInputState);
            }

            [MessageHandler((ushort)MessageID.PlayerChangeWeapon)]
            private static void PlayerChangeWeapon(ushort from, Message message){
                Player player = Player.GetById(from);
                int weapon = message.GetInt();
                if( player.equipment != null ){
                    player.equipment.ChangeSelectedItem((EquipmentSlot)weapon);

                    Message newMessage = Message.Create(MessageSendMode.reliable, MessageID.PlayerUpdateSelectedWeapon);
                    newMessage.AddUShort(player.id);
                    newMessage.AddInt(weapon);

                    NetworkManager.GameServer.Server.SendToAll(newMessage);
                }

            }

        #endif
        
    }

}
