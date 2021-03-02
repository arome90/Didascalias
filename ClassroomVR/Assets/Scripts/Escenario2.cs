using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Escenario2 : MonoBehaviour, Escenario
{
    private Text tmpPath1;
    private Text tmpPath2;
    private Text tmpPath3;
    private Text tmpPath4;
    public Text feedbackMsg;
    public Text correctPath;
    public GameObject path1;
    public GameObject path2;
    public GameObject path3;
    public GameObject path4;
    public GameObject feedbackPanel;
    public GameObject tipsPanel;
    public GameObject feedbackObject;

    private float timeToShowFeedback = 0;

    private float timeSinceSceneStarted = 0;

    private float timerForNoisyClass = 5.0f;

    private bool noisyClassAfterSittingStudent = false;
    
    private bool strongResponse = false;
    private bool calculatedStrongResponse = false;
    private bool decidedPath = false;
    private bool scenarioFinished = false;
    
    private bool activarDaniCheering = false;

    public List<GameObject> students = new List<GameObject>();

    private Animator student1Animator;
    private Animator student2Animator; 

    public UnityEvent reactionTimeFinishedEvent;
    public UnityEvent separateStudent;
    public UnityEvent decidedPathEvent;

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
        tmpPath4 = path4.GetComponentInChildren<Text>();

        student1Animator = students.Find(s => s.name.Equals(Constants.FIRST_STUDENT_SECOND_SCENARIO_NAME)).GetComponent<Animator>();
        student2Animator = students.Find(s => s.name.Equals(Constants.SECOND_STUDENT_SECOND_SCENARIO_NAME)).GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceSceneStarted += Time.deltaTime;

        if(timeSinceSceneStarted > Constants.TIME_FOR_SENTENCE && !calculatedStrongResponse)
        {
            calculatedStrongResponse = true;
            reactionTimeFinishedEvent.Invoke();
        }

        if (timeSinceSceneStarted > Constants.TIME_FOR_SENTENCE && !decidedPath)
        {
            decidedPath = true;
            SecondPath();
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

        if (student1Animator.GetCurrentAnimatorStateInfo(0).IsName(Constants.CHEERING_ANIM))
        {
            student1Animator.SetBool(Constants.CHEERING_PARAMETER, false);
            student2Animator.SetBool(Constants.CHEERING_PARAMETER, false);
        }

        if (noisyClassAfterSittingStudent)
        {
            timerForNoisyClass -= Time.deltaTime;
            if (timerForNoisyClass <= 0.0f)
            {
                PlayClassNoise();
                PlayConflictiveStudentsAnimations();
                PlayRestStudentsAnimations();
                noisyClassAfterSittingStudent = false;
            }
        }

        if (activarDaniCheering)
        {
            if (student2Animator.GetCurrentAnimatorStateInfo(0).IsName(Constants.SITTING_STATE))
            {
                activarDaniCheering = false;
                student2Animator.SetBool(Constants.CHEERING_PARAMETER, true);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("El profesor se ha acercado a " + other.name);
        if (other.name == Constants.FIRST_STUDENT_SECOND_SCENARIO_NAME || other.name == Constants.SECOND_STUDENT_SECOND_SCENARIO_NAME)
        {
            if(!decidedPath)
            {
                decidedPath = true;
                ThirdPath();
            }
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
        Debug.Log("Se ha tomado el primer camino: el profesor habla de forma asertiva y no hacen caso");

        timeToShowFeedback = timeSinceSceneStarted + 5;
        feedbackMsg.text = Constants.FEEDBACK_E2[0];
        correctPath.text = Constants.BAD_PATH;
        correctPath.color = Color.red;

        tmpPath1.color = Color.red;
        tmpPath2.color = Color.grey;
        tmpPath3.color = Color.grey;
        tmpPath4.color = Color.grey;

        PlayClassNoise();
        PlayConflictiveStudentsAnimations();
        PlayRestStudentsAnimations();
    }

    // SEGUNDO CAMINO
    public void SecondPath()
    {
        Debug.Log("Se ha tomado el segundo camino: el profesor lo ignora y los alumnos siguen molestando");

        tmpPath1.color = Color.grey;
        tmpPath2.color = Color.red;
        tmpPath3.color = Color.grey;
        tmpPath4.color = Color.grey;

        timeToShowFeedback = timeSinceSceneStarted + 5;
        feedbackMsg.text = Constants.FEEDBACK_E2[1];
        correctPath.text = Constants.BAD_PATH;
        correctPath.color = Color.red;

        PlayClassNoise();
        PlayConflictiveStudentsAnimations();
        PlayRestStudentsAnimations();
    }

    // TERCER CAMINO
    public void ThirdPath()
    {
        Debug.Log("Se ha tomado el tercer camino: el profesor se acerca y la clase se reanuda con normalidad");

        tmpPath1.color = Color.grey;
        tmpPath2.color = Color.grey;
        tmpPath3.color = new Color(0f / 255f, 160f / 255f, 0f / 255f);
        tmpPath4.color = Color.grey;

        timeToShowFeedback = timeSinceSceneStarted + 5;
        feedbackMsg.text = Constants.FEEDBACK_E2[2];
        correctPath.text = Constants.GOOD_PATH;
        correctPath.color = new Color(0f / 255f, 160f / 255f, 0f / 255f);
    }

    // CUARTO CAMINO
    public void FourthPath()
    {
        Debug.Log("Se ha tomado el cuarto camino: se separan");
        
        tmpPath1.color = Color.grey;
        tmpPath2.color = Color.grey;
        tmpPath3.color = Color.grey;
        tmpPath4.color = Color.red;

        timeToShowFeedback = timeSinceSceneStarted + 10;
        feedbackMsg.text = Constants.FEEDBACK_E2[3];
        correctPath.text = Constants.BAD_PATH;
        correctPath.color = Color.red;

        separateStudent.Invoke();
    }

    public void NoisyClassAfterSittingStudent()
    {
        noisyClassAfterSittingStudent = true;
        activarDaniCheering = true;
    }

    public void AssertiveRecognized()
    {
        if (!decidedPath)
        {
            decidedPath = true;
            FirstPath();
        }
    }

    public void AuthoritativeRecognized()
    {
        if (!decidedPath)
        {
            decidedPath = true;
            FourthPath();
        }
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

    private void PlayConflictiveStudentsAnimations()
    {
        student1Animator.SetBool(Constants.CHEERING_PARAMETER, true);
        student2Animator.SetBool(Constants.CHEERING_PARAMETER, true);
    }

    private void PlayRestStudentsAnimations()
    {
        foreach (GameObject student in students)
        {
            if (student.name != Constants.FIRST_STUDENT_SECOND_SCENARIO_NAME && student.name != Constants.SECOND_STUDENT_SECOND_SCENARIO_NAME)
            {
                Animator anim = student.GetComponent<Animator>();
                anim.SetBool(Constants.DISTRACTED_PARAMETER, true);
                Debug.Log("Se ha animado un student");
            }
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Restart()
    {
        SceneManager.LoadScene("Escenario2");
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
