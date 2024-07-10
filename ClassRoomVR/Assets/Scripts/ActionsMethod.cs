using MathNet.Numerics.Distributions;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ClassRoomVR
{
    public static class ActionsMethod
    {

        public static IEnumerator SentarseJuntos(Student studentProb1, Student studentProb2, Student studentNear, DisruptiveAction action, System.Action onComplete)
        {
            studentProb1.SetProblematicStudent();
            studentProb1.MoveTo(studentNear.GetDesk().GetPositionStudent(), 1f);
            while (Vector3.Distance(studentProb1.transform.position, studentNear.transform.position) > 3f)
            {
                yield return null; // Espera al siguiente frame
            }
            studentProb1.GenerateText("Oye" + studentNear.name + ", me voy a sentar con " + studentProb2.name + "Asi que te cambio el sitio");
            // El primer estudiante le dice al objetivo que se quiere sentar con el compañero de al lado
            yield return new WaitForSeconds(6f);
            studentNear.GenerateText("De acuerdo");
            studentProb2.SetProblematicStudent();
            ClassManager.Instance.GetStudentsController().ChangeDesk(studentProb1, studentNear);
            // Ambos estudiantes se sientan juntos y notifican que ya están juntos
            while (studentProb1.state!=State.Sitting)
            {
                yield return null; // Espera al siguiente frame
            }
            studentProb1.GenerateText("Te voy a contar lo que hice ayer a ver si no nos cambia el profe");
            onComplete?.Invoke();

        }



        public static IEnumerator Insultar(Student student, DisruptiveAction action, System.Action onComplete)
        {
            AudioClip clip = student.GetGender() == Gender.Women ? action.situationAudioFeminine : action.situationAudioMasculine;
            student.PayAttention();
            student.PlayDisruptiveAction(action.problematicsAnimation.name, clip); yield return new WaitForSeconds(2f);
            onComplete?.Invoke();
        }


        public static void Levantarse(Student student, DisruptiveAction action, Vector3 destino, System.Action onComplete )
        {
            AudioClip clip = student.GetGender() == Gender.Women ? action.situationAudioFeminine : action.situationAudioMasculine;
            student.PayAttention();
            student.PlayDisruptiveAction(action.problematicsAnimation.name, clip);
            student.MoveTo(destino, 1f, onComplete);
        }

    }
}