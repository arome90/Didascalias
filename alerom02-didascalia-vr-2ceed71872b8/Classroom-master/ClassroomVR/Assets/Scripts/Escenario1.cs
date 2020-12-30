using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Escenario1 : MonoBehaviour, Escenario
{
    public Text feedbackMsg;
    public Text correctPath;
    public GameObject feedbackPanel;
    public GameObject feedbackObject;

    private bool decidedPath = false;

    private float timeToShowFeedback = 0;

    private double audioLength;

    public GameObject tipsPanel;
    private Text tmpPath1;
    private Text tmpPath2;
    private Text tmpPath3;
    public GameObject path1;
    public GameObject path2;
    public GameObject path3;

    private bool startedPlaying = false;
    private bool finishedPlaying = false;
    private bool finishedCountdown = false;
    private bool finishedLaughs = false;
    private bool scenarioFinished = false;

    Animator anim;

    public UnityEvent commentFinishedEvent;
    public UnityEvent reactionTimeFinishedEvent;
    public UnityEvent decidedPathEvent;

    TextMeshPro tmp;

    public List<GameObject> students = new List<GameObject>();

    private float timeSinceSceneStarted;

    void Start()
    {
        timeSinceSceneStarted = 0;

        // Añadir todos los objetos con Tag "Student"
        foreach (GameObject student in GameObject.FindGameObjectsWithTag(Constants.STUDENT_TAG))
        {
            students.Add(student);
        }

        tmpPath1 = path1.GetComponentInChildren<Text>();
        tmpPath2 = path2.GetComponentInChildren<Text>();
        tmpPath3 = path3.GetComponentInChildren<Text>();

        Time.timeScale = 0;

        tmp = students.Find(s => s.name.Equals(Constants.INAPPROPRIATE_STUDENT_NAME)).GetComponentInChildren<TextMeshPro>();

        audioLength = students.Find(s => s.name.Equals(Constants.INAPPROPRIATE_STUDENT_NAME)).GetComponent<AudioSource>().clip.length;
    }

    public void ResumeGame()
    {
        this.GetComponentInChildren<MouseLook>().enabled = true;
        Time.timeScale = 1;

        PlayAnimationsAtDifferentTimeSitting();
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceSceneStarted += Time.deltaTime;
        // Temporizador para reproducir el audio inapropiado
        if (timeSinceSceneStarted > Constants.TIME_FOR_AUDIO && !startedPlaying)
        {
            startedPlaying = true;

            students.Find(s => s.name.Equals(Constants.INAPPROPRIATE_STUDENT_NAME)).GetComponent<AudioSource>().Play();

            // Nombre del alumno en verde para indicar que está hablando
            tmp.color = Color.green;

            // Animación de grito para el comentario inadecuado
            Animator anim = students.Find(s => s.name.Equals(Constants.INAPPROPRIATE_STUDENT_NAME)).GetComponent<Animator>();
            anim.SetBool(Constants.SHOUTING_PARAMETER, true);

            Debug.Log("Se reproduce el comentario inapropiado");
        }

        // Temporizador para reproducir las risas de los alumnos como respuesta al comentario inadecuado
        if (timeSinceSceneStarted > Constants.TIME_FOR_AUDIO + audioLength - 1 && !finishedLaughs)
        {
            finishedLaughs = true;

            PlayClassReaction();

            Debug.Log("Se reproducen risas de la clase como reacción al comentario inapropiado");
        }

        // Cuando ya se ha terminado de reproducir el audio
        if(timeSinceSceneStarted > Constants.TIME_FOR_AUDIO + audioLength && !finishedPlaying)
        {
            finishedPlaying = true;

            // Nombre del alumno en negro para indicar que ha terminado de hablar
            tmp.color = Color.black;

            // Vuelta a la animación de sentado
            Animator anim = students.Find(s => s.name.Equals(Constants.INAPPROPRIATE_STUDENT_NAME)).GetComponent<Animator>();
            anim.SetBool(Constants.SHOUTING_PARAMETER, false);

            commentFinishedEvent.Invoke();

            tipsPanel.SetActive(true);

            Debug.Log("El audio termina y el micrófono graba la intensidad del sonido");
        }

        // Cuando ha finalizado el tiempo de reacción después de haberse reproducido el audio
        if (timeSinceSceneStarted > Constants.TIME_FOR_AUDIO + audioLength + Constants.TIME_FOR_REACTION_E1 && !finishedCountdown)
        {
            finishedCountdown = true;

            reactionTimeFinishedEvent.Invoke();

            if (!decidedPath)
            {
                this.ThirdPath();
            }
        }

        if (timeSinceSceneStarted > timeToShowFeedback && decidedPath && !scenarioFinished)
        {
            scenarioFinished = true;
            tipsPanel.SetActive(false);
            feedbackPanel.SetActive(true);
            Time.timeScale = 0;
            this.GetComponentInChildren<MouseLook>().enabled = false;
            Cursor.lockState = CursorLockMode.None;
            decidedPathEvent.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == Constants.INAPPROPRIATE_STUDENT_NAME)
        {
            Debug.Log("Me he acercado al alumno");
            if (timeSinceSceneStarted > Constants.TIME_FOR_AUDIO + audioLength && timeSinceSceneStarted < Constants.TIME_FOR_AUDIO + audioLength + Constants.TIME_FOR_REACTION_E1)
            {
                this.SecondPath(); 
            }
        }
    }

    // PRIMER CAMINO - EL PROFESOR HA RESPONDIDO FIRMEMENTE
    // Cuando el profesor se da la vuelta, se reproducen sonidos de burla y susurros
    public void FirstPath()
    {
        if (!decidedPath)
        {
            decidedPath = true;

            timeToShowFeedback = timeSinceSceneStarted + 5;
            feedbackMsg.text = Constants.FEEDBACK_E1[0];
            correctPath.text = Constants.BAD_PATH;
            correctPath.color = Color.red;

            tmpPath1.color = Color.red;
            tmpPath2.color = Color.grey;
            tmpPath3.color = Color.grey;
            
            Debug.Log("Se ha tomado el primer camino");

            PlayAnimations();

            PlayClassNoise();
        }
    }

    // SEGUNDO CAMINO - NOS ACERCAMOS
    // La clase continúa
    public void SecondPath()
    {
        if (!decidedPath)
        {
            decidedPath = true;

            timeToShowFeedback = timeSinceSceneStarted + 5;
            feedbackMsg.text = Constants.FEEDBACK_E1[1];
            correctPath.text = Constants.GOOD_PATH;
            correctPath.color = new Color(0f / 255f, 160f / 255f, 0f / 255f);

            tmpPath1.color = Color.grey;
            tmpPath2.color = new Color (0f/255f, 160f/255f, 0f/255f);
            tmpPath3.color = Color.grey;

            Debug.Log("Se ha tomado el segundo camino");
        }
    }

    // TERCER CAMINO - HA FINALIZADO EL TIEMPO, LO HA IGNORADO
    // Risas de fondo y los alumnos se distraen
    public void ThirdPath()
    {
        timeToShowFeedback = timeSinceSceneStarted + 5;
        feedbackMsg.text = Constants.FEEDBACK_E1[2];
        correctPath.text = Constants.BAD_PATH;
        correctPath.color = Color.red;

        decidedPath = true;

        tmpPath1.color = Color.grey;
        tmpPath2.color = Color.grey;
        tmpPath3.color = Color.red;

        Debug.Log("Se ha tomado el tercer camino");

        PlayAnimations();

        PlayClassNoise();
    }

    private void PlayClassNoise()
    {
        // Audios molestando
        GameObject player = GameObject.FindGameObjectWithTag(Constants.PLAYER_TAG);
        foreach (AudioSource aSource in player.GetComponents<AudioSource>())
        {
            if (aSource.clip.name.Equals(Constants.CLASS_NOISE_AUDIO))
            {
                aSource.Play();
            }
        }
    }


    private void PlayClassReaction()
    {
        // Reproducir murmullos o risas (reacción de la clase al comentario inapropiado)
        GameObject player = GameObject.FindGameObjectWithTag(Constants.PLAYER_TAG);
        foreach (AudioSource aSource in player.GetComponents<AudioSource>())
        {
            if (aSource.clip.name.Equals(Constants.CLASS_LAUGH_AUDIO))
            {
                aSource.Play();
            }
        }
    }
    

    private void PlayAnimations()
    {
        // Animaciones de distraídos
        foreach (GameObject student in students)
        {
            anim = student.GetComponent<Animator>();
            anim.SetBool(Constants.DISTRACTED_PARAMETER, true);
            Debug.Log("Se ha animado un student");
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Restart()
    {
        SceneManager.LoadScene("Escenario1");
    }

    public void Finish()
    {
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
    
    private void PlayAnimationsAtDifferentTimeSitting()
    {
        // Play animations at different time
        float time = 0.0f;
        foreach (GameObject student in students)
        {
            time = time + 1f / 8;
            Animator anim = student.GetComponent<Animator>();
            JumpToTime(anim, Constants.SITTING_STATE, time);
        }
    }

    private void JumpToTime(Animator animator, string name, float nTime)
    {
        animator.Play(name, 0, nTime);
    }
}
