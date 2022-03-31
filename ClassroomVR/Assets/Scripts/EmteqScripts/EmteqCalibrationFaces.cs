using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using EmteqLabs.Faceplate;
using EmteqLabs.MaskProtocol;
using UnityEngine;
using UnityEngine.Video;

namespace EmteqLabs
{
    public class EmteqCalibrationFaces : MonoBehaviour
    {
        public VideoPlayer _vp;


        [Serializable]
        public class ListStorage : SerializableDictionary.Storage<ushort[]> { }



        private ushort[] pru;

        Musculos m;

        public ushort[] happy;
        public ushort[] sad;
        public ushort[] neutral;
        [Serializable]
        public class Musculos : SerializableDictionary<string, ushort[], ListStorage>
        { }

        // Start is called before the first frame update
        void Awake()
        {

            m = leerfichero();
            if (m == null)
            {
                m = new Musculos();
            }
            pru = new ushort[7];
            happy = new ushort[7];
            sad = new ushort[7];
            neutral = new ushort[7];
            /*if (_vp.isPlaying)
            {
                Debug.Log("Llamada incial");
                EmteqVRManager.StartRecordingData();
            }*/
        }
        bool firstCall = false;
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _vp.Play();
                Invoke("IniciarGrabarEmteq", 1);
            }


            if (Input.GetKeyDown(KeyCode.H))
            {
                //happy=captura();
                m["happy"] = captura();

            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                m["sad"] = captura();
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                m["neutral"] = captura();
            }
            // EmteqVRManager.GetEmgAmplitudeRms();
            //if (frame == momento captura) neutral = GetEmgAmplitudeRms();
            //if (frame == momento captura) happy = GetEmgAmplitudeRms();
            //if (frame == momento captura) sad = GetEmgAmplitudeRms();
            CompruebaExpresiones();
        }


        void paraDegrabar()
        {
            Debug.Log("Grabar final");


            if (EmteqVRManager.IsDeviceConnected())
            {
                //  EmteqVRManager.EndDataSection("hola", m);

                EmteqVRManager.StopRecordingData();

            }
        }


        void IniciarGrabarEmteq()
        {
            Debug.Log("Grabar inicial");

            if (EmteqVRManager.IsDeviceConnected())
            {
                EmteqVRManager.StartRecordingData();
            }
            Invoke("paraDegrabar", 5);

        }

        private void OnApplicationQuit()
        {
            Debug.Log("finish");
            // guardarfichero();
        }

        void guardarfichero()
        {
            //Manera 1
            string json = JsonUtility.ToJson(m, true);

            string path = Application.persistentDataPath + "/save.json";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.WriteAllText(path, json);

            //Manera 2
            //EmteqVRManager.SetDataPoint("hola", m);
            //EmteqVRManager.StartDataSection("hola", m);
        }

        Musculos leerfichero()
        {
            string path = Application.persistentDataPath + "/save.json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                Musculos data = JsonUtility.FromJson<Musculos>(json);

                return data;
            }
            else
            {
                return null;

            }
        }
        ushort[] captura()
        {
            Dictionary<MuscleMapping, ushort> dic = EmteqVRManager.GetEmgAmplitudeRms();

            
            foreach (KeyValuePair<MuscleMapping, ushort> v in dic)
            {
                pru[((int)v.Key)] = v.Value;
            }
            return pru;
        }


        void CompruebaExpresiones()
        {
            string similarexp = "";
            int min = int.MaxValue;
            captura();
            foreach (KeyValuePair<string, ushort[]> v in m)
            {

                int sum = 0;
                for (int i = 0; i < 7; i++)
                {
                    sum += Math.Abs(pru[i] - v.Value[i]);
                }

                if (sum < min) { min = sum; similarexp = v.Key; }
            }

           
            ClassRoomVR.GameManager.Instance._sceneManager.uiManager.ChangeExpressions(similarexp);

        }
    }

}
