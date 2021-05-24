using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IronPython;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using System.IO;
using UnityEngine.Audio;
using System;
using UnityEngine.UI.Extensions;

namespace ClassRoomVR//NOSE SI ES ENECESARIO
{
    public class MicToVokaturi : MonoBehaviour
    {

        static ScriptEngine pyEngine = null;
        dynamic vokaWrapper;

        double actTime;
        public int ClipLength = 5;
        private bool collect = false;
        private int window = -1;

        List<float> results;

        List<float> happyList;
        List<float> sadList;
        List<float> fearList;
        List<float> angerList;
        List<float> neutralList;


        //Valores para serializacion
        public float mediaHappy, mediaSad, mediaFear, mediaAnger, mediaNeutral;
        public float finalHappy, finalSad, finalFear, finalAnger, finalNeutral;
        public RadarPolygon radarPolygon;

        // Start is called before the first frame update
        void Start()
        {
            pyEngine = Python.CreateEngine();
            setSearchPaths();


            dynamic py = pyEngine.ExecuteFile(Application.dataPath + "/Libs/Vokaturi_Python/Python/VokaWrapper.py");
            vokaWrapper = py.vokaNetWrapper(Application.dataPath + "/Libs/DLL/OpenVokaturi-3-0-win64.dll");

            window = ClipLength * MicrophoneManager.SAMPLERATE;
        }
        public void StartCollecting()
        {
            results = new List<float>();
            collect = true;
            actTime = Time.realtimeSinceStartup;

            happyList = new List<float>();
            sadList = new List<float>();
            fearList = new List<float>();
            angerList = new List<float>();
            neutralList = new List<float>();
        }

        public void StopCollecting()
        {
            collect = false;
        }

        public float[] GetData()
        {
            return results.ToArray();
        }

        // Update is called once per frame
        void Update()
        {
            if (!collect) return;
            if ((Time.realtimeSinceStartup - actTime) > ClipLength)
            {
                //Get the data from the microphone
                float[] data = new float[window];

                int position = MicrophoneManager.GetMicrophonePosition() - (window + 1);
                bool success = MicrophoneManager.AudioClip.GetData(data, position);

                float z = data[0];
                //Parse it to double so python can use it
                double[] doubleArray = Array.ConvertAll(data, x => (double)x);

                if (success)
                {
                    //  Debug.Log("Data copied");
                    dynamic result = vokaWrapper.vokalculate(doubleArray, MicrophoneManager.SAMPLERATE);

                    if (result["Success"])
                    {
                        //Debug.Log("Neutrality: "+ result["Neutral"]);
                        //Debug.Log("Happiness: " + result["Happy"]);
                        //Debug.Log("Sadness: " + result["Sad"]);
                        //Debug.Log("Anger: " + result["Angry"]);
                        //Debug.Log("Fear: " + result["Fear"]);
                        //Debug.Log("Error msg: "+ result["Error"]);

                        float neutral = (float)result["Neutral"];
                        float happy = (float)result["Happy"];
                        float sad = (float)result["Sad"];
                        float angry = (float)result["Angry"];
                        float fear = (float)result["Fear"];


                        results.Add(neutral);
                        results.Add(happy);
                        results.Add(sad);
                        results.Add(angry);
                        results.Add(fear);

                        sadList.Add(sad);
                        happyList.Add(happy);
                        angerList.Add(angry);
                        fearList.Add(fear);
                        neutralList.Add(neutral);
                    }
                    else
                    {

                        results.Add(-1);
                        results.Add(-1);
                        results.Add(-1);
                        results.Add(-1);
                        results.Add(-1);
                        Debug.Log(result["Error"]);
                    }
                }
                else
                {
                    Debug.Log("Something went wrong while copying the data ");
                    results.Add(-1);
                    results.Add(-1);
                    results.Add(-1);
                    results.Add(-1);
                    results.Add(-1);
                }
                actTime = Time.realtimeSinceStartup;
            }
        }

        /*
            Function used to set the search paths for python to find dependencies 
        */
        private void setSearchPaths()
        {
            ICollection<string> searchPaths = pyEngine.GetSearchPaths();
            searchPaths.Add(Application.dataPath + "/Libs/Vokaturi_Python/Python");
            searchPaths.Add(Application.dataPath + "/Libs/Vokaturi_Python/Python/Lib");
            pyEngine.SetSearchPaths(searchPaths);
        }

        public void fillChart()
        {
            float happy = 0;
            float sad = 0;
            float anger = 0;
            float fear = 0;
            float neutral = 0;

            for (int i = 0; i < happyList.Count; i++)
            {
                happy += happyList.ToArray()[i];
                sad += sadList.ToArray()[i];
                anger += angerList.ToArray()[i];
                fear += fearList.ToArray()[i];
                neutral += neutralList.ToArray()[i];
            }

            if (happyList.Count > 0)
            {
                happy /= happyList.Count;
                sad /= happyList.Count;
                anger /= happyList.Count;
                fear /= happyList.Count;
                neutral /= happyList.Count;
            }

            Debug.Log("Media Neutrality: " + neutral);
            Debug.Log("Media Happiness: " + happy);
            Debug.Log("Media Sadness: " + sad);
            Debug.Log("Media Anger: " + anger);
            Debug.Log("Media Fear: " + fear);

            mediaHappy = happy; mediaAnger = anger; mediaSad = sad; mediaFear = fear; mediaNeutral = neutral;

            float max = -1;
            float[] floats = { fear, anger, happy, sad, neutral };


            foreach (float number in floats)
            {
                if (number > max)
                {
                    max = number;
                }
            }

            // Les ponemos un mímino de 0.1 para que se pueda ver el gráfico correctamente

            fear = fear / max > (float)0.1 ? fear / max : (float)0.1;
            anger = anger / max > (float)0.1 ? anger / max : (float)0.1;
            sad = sad / max > (float)0.1 ? sad / max : (float)0.1;
            happy = happy / max > (float)0.1 ? happy / max : (float)0.1;
            neutral = neutral / max > (float)0.1 ? neutral / max : (float)0.1;

            finalHappy = happy; finalAnger = anger; finalSad = sad; finalFear = fear; finalNeutral = neutral;

            Debug.Log("Valor final Neutrality: " + neutral);
            Debug.Log("Valor final Happiness: " + happy);
            Debug.Log("Valor final Sadness: " + sad);
            Debug.Log("Valor final Anger: " + anger);
            Debug.Log("Valor final Fear: " + fear);

            radarPolygon = new RadarPolygon();
            radarPolygon.value[0] = fear;
            radarPolygon.value[1] = anger;
            radarPolygon.value[2] = sad;
            //radarPolygon.value[2] = happy;
            //radarPolygon.value[4] = neutral;

            //radarPolygon.SetAllDirty();
        }
    }
}