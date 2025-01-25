using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    public static LightingManager Instance { get; private set; }

    //Scene References
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;

    //Variables
    [SerializeField, Range(0, 24)] private float TimeOfDay;
    [SerializeField] private float transitionSpeed;

    private float targetTime;
    //usado para fmod
    [SerializeField, Range(0f, 1f)]
    private float dayNightProgress = 0;

    [SerializeField]
    private Material material;
    [SerializeField]
    float skyBoxTimeScale = 0.2f;
    [SerializeField]
    private float transitionDuration = 5f;
    private float elapsedTime = 5f;
    private float startTime;
    enum DayTime { Day, Night }

    [SerializeField]
    private List<GameObject> nightGameObjects, dayGameObjects;

    private DayTime time;
    private bool inTransition;
    private int skyBoxTransition;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

    }
    //private void Start()
    //{
    //    inTransition = false;
    //    //UpdateLighting(14f / 24f);
    //    material.SetFloat("_TimeScale", 1);
    //    setToDay();
    //}

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Q) && !inTransition)
    //    {
    //        inTransition = true;
    //        if (time == DayTime.Day) setToNight();
    //        else setToDay();
    //    }

    //    if (inTransition)
    //    {
    //        elapsedTime += Time.deltaTime;
    //        float t = elapsedTime / transitionDuration;

    //        // Interpolate the value over the 5-second duration
    //        material.SetFloat("_TimeScale", Mathf.Lerp(-skyBoxTimeScale * skyBoxTransition, skyBoxTimeScale * skyBoxTransition, t));
    //        TimeOfDay = (Mathf.Lerp(startTime, targetTime, t)) % 24;

    //        // Update `dayNightProgress`
    //        if (time == DayTime.Night)
    //            dayNightProgress = Mathf.Lerp(0f, 1f, t);
    //        else
    //            dayNightProgress = Mathf.Lerp(1f, 0f, t);W


    //        UpdateLighting(TimeOfDay / 24f);
    //        if (elapsedTime > transitionDuration)
    //        {
    //            inTransition = false;
    //        }
    //    }

    //}
    private void Update()
    {
        if (Preset == null)
            return;

        if (Application.isPlaying)
        {
            //(Replace with a reference to the game time)
            TimeOfDay += Time.deltaTime*transitionSpeed;
            TimeOfDay %= 24; //Modulus to ensure always between 0-24
            UpdateLighting(TimeOfDay / 24f); 
        }
        else
        {
            UpdateLighting(TimeOfDay / 24f);
        }
    }


    public float getDayNightProgress() { return dayNightProgress; }
    public void setToDay()
    {
        elapsedTime = 0;

        time = DayTime.Day;
        targetTime = 14f;
        startTime = TimeOfDay;
        skyBoxTransition = 1;
        dayNightProgress = 0f; // Día inicial

        for (int i = 0; i < nightGameObjects.Count; ++i)
        {
            nightGameObjects[i].SetActive(false);
        }
        for (int i = 0; i < dayGameObjects.Count; ++i)
        {
            dayGameObjects[i].SetActive(true);
        }
    }
    public void setToNight()
    {
        elapsedTime = 0;

        time = DayTime.Night;
        targetTime = 26f;
        startTime = TimeOfDay;
        skyBoxTransition = -1;

        dayNightProgress = 1f; // Noche inicial

        for (int i = 0; i < nightGameObjects.Count; ++i)
        {
            nightGameObjects[i].SetActive(true);
        }
        for (int i = 0; i < dayGameObjects.Count; ++i)
        {
            dayGameObjects[i].SetActive(false);
        }
    }

    private void UpdateLighting(float timePercent)
    {
        //Set ambient and fog
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);

        //If the directional light is set then rotate and set it's color, I actually rarely use the rotation because it casts tall shadows unless you clamp the value
        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);

            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 258.2f, 0));
        }

    }

    //Try to find a directional light to use if we haven't set one
    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;

        //Search for lighting tab sun
        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        //Search scene for light that fits criteria (directional)
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
            }
        }
    }
}