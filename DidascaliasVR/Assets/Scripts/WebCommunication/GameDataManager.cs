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
        private WebDashboardManager _connectionManager;
        /// <summary>
        /// Objeto que reúne datos sobre la sesión en un diccionario
        /// </summary>
        private GameData _gameData = new GameData();

        private int maxPlayerDataCount = 50;

        // TODO: Cambiar para código genérico
        private string sessionID { get { return "0"; } }

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
            _connectionManager = WebDashboardManager.Instance;

            // TODO: Crear entrada en Local
            // CreateNewEntry();


            // TODO: Esto es solo para la web de Barcelona
            if (_connectionManager != null)
            {
                if (sessionID != null && !_gameData.datas.ContainsKey(sessionID)) { CreateNewEntry(); }
                InvokeRepeating("SendDataByTime", timer, timer);
            }
            
        }

        public void CreateNewEntry()
        {
            _gameData.datas[sessionID] = new List<BaseData>();
            _gameData.Session = sessionID;
            string folderPath = Path.Combine(Application.persistentDataPath, "SessionData");

            // Crear la carpeta si no existe
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            _path = sessionID + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ".json";
            _path = Path.Combine(folderPath, _path);
        }

        public void SendData(BaseData data)
        {
            if (_connectionManager == null) return;

            lock (_gameData)
            {
                if (_gameData != null && _gameData.datas != null  && _gameData.datas.Count > 0)
                {
                    _gameData.datas[sessionID].Add(data);
                    if (_gameData.datas[sessionID].Count >= maxPlayerDataCount)
                    {
                        SendJSON();
                        // _ = WriterManager.Instance.WriteToStreamWriter(path, text);
                        // gameData.datas[Session].Clear();
                    }
                }
            }
        }

        private void SendDataByTime()
        {
            if (sessionID == null) return;

            lock (_gameData)
            {
                if (_gameData.datas[sessionID].Count > 0)
                {
                    SendJSON();
                }
            }
        }

        private void SendJSON()
        {
            lock (_gameData)
            {
                string text = _gameData.ToJson();
                // TODO: un if 'local' que te mande los archivos a una ruta configurada en local :)

                // if !local jeje
                _connectionManager.SendWebRequest(text);
                _gameData.datas[sessionID].Clear();
            }
        }

        void OnDestroy()
        {
            Debug.Log("Destroying GameDataManager");

            if (_gameData.datas[sessionID].Count > 0)
            {
                SendJSON();
                _gameData.datas.Clear();
            }
        }

    }
}