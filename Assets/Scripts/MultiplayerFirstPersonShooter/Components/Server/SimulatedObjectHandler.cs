using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

namespace ShardStudios {

    public class SimulatedObjectHandler : MonoBehaviour
    {
        
        #if SERVER

            private const int CACHE_SIZE = 1024;

            public static Dictionary<uint, Queue<InputState>> clientInputs = new Dictionary<uint, Queue<InputState>>();
            public static Dictionary<uint, SimulationState[]> simulationStates = new Dictionary<uint, SimulationState[]>();

            private void FixedUpdate(){

                foreach(KeyValuePair<uint, Queue<InputState>> inputs in clientInputs){
                    
                    NetworkedEntity networkedEntity = NetworkedEntity.Entities[inputs.Key];
                    MovementController movementController = networkedEntity.movementController;

                    Queue<InputState> queue = inputs.Value;
                    InputState inputState;

                    while( queue.Count > 0 ){

                        inputState = queue.Dequeue();

                        if( inputState.tick > networkedEntity.lastSimulatedTick ){

                            networkedEntity.lastSimulatedTick = inputState.tick;
                            if( inputState.jumping == (byte)1 ){
                                movementController.Jump();
                            }
                            
                            movementController.transform.rotation = inputState.rotation;
                            movementController.AddForce(new Vector3( inputState.input.x, 0f, inputState.input.y).normalized);
                            movementController.Move();

                            SimulationState simulationState = new SimulationState {
                                position = networkedEntity.transform.position,
                                velocity = movementController.velocity,
                                rotation = networkedEntity.transform.rotation,
                                tick = inputState.tick
                            };

                            simulationStates[inputs.Key][inputState.tick % CACHE_SIZE] = simulationState;

                        }
                    }

                }

                if( NetworkManager.tick % 2 == 0 ){
                    UpdateSimulationStateAllPlayers();
                }

            }


            // This could be better, revise in the future.
            public void UpdateSimulationStateAllPlayers(){

                foreach( KeyValuePair<uint, NetworkedEntity> entity in NetworkedEntity.Entities ){
                    if( entity.Value.lastSimulatedTick > 0 ){
                        Message message = Message.Create(MessageSendMode.unreliable, MessageID.ReceiveSimulationState);
                        message.AddUInt(entity.Key);
                        message.AddVector3(entity.Value.transform.position);
                        message.AddVector3(entity.Value.movementController.velocity);
                        message.AddQuaternion(entity.Value.transform.rotation);
                        message.AddUInt(NetworkManager.tick);
                        
                        NetworkManager.GameServer.Server.SendToAll(message);

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
                    tick = message.GetUInt()
                };

                if( clientInputs.ContainsKey(id) == false ){
                    clientInputs.Add(id, new Queue<InputState>());
                    simulationStates.Add(id, new SimulationState[CACHE_SIZE]);
                }

                clientInputs[id].Enqueue(receivedInputState);
            }

        #endif
        
    }

}
