using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseClassroomManager : Singleton<FirebaseClassroomManager>
{
    [Header("Configurações do Firebase")]
    [Tooltip("A ID da sala atual no banco de dados")]
    [HideInInspector]
    public string classroomId = "";

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

        // Puxa a ID da sala salva pelo Botao_entrar.cs
        classroomId = PlayerPrefs.GetString("session_configId", "");

        if (string.IsNullOrEmpty(classroomId))
        {
            Debug.LogError("<color=red>[Firebase]</color> O jogo entrou na cena da sala, mas perdeu a referência do ID. Nenhuma sala foi carregada!");
            return; // Impede que o Firebase tente conectar a uma sala vazia
        }

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

    private void Update()
    {
        while (_mainThreadActions.Count > 0)
        {
            _mainThreadActions.Dequeue()?.Invoke();
        }
    }

    public void StartFirebaseSession(List<Student> students)
    {
        // Se o Firebase ainda não conectou, guardamos a lista para enviar na fração de segundo seguinte
        if (!_firebaseReady || _dbReference == null)
        {
            _pendingSessionStart = true;
            _pendingStudents = students;
            Debug.Log("<color=#FFFF00>[Firebase]</color> Aguardando conexão para enviar os dados da sala...");
            return;
        }

        _pendingSessionStart = false;
        DatabaseReference roomRef = _dbReference.Child("classroom_configs").Child(classroomId);

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

            // Create a nested object for the student's details
            Dictionary<string, string> studentData = new Dictionary<string, string>();
            studentData["nome"] = student.Name;
            studentData["condicao"] = condition;

            // Keeps the "Aluno1", "Aluno2" structure as the main key
            alunosDict["Aluno" + (i + 1)] = studentData;
        }
        roomRef.Child("Alunos").SetValueAsync(alunosDict);

        roomRef.Child("comando_facilitador").ChildAdded += OnCommandReceived;

        Debug.Log("<color=#00FFFF>[Firebase]</color> Sessão iniciada, Alunos enviados e escutando comandos.");
    }

    public void EndFirebaseSession()
    {
        if (!_firebaseReady || _dbReference == null) return;

        DatabaseReference roomRef = _dbReference.Child("classroom_configs").Child(classroomId);

        roomRef.Child("situacao_atual").Child("ativo").SetValueAsync("nao");

        // Comentado pois a plataforma web já faz a limpeza da lista de alunos
        // roomRef.Child("Alunos").RemoveValueAsync();

        // Para de escutar novos comandos
        roomRef.Child("comando_facilitador").ChildAdded -= OnCommandReceived;

        // Apaga o nó de comandos do Firebase
        roomRef.Child("comando_facilitador").RemoveValueAsync();

        Debug.Log("<color=#FF0000>[Firebase]</color> Sessão encerrada e dados limpos.");
    }

    private void OnCommandReceived(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null) return;

        DataSnapshot snapshot = args.Snapshot;

        // Agora só exigimos obrigatoriamente que exista um tipo_conflito
        if (snapshot.HasChild("tipo_conflito"))
        {
            string tipoConflitoWeb = snapshot.Child("tipo_conflito").Value.ToString();

            // O aluno alvo passa a ser opcional
            string alunoAlvo = "";
            if (snapshot.HasChild("aluno_alvo"))
            {
                alunoAlvo = snapshot.Child("aluno_alvo").Value.ToString();
            }

            _mainThreadActions.Enqueue(() => ProcessFirebaseCommand(alunoAlvo, tipoConflitoWeb));
        }
    }

    private void ProcessFirebaseCommand(string alunoAlvo, string tipoConflitoWeb)
    {
        Debug.Log($"<color=#FFA500>[Firebase Comando]</color> Alvo: {alunoAlvo} | Ação recebida: {tipoConflitoWeb}");

        // Ação Global
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

        //Procura do Aluno 
        Student alvo = StudentManager.Instance.GetStudent(alunoAlvo);

        if (alvo == null)
        {
            Debug.LogWarning($"<color=red>[Firebase]</color> Aluno alvo '{alunoAlvo}' não encontrado. O comando {tipoConflitoWeb} foi ignorado.");
            return;
        }

        // Ações de Ambiente
        if (tipoConflitoWeb == "Make Students Laugh") { StudentManager.Instance.MakeNearbyStudentsLaugh(alvo); return; }
        if (tipoConflitoWeb == "Make Students Talk") { StudentManager.Instance.MakeNearbyStudentsTalk(alvo); return; }

        // Mapeamento Estrito de Conflitos Pedagógicos
        ConflictType type = ConflictType.UNKNOWN;

        switch (tipoConflitoWeb)
        {
            // CONFLITOS (Requerem resolução do jogador)
            case "HyperstimulationConflict": type = ConflictType.Hyperstimulation; break;
            case "GetDistractedTEAConflict": type = ConflictType.DistractionTEA; break;
            case "BotherSomeoneConflict": type = ConflictType.BotherStudents; break;
            case "GetMaterialWrongConflict": type = ConflictType.MaterialOutWrong; break;
            case "DrawDistractedConflict": type = ConflictType.DrawDistracted; break;
            case "StandUpConflict": type = ConflictType.StandUp; break;

            //(Não são conflitos diretos) 
            case "SitTogether":
                type = ConflictType.SitTogether;
                break;

            // MOVIMENTOS E AÇÕES DIRETAS
            case "StandUp":
                // Chamar função do aluno para levantar
                break;
            case "LeaveDesk":
                // Chamar função do aluno para sair da carteira
                break;
            case "GoToFloor":
                // Chamar função do aluno para sentar no chão
                break;
            case "MoveToRandomPoint":
                // Chamar função de movimento
                break;
            case "RunToRandomPoint":
                // Chamar função de corrida
                break;
            case "AnxiousRunToRandomPoint":
                // Chamar função de corrida ansiosa
                break;
            case "MoveToFrontDoor":
                // Chamar função de ir para a porta
                break;
            case "ChangeSits":
                break;

            //COMANDOS GERAIS DA TURMA E MATERIAIS
            case "TakeMaterialAll":
                // Chamar função global para todos pegarem o material
                break;
            case "TakeMaterialOut":
                // Chamar função para um aluno específico pegar material
                break;
            case "Make Students Laugh": 
                break;
            case "Make Students Talk": 
                break;
            // Botão: Reset Global 
            case "ResetAllStudents":
                StudentManager.Instance.ResetAllStudents();
                return; 

            // Botão: Reset Individual
            case "ResetSingleStudent":
                // Utiliza a variável do Firebase que guarda o nome do aluno alvo 
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