using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Didascalia
{
    /// <summary>
    /// Gestiona el envío de JSONs con información de la sesión
    /// al servidor
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
        private WebDashboardManager _webDashboardManager;

        /// <summary>
        /// Objeto que reúne datos sobre la sesión en un diccionario
        /// Buffer exclusivo para el servidor remoto
        /// </summary>
        private GameData _gameData = new GameData();

        private string _sessionID = "0";
        private string _path;

        // Stream de escritura persistente en disco
        string _localDataPath;
        private StreamWriter _streamWriter;
        private bool _isFirstRecord = true;

        protected override void Awake()
        {
            _localDataPath = Path.Combine(Application.persistentDataPath, "SessionData");

            _destroyOnLoad = true;
            base.Awake();
        }

        void Start()
        {
            _webDashboardManager = WebDashboardManager.Instance;

            // creamos sesión SOLO en local con la sessionID por defecto si NO queremos usar el servidor
            // Si queremos usar el servidor, debemos esperar a que la sesión se inicialice por su cuenta
            //      y nos avise de cuándo crear la sesión
            if (storageMode == StorageMode.LocalOnly) CreateNewEntry(_sessionID);

            if (storageMode == StorageMode.RemoteServer || storageMode == StorageMode.Both)
            {
                InvokeRepeating("SendRemoteDataByTime", timer, timer);
            }
        }

        public void CreateNewEntry(string SessionID)
        {
            _sessionID = SessionID;
            _gameData.datas[_sessionID] = new List<BaseData>();
            _gameData.Session = _sessionID;

            if (storageMode == StorageMode.LocalOnly || storageMode == StorageMode.Both)
            {
                InitLocalStream();
            }
        }

        private void InitLocalStream()
        {
            try
            {
                if (!Directory.Exists(_localDataPath))
                {
                    Directory.CreateDirectory(_localDataPath);
                }

                string fileName = _sessionID + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ".json";
                _path = Path.Combine(_localDataPath, fileName);

                FileStream fileStream = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.Read);
                _streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                // Escribir cabecera con salto de línea
                _streamWriter.WriteLine("{\"" + _sessionID + "\": [");
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
                    if (_gameData != null && _gameData.datas.ContainsKey(_sessionID))
                    {
                        _gameData.datas[_sessionID].Add(data);

                        if (_gameData.datas[_sessionID].Count >= maxPlayerDataCount)
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
                if (_gameData.datas.ContainsKey(_sessionID) && _gameData.datas[_sessionID].Count > 0)
                {
                    SendBatchToRemote();
                }
            }
        }

        private void SendBatchToRemote()
        {
            if (_webDashboardManager != null)
            {
                string batchJsonText = _gameData.ToJson();
                _webDashboardManager.SendJsonData(batchJsonText);
                Debug.Log($"[GameDataManager] Lote remoto enviado ({_gameData.datas[_sessionID].Count} elementos).");
            }

            _gameData.datas[_sessionID].Clear();
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
                if (_gameData.datas.ContainsKey(_sessionID) && _gameData.datas[_sessionID].Count > 0)
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
                if (_gameData.datas.ContainsKey(_sessionID) && _gameData.datas[_sessionID].Count > 0)
                {
                    SendBatchToRemote();
                }

                // Cerrar el archivo local
                CloseLocalStream();
            }
        }
    }
}