using ClassRoomVR;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using UnityEngine;

namespace ClassRoomVR
{
    public class GameDataManager : SceneSingleton<GameDataManager>
    {
        private WsClient ws;
        private HttpClient client;
        private GameData gameData = new GameData();
        private int maxPlayerDataCount = 50; // Número máximo de PlayerData antes de enviar
        private string Session;

        [SerializeField]
        private float timer = 5.0f;
        void Start()
        {
            ws = WsClient.Instance;
            client = HttpClient.Instance;
            Session = ws.Session;
            InvokeRepeating("SendDataByTime", timer, timer);

        }

        private void SendDataByTime()
        {
            lock (gameData)
            {
                if (gameData.Players[Session].Count > 0)
                {
                    client.sendJson(gameData.ToJson());
                    gameData.Players.Clear();
                }
            }
        }

        public void SendData(PlayerData data)
        {
            gameData.Players[Session].Add(data);
            lock (gameData)
            {
                if (gameData.Players[Session].Count >= maxPlayerDataCount)
                {
                    client.sendJson(gameData.ToJson());
                    gameData.Players.Clear();
                }
            }
        }

        void OnDestroy()
        {
            if (gameData.Players[Session].Count > 0)
            {
                client.sendJson(gameData.ToJson());
                gameData.Players.Clear();
            }
        }

    }
}