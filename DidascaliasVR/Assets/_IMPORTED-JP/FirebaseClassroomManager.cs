using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FirebaseClassroomManager : Singleton<FirebaseClassroomManager>
{
    public struct StartRoomResult
    {
        public string errorText;
        public bool wasSuccesful;
        public ClassSettings settings;
        public DatabaseReference database;
    }

    [Header("Firebase Configurations")]
    [Tooltip("A ID da sala atual no banco de dados")]
    [HideInInspector]
    public string ClassroomID = "";

    private DatabaseReference _dbReference;
    private bool _firebaseReady = false;

    // Fila para rodar comandos do Firebase na Thread Principal
    private readonly Queue<Action> _mainThreadActions = new Queue<Action>();

    // Variáveis para guardar o pedido de sessão caso o Firebase atrase
    private bool _pendingSessionStart = false;
    private List<Student> _pendingStudents = null;

    protected override void Awake()
    {
        base.Awake();

        // Tenta carregar a ID salva localmente (fallback rápido)
        LoadSessionID();

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                _dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                _firebaseReady = true;
                Debug.Log("<color=#00FF00>[Firebase]</color> Inicializado com sucesso.");

                // Se não temos a ID da sala na memória, faz a busca dinâmica no Firebase
                if (string.IsNullOrEmpty(ClassroomID))
                {
                    FetchClassroomIDFromFirebase(() =>
                    {
                        if (_pendingSessionStart)
                        {
                            StartFirebaseSession(_pendingStudents);
                        }
                    });
                }
                else if (_pendingSessionStart)
                {
                    StartFirebaseSession(_pendingStudents);
                }
            }
            else
            {
                Debug.LogError($"[Firebase] Erro de dependência: {task.Result}");
            }
        });
    }

    /// <summary>
    /// Consulta diretamente o Firebase para identificar a sala associada ao ID deste óculos.
    /// Elimina a dependência estrita do PlayerPrefs.
    /// </summary>
    public void FetchClassroomIDFromFirebase(Action onComplete = null)
    {
        if (!_firebaseReady || _dbReference == null) return;

        string idDoOculos = SystemInfo.deviceUniqueIdentifier;

        _dbReference.Child("classroom_configs").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists && task.Result.ChildrenCount > 0)
            {
                DataSnapshot mostRecentRoom = null;
                string greatestDateCreated = "";

                foreach (DataSnapshot sala in task.Result.Children)
                {
                    if (sala.HasChild("activeHeadsetId") && sala.Child("activeHeadsetId").Value.ToString() == idDoOculos)
                    {
                        if (sala.HasChild("criadoEm"))
                        {
                            string dateCreated = sala.Child("criadoEm").Value.ToString();
                            if (string.Compare(dateCreated, greatestDateCreated) >= 0)
                            {
                                greatestDateCreated = dateCreated;
                                mostRecentRoom = sala;
                            }
                        }
                        else if (mostRecentRoom == null)
                        {
                            mostRecentRoom = sala;
                        }
                    }
                }

                if (mostRecentRoom != null)
                {
                    ClassroomID = mostRecentRoom.Key;
                    SaveSessionID(ClassroomID);
                    Debug.Log($"<color=#00FF00>[Firebase]</color> Sala identificada via Query direta: {ClassroomID}");
                }
                else
                {
                    Debug.LogWarning($"<color=orange>[Firebase]</color> Nenhuma sala ativa encontrada para o dispositivo {idDoOculos}.");
                }
            }

            onComplete?.Invoke();
        });
    }

    private void Update()
    {
        while (_mainThreadActions.Count > 0)
        {
            _mainThreadActions.Dequeue()?.Invoke();
        }
    }

    public void StartFirebaseSession(List<Student> students)
    {
        // Se a conexão ou a ID da sala ainda não estiverem prontas, realiza a busca assíncrona
        if (!_firebaseReady || _dbReference == null || string.IsNullOrEmpty(ClassroomID))
        {
            _pendingSessionStart = true;
            _pendingStudents = students;
            Debug.Log("<color=#FFFF00>[Firebase]</color> Aguardando conexão/identificação da sala para enviar os dados...");

            if (_firebaseReady && string.IsNullOrEmpty(ClassroomID))
            {
                FetchClassroomIDFromFirebase(() =>
                {
                    if (_pendingSessionStart && !string.IsNullOrEmpty(ClassroomID))
                    {
                        StartFirebaseSession(_pendingStudents);
                    }
                });
            }
            return;
        }

        _pendingSessionStart = false;
        DatabaseReference roomRef = _dbReference.Child("classroom_configs").Child(ClassroomID);

        roomRef.Child("situacao_atual").Child("ativo").SetValueAsync("sim");

        Dictionary<string, object> alunosDict = new Dictionary<string, object>();
        for (int i = 0; i < students.Count; i++)
        {
            Student student = students[i];
            string condition = "Típico"; // Default

            if (student.StType == StudentType.ADHD)
            {
                condition = "ADHD";
            }
            else if (student.StType == StudentType.Autistic)
            {
                condition = "TEA";
            }

            Dictionary<string, string> studentData = new Dictionary<string, string>();
            studentData["nome"] = student.Name;
            studentData["condicao"] = condition;

            alunosDict["Aluno" + (i + 1)] = studentData;
        }
        roomRef.Child("Alunos").SetValueAsync(alunosDict);

        Debug.Log("<color=#00FFFF>[Firebase]</color> Sessão iniciada, Alunos enviados e escutando comandos.");
    }

    public void EndFirebaseSession()
    {
        if (!_firebaseReady || _dbReference == null || string.IsNullOrEmpty(ClassroomID)) return;

        DatabaseReference roomRef = _dbReference.Child("classroom_configs").Child(ClassroomID);

        roomRef.Child("situacao_atual").Child("ativo").SetValueAsync("nao");
        roomRef.Child("comando_facilitador").ChildAdded -= OnCommandReceived;
        roomRef.Child("comando_facilitador").RemoveValueAsync();

        Debug.Log("<color=#FF0000>[Firebase]</color> Sessão encerrada e dados limpos.");
    }

    public void StartRoom(UnityAction<StartRoomResult> action)
    {
        if (!_firebaseReady)
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    _dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                    _firebaseReady = true;
                    Debug.Log("<color=#00FF00>[Firebase]</color> Inicializado com sucesso.");

                    if (_pendingSessionStart)
                    {
                        StartFirebaseSession(_pendingStudents);
                    }
                }
                else
                {
                    Debug.LogError($"[Firebase] Erro de dependência: {task.Result}");
                }
            });
        }

        StartRoomResult result;
        result.wasSuccesful = false;
        result.settings = null;
        result.errorText = string.Empty;
        result.database = null;

        _dbReference.Child("classroom_configs")
                   .GetValueAsync().ContinueWithOnMainThread(task =>
                   {
                       if (task.IsFaulted)
                       {
                           Debug.LogError("<color=red>[MENU]</color> Falha ao buscar no Firebase.");
                           result.errorText = "Erro de conexão com o servidor.";
                       }
                       else if (task.IsCompleted)
                       {
                           DataSnapshot snapshot = task.Result;
                           if (snapshot.Exists && snapshot.ChildrenCount > 0)
                           {
                               DataSnapshot mostRecentRoom = null;
                               string greatestDateCreated = "";
                               int foundRooms = 0;

                               foreach (DataSnapshot sala in snapshot.Children)
                               {
                                   if (sala.HasChild("activeHeadsetId") && sala.Child("activeHeadsetId").Value.ToString() == SystemInfo.deviceUniqueIdentifier)
                                   {
                                       foundRooms++;

                                       if (sala.HasChild("criadoEm"))
                                       {
                                           string dateCreated = sala.Child("criadoEm").Value.ToString();
                                           if (string.Compare(dateCreated, greatestDateCreated) >= 0)
                                           {
                                               greatestDateCreated = dateCreated;
                                               mostRecentRoom = sala;
                                           }
                                       }
                                       else
                                       {
                                           if (mostRecentRoom == null) mostRecentRoom = sala;
                                       }
                                   }
                               }

                               if (mostRecentRoom != null)
                               {
                                   Debug.Log($"<color=#00FF00>[MENU]</color> Sucesso! {foundRooms} sala(s) encontrada(s) para este óculos. Injetando dados da sala: {mostRecentRoom.Child("roomName").Value}");

                                   ClassroomID = mostRecentRoom.Key; // Atualiza a variável na memória imediatamente!
                                   SaveSessionID(ClassroomID);

                                   ClassSettings classSettings = new ClassSettings();

                                   classSettings.NumBoys = ParseInt("numBoys", mostRecentRoom);
                                   classSettings.NumGirls = ParseInt("numGirls", mostRecentRoom);
                                   classSettings.NumDesks = ParseInt("numDesks", mostRecentRoom);
                                   classSettings.ClassShape = (ClassSettings.Shape)ParseInt("shape", mostRecentRoom);
                                   classSettings.Rows = ParseInt("rows", mostRecentRoom);
                                   classSettings.Cols = ParseInt("cols", mostRecentRoom);
                                   classSettings.Radius = ParseInt("radius", mostRecentRoom);
                                   classSettings.MaxDesksInSemiCircle = ParseInt("maxDesksInSemiCircle", mostRecentRoom);

                                   if (mostRecentRoom.HasChild("numTEA"))
                                       classSettings.NumTEA = ParseInt("numTEA", mostRecentRoom);

                                   if (mostRecentRoom.HasChild("numADHD"))
                                       classSettings.NumADHD = ParseInt("numADHD", mostRecentRoom);

                                   DatabaseReference roomRef = _dbReference.Child("classroom_configs").Child(ClassroomID);

                                   roomRef.Child("comando_facilitador").ChildAdded += OnCommandReceived;

                                   result.settings = classSettings;
                                   result.wasSuccesful = true;
                                   result.errorText = string.Empty;
                                   result.database = roomRef;
                               }
                               else
                                   result.errorText = "Nenhuma aula agendada para este dispositivo no momento.";
                           }
                           else
                               result.errorText = "Banco de dados vazio.";

                           action.Invoke(result);
                       }
                   });
    }

    private int ParseInt(string name, DataSnapshot parent)
    {
        return int.Parse(parent.Child(name).Value.ToString());
    }

    private void SaveSessionID(string key)
    {
        PlayerPrefs.SetString("session_configId", key);
        PlayerPrefs.Save();
    }

    private void LoadSessionID()
    {
        ClassroomID = PlayerPrefs.GetString("session_configId", "");
    }

    private void OnCommandReceived(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null) return;

        DataSnapshot snapshot = args.Snapshot;

        if (snapshot.HasChild("tipo_conflito"))
        {
            string tipoConflitoWeb = snapshot.Child("tipo_conflito").Value.ToString();

            string alunoAlvo = "";
            if (snapshot.HasChild("aluno_alvo")) alunoAlvo = snapshot.Child("aluno_alvo").Value.ToString();

            string message = "";
            if (snapshot.HasChild("mensagem")) message = snapshot.Child("mensagem").Value.ToString();

            _mainThreadActions.Enqueue(() => ProcessFirebaseCommand(alunoAlvo, tipoConflitoWeb, message));
        }
    }

    private void ProcessFirebaseCommand(string alunoAlvo, string tipoConflitoWeb, string message = "")
    {
        Debug.Log($"<color=#FFA500>[Firebase Comando]</color> Alvo: {alunoAlvo} | Ação recebida: {tipoConflitoWeb}");

        if (tipoConflitoWeb == "TakeMaterialAll")
        {
            StudentManager.Instance.GetMaterialOutAllStudents();
            return;
        }
        if (tipoConflitoWeb == "ResetAllStudents")
        {
            StudentManager.Instance.ResetAllStudents();
            return;
        }
        if (tipoConflitoWeb == "TextToSpeech")
        {
            Student st = StudentManager.Instance.GetStudent(alunoAlvo);
            st.Speak(message);
            return;
        }
        if (tipoConflitoWeb == "PlaySound")
        {

        }

        Student alvo = StudentManager.Instance.GetStudent(alunoAlvo);

        if (alvo == null)
        {
            Debug.LogWarning($"<color=red>[Firebase]</color> Aluno alvo '{alunoAlvo}' não encontrado. O comando {tipoConflitoWeb} foi ignorado.");
            return;
        }

        if (tipoConflitoWeb == "Make Students Laugh") { StudentManager.Instance.MakeNearbyStudentsLaugh(alvo); return; }
        if (tipoConflitoWeb == "Make Students Talk") { StudentManager.Instance.MakeNearbyStudentsTalk(alvo); return; }

        ConflictType type = ConflictType.UNKNOWN;

        switch (tipoConflitoWeb)
        {
            case "HyperstimulationConflict": type = ConflictType.Hyperstimulation; break;
            case "GetDistractedTEAConflict": type = ConflictType.DistractionTEA; break;
            case "BotherSomeoneConflict": type = ConflictType.BotherStudents; break;
            case "GetMaterialWrongConflict": type = ConflictType.MaterialOutWrong; break;
            case "DrawDistractedConflict": type = ConflictType.DrawDistracted; break;
            case "StandUpConflict": type = ConflictType.StandUp; break;

            case "SitTogether":
                type = ConflictType.SitTogether;
                break;

            case "StandUp":
                break;
            case "LeaveDesk":
                break;
            case "GoToFloor":
                break;
            case "MoveToRandomPoint":
                break;
            case "RunToRandomPoint":
                break;
            case "AnxiousRunToRandomPoint":
                break;
            case "MoveToFrontDoor":
                break;
            case "ChangeSits":
                break;

            case "TakeMaterialAll":
                break;
            case "TakeMaterialOut":
                break;
            case "Make Students Laugh":
                break;
            case "Make Students Talk":
                break;
            case "ResetAllStudents":
                StudentManager.Instance.ResetAllStudents();
                return;

            case "ResetSingleStudent":
                StudentManager.Instance.ResetSingleStudent(alunoAlvo);
                return;

            default:
                Debug.LogWarning($"[Firebase] O comando '{tipoConflitoWeb}' foi ignorado por ser uma ação avulsa ou não mapeada.");
                return;
        }

        StudentManager.Instance.GenerateConflictForSpecificStudent(alunoAlvo, type);
    }

    private void OnDestroy()
    {
        EndFirebaseSession();
    }
}