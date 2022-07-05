using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RiptideNetworking;

namespace ShardStudios {

#if !SERVER

    public class GameClient
    {

        public Client Client { get; set; }
        

        public bool IsConnected(){
            return Client.IsConnected;
        }
        public void Start(){

            Client = new Client();
            Client.Connected += PlayerConnected;
            Client.ConnectionFailed += PlayerConnectionFailed;
            Client.Disconnected += PlayerDisconnected;
            Client.ClientDisconnected += OtherPlayerDisconnected;

        }

        public void PlayerConnected(object sender, EventArgs e){
            Debug.Log("Connected to GameServer!");
            NetworkManager.User.connectedToGameServer = true;
            NetworkManager.Instance.StartCoroutine(LoadMap(NetworkManager.User.game_mapname));
        }

        public void PlayerConnectionFailed(object sender, EventArgs e){
            Debug.Log("Connection to GameServer Failed!\nRetrying!");
            Client.Connect($"{NetworkManager.User.game_ip}:{NetworkManager.User.game_port}");
        }

        public void PlayerDisconnected(object sender, EventArgs e){
            NetworkManager.User.Reset();
        }

        public void OtherPlayerDisconnected(object sender, ClientDisconnectedEventArgs e){
            NetworkedEntity.CleanupPlayerOwnedEntities(e.Id);
            Player.playerList.Remove(e.Id);
            Debug.Log("Other player disconnected!");
        }

        IEnumerator LoadMap(string mapname){
            //Cleanup previous map things.
            AsyncOperation loading = SceneManager.LoadSceneAsync(mapname);
            while( !loading.isDone ){
                yield return null;
            }

            Message message = Message.Create(MessageSendMode.reliable, MessageID.PlayerReady);
            Client.Send(message);
        }

        public void Tick(){
            Client.Tick();
        }

        public void Kill(){
            Client.Disconnect();
            Client.Connected -= PlayerConnected;
            Client.ConnectionFailed -= PlayerConnectionFailed;
            Client.Disconnected -= PlayerDisconnected;
            Client.ClientDisconnected -= OtherPlayerDisconnected;
        }
    }

#endif
}
