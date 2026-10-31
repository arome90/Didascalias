using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Didascalia
{
    /// <summary>
    /// Gestiona el envío de JSONs con información de la sesión
    /// al servidor (a través del ConnectionManager)
    /// </summary>
    public class GameDataManager : Singleton<GameDataManager>
    {
        public enum StorageMode
        {
            RemoteServer,
            LocalOnly,
            Both
        }

        [Header("Modo de Almacenamiento")]
        [SerializeField] private StorageMode storageMode = StorageMode.LocalOnly;

        [Header("Configuración de Envío Remoto")]
        [SerializeField] private float timer = 5.0f;
        [SerializeField] private int maxPlayerDataCount = 50;

        /// <summary>
        /// Referencia al ConnectionManager, para gestionar las conexiones
        /// con el servidor y envío de los JSONs
        /// </summary>
        private WebDashboardManager _connectionManager;

        /// <summary>
        /// Objeto que reúne datos sobre la sesión en un diccionario
        /// Buffer exclusivo para el servidor remoto
        /// </summary>
        private GameData _gameData = new GameData();

        private string sessionID { get { return "0"; } }
        private string _path;

        // Stream de escritura persistente en disco
        private StreamWriter _streamWriter;
        private bool _isFirstRecord = true;

        protected override void Awake()
        {
            _destroyOnLoad = true;
            base.Awake();
        }

        void Start()
        {
            _connectionManager = WebDashboardManager.Instance;

            CreateNewEntry();

            if (storageMode == StorageMode.RemoteServer || storageMode == StorageMode.Both)
            {
                InvokeRepeating("SendRemoteDataByTime", timer, timer);
            }
        }

        public void CreateNewEntry()
        {
            _gameData.datas[sessionID] = new List<BaseData>();
            _gameData.Session = sessionID;

            if (storageMode == StorageMode.LocalOnly || storageMode == StorageMode.Both)
            {
                InitLocalStream();
            }
        }

        private void InitLocalStream()
        {
            try
            {
                string folderPath = Path.Combine(Application.persistentDataPath, "SessionData");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = sessionID + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ".json";
                _path = Path.Combine(folderPath, fileName);

                FileStream fileStream = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.Read);
                _streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                // Escribir cabecera con salto de línea
                _streamWriter.WriteLine("{\"" + sessionID + "\": [");
                _streamWriter.Flush();

                _isFirstRecord = true;
                Debug.Log($"[GameDataManager] Stream local iniciado en: {_path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameDataManager] Error al inicializar StreamWriter: {ex.Message}");
            }
        }

        public void SendData(BaseData data)
        {
            lock (_gameData)
            {
                // Escritura local instantánea
                if (storageMode == StorageMode.LocalOnly || storageMode == StorageMode.Both)
                {
                    AppendRecordToLocalStream(data);
                }

                // Acumulación en buffer para lote remoto
                if (storageMode == StorageMode.RemoteServer || storageMode == StorageMode.Both)
                {
                    if (_gameData != null && _gameData.datas.ContainsKey(sessionID))
                    {
                        _gameData.datas[sessionID].Add(data);

                        if (_gameData.datas[sessionID].Count >= maxPlayerDataCount)
                        {
                            SendBatchToRemote();
                        }
                    }
                }
            }
        }

        private void AppendRecordToLocalStream(BaseData data)
        {
            if (_streamWriter == null) return;

            try
            {
                // Convertir el elemento a JSON con formato multilínea
                string itemJson = JsonUtility.ToJson(data, true);

                // Indentación
                string indentedJson = "    " + itemJson.Replace("\n", "\n    ");

                if (!_isFirstRecord)
                {

                    _streamWriter.WriteLine(",");
                }
                else
                {
                    _streamWriter.WriteLine();
                    _isFirstRecord = false;
                }

                _streamWriter.Write(indentedJson);

                // Vaciado rápido de buffer de SO a disco
                _streamWriter.Flush();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameDataManager] Error al escribir en StreamWriter: {ex.Message}");
            }
        }

        private void SendRemoteDataByTime()
        {
            lock (_gameData)
            {
                if (_gameData.datas.ContainsKey(sessionID) && _gameData.datas[sessionID].Count > 0)
                {
                    SendBatchToRemote();
                }
            }
        }

        private void SendBatchToRemote()
        {
            if (_connectionManager != null)
            {
                string batchJsonText = _gameData.ToJson();
                _connectionManager.SendWebRequest(batchJsonText);
                Debug.Log($"[GameDataManager] Lote remoto enviado ({_gameData.datas[sessionID].Count} elementos).");
            }

            _gameData.datas[sessionID].Clear();
        }

        private void CloseLocalStream()
        {
            if (_streamWriter != null)
            {
                try
                {
                    // Cerrar la estructura JSON correctamente
                    _streamWriter.WriteLine();
                    _streamWriter.Write("]}");
                    _streamWriter.Flush();
                    _streamWriter.Close();
                    _streamWriter.Dispose();
                    _streamWriter = null;

                    Debug.Log("[GameDataManager] StreamWriter cerrado limpiamente.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GameDataManager] Error al cerrar StreamWriter: {ex.Message}");
                }
            }
        }

        private void ShutdownManager()
        {
            lock (_gameData)
            {
                if (_gameData.datas.ContainsKey(sessionID) && _gameData.datas[sessionID].Count > 0)
                {
                    SendBatchToRemote();
                }

                CloseLocalStream();
            }
        }

        private void OnApplicationQuit()
        {
            Debug.Log("[GameDataManager] Cierre de aplicación detectado (OnApplicationQuit)...");
            ShutdownManager();
        }

        void OnDestroy()
        {
            Debug.Log("[GameDataManager] Finalizando sesión...");

            lock (_gameData)
            {
                // Enviar cualquier dato remoto restante
                if (_gameData.datas.ContainsKey(sessionID) && _gameData.datas[sessionID].Count > 0)
                {
                    SendBatchToRemote();
                }

                // Cerrar el archivo local
                CloseLocalStream();
            }
        }
    }
}