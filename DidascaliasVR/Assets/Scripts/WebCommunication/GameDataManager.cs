using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Didascalia
{
    /// <summary>
    /// Gestiona el envío de JSONs con información de la sesión
    /// al servidor (a través del ConnectionManager)
    /// </summary>
    public class GameDataManager : Singleton<GameDataManager>
    {
        /// <summary>
        /// Referencia al ConnectionManager, para gestionar las conexiones
        /// con el servidor y envío de los JSONs
        /// </summary>
        private ConnectionManager _connectionManager;
        /// <summary>
        /// Objeto que reúne datos sobre la sesión en un diccionario
        /// </summary>
        private GameData _gameData = new GameData();

        private string _sessionID;

        private string _path;

        protected override void Awake()
        {
            _destroyOnLoad = true;
            base.Awake();
        }

        [SerializeField]
        private float timer = 5.0f;

        void Start()
        {
            _connectionManager = ConnectionManager.Instance;
            _sessionID = _connectionManager.SessionID;
            CreateNewEntry();
            
            InvokeRepeating("SendDataByTime", timer, timer);
        }

        private void CreateNewEntry()
        {
            _gameData.datas[_sessionID] = new List<BaseData>();
            _gameData.Session = _sessionID;
            string folderPath = Path.Combine(Application.persistentDataPath, "SessionData");

            // Crear la carpeta si no existe
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            _path = _sessionID + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ".json";
            _path = Path.Combine(folderPath, _path);
        }

        private void SendDataByTime()
        {
            lock (_gameData)
            {
                if (_gameData.datas[_sessionID].Count > 0)
                {
                    SendJSON();
                }
            }
        }

        private void SendJSON()
        {
            string text = _gameData.ToJson();
            _connectionManager.SendWebRequest(text);
            _gameData.datas[_sessionID].Clear();
        }

        //public void SendData(BaseData data)
        //{
        //    lock (gameData)
        //    {
        //        gameData.datas[Session].Add(data);
        //        if (gameData.datas[Session].Count >= maxPlayerDataCount)
        //        {
        //            string text = gameData.ToJson();
        //            client.sendJson(text);
        //            _ = WriterManager.Instance.WriteToStreamWriter(path, text);
        //            gameData.datas[Session].Clear();
        //        }
        //    }
        //}

        void OnDestroy()
        {
            Debug.Log("Destroying GameDataManager");

            if (_gameData.datas[_sessionID].Count > 0)
            {
                SendJSON();
                _gameData.datas.Clear();
            }
        }

    }
}