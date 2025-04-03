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
        private int maxPlayerDataCount = 5; // N�mero m�ximo de PlayerData antes de enviar
        private string Session;

        [SerializeField]
        private float timer = 5.0f;
        void Start()
        {
            ws = WsClient.Instance;
            client = HttpClient.Instance;
            Session = ws.Session;
            gameData.datas[Session] = new List<BaseData>();
            gameData.Session = Session;
            InvokeRepeating("SendDataByTime", timer, timer);

        }

        private void SendDataByTime()
        {
            lock (gameData)
            {
                if (gameData.datas[Session].Count > 0)
                {
                    client.sendJson(gameData.ToJson());
                    gameData.datas[Session].Clear();
                }
            }
        }

        public void SendData(BaseData data)
        {
            lock (gameData)
            {
                gameData.datas[Session].Add(data);
                if (gameData.datas[Session].Count >= maxPlayerDataCount)
                {
                    client.sendJson(gameData.ToJson());
                    gameData.datas[Session].Clear();
                }
            }
        }

        void OnDestroy()
        {
            if (gameData.datas[Session].Count > 0)
            {
                client.sendJson(gameData.ToJson());
                gameData.datas[Session].Clear();
                gameData.datas.Clear();
            }
        }

    }
}