using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace ClassRoomVR
{
    public class KeyWordRecognizer : MonoBehaviour
    {
        public delegate void DelegateMethod(int i);

        KeywordRecognizer keywordRecognizer;
        Dictionary<string, UnityAction> keywords = new Dictionary<string, UnityAction>();

        public UnityEvent assertiveWordEvent;
        public UnityEvent authoritativeWordEvent;

        public UnityEvent thirdScenarioSittingEvent;
        public UnityEvent thirdScenarioBackEvent;

        void Start()
        {
            /*
            foreach (string word in Constants.ASSERTIVE_WORDS)
            {
                keywords.Add(word, () =>
                {
                    assertiveWordEvent.Invoke();
                });
            }
            foreach (string word in Constants.AUTHORITATIVE_WORDS)
            {
                keywords.Add(word, () =>
                {
                    authoritativeWordEvent.Invoke();
                });
            }
            foreach (string word in Constants.SITTING_WORDS)
            {
                keywords.Add(word, () =>
                {
                    thirdScenarioSittingEvent.Invoke();
                });
            }
            foreach (string word in Constants.END_WORDS)
            {
                keywords.Add(word, () =>
                {
                    thirdScenarioBackEvent.Invoke();
                });
            }
            */

            // Hay que reinicializarlo para cuando se le da a reintentar un escenario,
            // ya que añadiría más diccionarios con palabras ya contenidas y saltaría una excepción
            /*
            if (keywordRecognizer != null)
            {
                keywordRecognizer.Stop();
                keywordRecognizer.Dispose();
            }

            keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
            keywordRecognizer.Start();
            */
        }

        public void init()
        {
            if (keywordRecognizer != null)
            {
                keywordRecognizer.Stop();
                keywordRecognizer.Dispose();
            }

            keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
            keywordRecognizer.Start();
        }

        public void addWordsToKeyWord(string[] words, int i, DelegateMethod eventToWord)
        {
            foreach (string w in words)
            {
                if (!keywords.ContainsKey(w))
                {
                    keywords.Add(w, () =>
                    {
                        eventToWord(i);
                    });
                }
            }
        }

        private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            UnityAction keywordAction;
            if (keywords.TryGetValue(args.text, out keywordAction))
            {
                Debug.Log("Se ha reconocido: " + args.text);
                keywordAction();
                CSVSerializer.storeData("Se ha reconocido: " + args.text);
            }
        }
    }
}