using ClassRoomVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        private int maxPlayerDataCount = 50; // N�mero m�ximo de PlayerData antes de enviar
        private string Session;

        private string path;

        [SerializeField]
        private float timer = 5.0f;
        void Start()
        {
            ws = WsClient.Instance;
            client = HttpClient.Instance;
            Session = ws.Session;
            Debug.Log("Creating GameDataManager for session " + Session);
            gameData.datas[Session] = new List<BaseData>();
            gameData.Session = Session;
            string folderPath = Path.Combine(Application.persistentDataPath, "SessionData");

            // Crear la carpeta si no existe
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            path = Session+"_"+ DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ".json";
            path = Path.Combine(folderPath, path);
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
                    string text = gameData.ToJson();
                    client.sendJson(text);
                    _ = WriterManager.Instance.WriteToStreamWriter(path, text);
                    gameData.datas[Session].Clear();
                }
            }
        }

        void OnDestroy()
        {
            Debug.Log("Destroying GameDataManager");

            if (gameData.datas[Session].Count > 0)
            {
                client.sendJson(gameData.ToJson());
                gameData.datas[Session].Clear();
                gameData.datas.Clear();
            }
        }

    }
}