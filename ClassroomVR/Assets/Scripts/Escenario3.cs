using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Escenario3 : MonoBehaviour, Escenario
{
    public GameObject tipsPanel;
    public GameObject feedbackPanel;
    private Text tmpPath1;
    private Text tmpPath2;
    private Text tmpPath3;
    public Text feedbackMsg;
    public Text correctPath;
    public GameObject path1;
    public GameObject path2;
    public GameObject path3;
    public GameObject feedbackObject;

    private float timeSinceSceneStarted = 0;
    private float timeToShowFeedback = 0;

    private bool pathChosen = false;
    private bool playedAngryAudio = false;
    private bool playedClassLaugh = false;
    private bool scenarioFinished = false;
    private bool panelTipsActivado = false;

    public List<GameObject> students = new List<GameObject>();

    private Animator studentAnimator;
    private Animator classmate1;
    private Animator classmate2;
    private Animator classmate3;

    public UnityEvent moveToBack;
    public UnityEvent returnToChair;
    public UnityEvent decidedPathEvent;
    public UnityEvent moveToCorridor;

    private float angryAudioLength;

    private bool playExplanationAudioBool = false;

    private double audioLength;

    private double TIME_TO_START_REACTION;

    // Start is called before the first frame update
    void Start()
    {
        // Añadir todos los objetos con Tag "Student"
        foreach (GameObject student in GameObject.FindGameObjectsWithTag(Constants.STUDENT_TAG))
        {
            students.Add(student);
        }
        Time.timeScale = 0;

        tmpPath1 = path1.GetComponentInChildren<Text>();
        tmpPath2 = path2.GetComponentInChildren<Text>();
        tmpPath3 = path3.GetComponentInChildren<Text>();

        studentAnimator = students.Find(s => s.name.Equals(Constants.STUDENT_THIRD_SCENARIO_NAME)).GetComponent<Animator>();
        classmate1 = students.Find(s => s.name.Equals(Constants.CLASSMATE1_THIRD_SCENARIO)).GetComponent<Animator>();
        classmate2 = students.Find(s => s.name.Equals(Constants.CLASSMATE2_THIRD_SCENARIO)).GetComponent<Animator>();
        classmate3 = students.Find(s => s.name.Equals(Constants.CLASSMATE3_THIRD_SCENARIO)).GetComponent<Animator>();

        foreach (AudioSource aSource in this.GetComponents<AudioSource>())
        {
            if (aSource.clip.name.Equals(Constants.CLASS_LAUGH_AUDIO))
            {
                audioLength = aSource.clip.length;
            }
        }


        TIME_TO_START_REACTION = Constants.TIME_FOR_CLASS_LAUGH + audioLength - 2;
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceSceneStarted += Time.deltaTime;

        // Se reproduce el audio inicial del alumno
        if(timeSinceSceneStarted > Constants.TIME_FOR_ANGRY_AUDIO && !playedAngryAudio)
        {
            playedAngryAudio = true;

            moveToCorridor.Invoke();
            PlayAngryAudio();
        }


        if (timeSinceSceneStarted > Constants.TIME_FOR_CLASS_LAUGH && !playedClassLaugh) {
            playedClassLaugh = true;
            PlayGroupLaugh();
        }

        if (timeSinceSceneStarted > TIME_TO_START_REACTION)
        {
            if (!panelTipsActivado)
            {
                panelTipsActivado = true;
                tipsPanel.SetActive(true);
            }
        }

        if (classmate1.GetCurrentAnimatorStateInfo(0).IsName(Constants.CHEERING_ANIM))
        {
            classmate1.SetBool(Constants.CHEERING_PARAMETER, false);
            classmate2.SetBool(Constants.CHEERING_PARAMETER, false);
            classmate3.SetBool(Constants.CHEERING_PARAMETER, false);
        }

        if(timeSinceSceneStarted > timeToShowFeedback && pathChosen && !scenarioFinished)
        {
            scenarioFinished = true;
            tipsPanel.SetActive(false);
            feedbackPanel.SetActive(true);
            Time.timeScale = 0;
            this.GetComponentInChildren<MouseLook>().enabled = false;
            Cursor.lockState = CursorLockMode.None;
            decidedPathEvent.Invoke();
        }

        if (playExplanationAudioBool && timeSinceSceneStarted > Constants.TIME_FOR_ANGRY_AUDIO + angryAudioLength + 1.5f)
        {
            PlayExplanationAudio();
            playExplanationAudioBool = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("El profesor se ha acercado a " + other.name);
        if (other.name == Constants.STUDENT_THIRD_SCENARIO_NAME)
        {
            ThirdPath();
        }
    }

    public void ResumeGame()
    {
        this.GetComponentInChildren<MouseLook>().enabled = true;
        //this.GetComponent<KeyWordRecognizer>().enabled = true;
        Time.timeScale = 1;

        PlayAnimationsAtDifferentTimeSitting();
    }

    // PRIMER CAMINO
    public void FirstPath()
    {
        if (!pathChosen)
        {
            tmpPath1.color = Color.red;
            tmpPath2.color = Color.grey;
            tmpPath3.color = Color.grey;

            Debug.Log("Se ha tomado el primer camino: el profesor manda al alumno sentarse de nuevo");
            pathChosen = true;
            timeToShowFeedback = timeSinceSceneStarted + 5;
            feedbackMsg.text = Constants.FEEDBACK_E3[0];
            correctPath.text = Constants.BAD_PATH;
            correctPath.color = Color.red;

            PlayGroupLaugh();
            returnToChair.Invoke();
        }
    }

    // SEGUNDO CAMINO
    public void SecondPath()
    {
        if (!pathChosen)
        {
            tmpPath1.color = Color.grey;
            tmpPath2.color = Color.red;
            tmpPath3.color = Color.grey;

            Debug.Log("Se ha tomado el segundo camino: el profesor manda al alumno al final de la clase");
            pathChosen = true;
            timeToShowFeedback = timeSinceSceneStarted + 10;
            feedbackMsg.text = Constants.FEEDBACK_E3[1];
            correctPath.text = Constants.BAD_PATH;
            correctPath.color = Color.red;

            PlayGroupLaugh();
            moveToBack.Invoke();
        }
    }

    // TERCER CAMINO
    public void ThirdPath()
    {
        if (!pathChosen)
        {
            tmpPath1.color = Color.grey;
            tmpPath2.color = Color.grey;
            tmpPath3.color = new Color(0f / 255f, 160f / 255f, 0f / 255f);

            Debug.Log("Se ha tomado el tercer camino: el profesor se acerca y el alumno explica sus motivos");
            pathChosen = true;
            timeToShowFeedback = timeSinceSceneStarted + angryAudioLength + 7;
            feedbackMsg.text = Constants.FEEDBACK_E3[2];
            correctPath.text = Constants.GOOD_PATH;
            correctPath.color = new Color(0f / 255f, 160f / 255f, 0f / 255f);

            // We have to do this in update in order to not overlap the initial angry audio with the explanation audio
            playExplanationAudioBool = true;
        }
    }

    private void PlayGroupLaugh()
    {
        // Audios riendo
        foreach (AudioSource aSource in this.GetComponents<AudioSource>())
        {
            if (aSource.clip.name.Equals(Constants.CLASS_LAUGH_AUDIO))
            {
                aSource.Play();
            }
        }
        classmate1.SetBool(Constants.CHEERING_PARAMETER, true);
        classmate2.SetBool(Constants.CHEERING_PARAMETER, true);
        classmate3.SetBool(Constants.CHEERING_PARAMETER, true);
    }

    private void PlayAngryAudio()
    {
        foreach (AudioSource aSource in this.GetComponents<AudioSource>())
        {
            if (aSource.clip.name.Equals(Constants.ANGRY_AUDIO))
            {
                aSource.Play();
                angryAudioLength = aSource.clip.length;
            }
        }
    }

    private void PlayExplanationAudio()
    {
        foreach (AudioSource aSource in this.GetComponents<AudioSource>())
        {
            if (aSource.clip.name.Equals(Constants.EXPLANATION_AUDIO))
            {
                aSource.Play();
            }
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Restart()
    {
        SceneManager.LoadScene("Escenario3");
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
