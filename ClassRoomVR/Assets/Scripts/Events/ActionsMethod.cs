using System.Collections;
using UnityEngine;

namespace ClassRoomVR
{
    /// <summary>
    /// Métodos estáticos para manejar acciones disruptivas de los estudiantes.
    /// </summary>
    public static class ActionsMethod
    {
        /// <summary>
        /// Reproduce la animación y el audio apropiado para una acción disruptiva, basado en el género del estudiante.
        /// </summary>
        /// <param name="student">El estudiante que realiza la acción.</param>
        /// <param name="action">La acción disruptiva que realizará el estudiante.</param>
        private static void PlayDisruptiveAction(Student student, DisruptiveAction action)
        {
            student.PayAttention(); // Indica que el estudiante está prestando atención.

            // Reproduce la animación y el audio correspondiente según el género.
            var audioClip = student.GetGender() == Gender.Women
                ? action.SituationAudioFeminine
                : action.SituationAudioMasculine;

            student.PlayDisruptiveAction(action.ProblematicAnimation.name, audioClip);
        }

        /// <summary>
        /// Corrutina que simula a dos estudiantes sentándose juntos, intercambiando asientos con otro estudiante.
        /// </summary>
        /// <param name="student1">Primer estudiante problemático.</param>
        /// <param name="student2">Segundo estudiante problemático.</param>
        /// <param name="studentNear">Estudiante cercano al cual le cambiarán el sitio.</param>
        /// <param name="action">Acción disruptiva a ejecutar.</param>
        /// <param name="onComplete">Acción a ejecutar al finalizar la corrutina.</param>
        /// <returns>Retorna un IEnumerator necesario para las corrutinas.</returns>
        public static IEnumerator SitTogether(Student student1, Student student2, Student studentNear, DisruptiveAction action, System.Action onComplete)
        {
            student1.SetProblematicStudent(); // Marca al primer estudiante como problemático.
            student1.MoveTo(studentNear.GetDesk().GetStudentPosition(), 1f); // Mueve al estudiante al asiento del estudiante cercano.

            // Espera hasta que el estudiante se acerque lo suficiente.
            yield return new WaitUntil(() => Vector3.Distance(student1.transform.position, studentNear.transform.position) <= 3f);

            // Genera un texto indicando que cambiarán de asiento.
            student1.GenerateText($"Oye {studentNear.name}, me voy a sentar con {student2.name}. Así que te cambio el sitio");

            yield return new WaitForSeconds(6f); // Espera 6 segundos para simular una acción más natural.

            studentNear.GenerateText("De acuerdo"); // El estudiante acepta el cambio de lugar.
            student2.SetProblematicStudent(); // Marca al segundo estudiante como problemático.

            // Cambia los asientos de los estudiantes.
            ClassManager.Instance.GetStudentsController().ChangeDesk(student1, studentNear);

            // Espera hasta que el primer estudiante esté sentado.
            yield return new WaitUntil(() => student1.GetState() == State.Sitting);

            student1.GenerateText("Te voy a contar lo que hice ayer a ver si no nos cambia el profe"); // Inicia una conversación disruptiva.

            onComplete?.Invoke(); // Llama a la acción final si está definida.
        }

        /// <summary>
        /// Corrutina que simula a un estudiante insultando.
        /// </summary>
        /// <param name="student">El estudiante que realizará el insulto.</param>
        /// <param name="action">La acción disruptiva a ejecutar.</param>
        /// <param name="onComplete">Acción a ejecutar al finalizar la corrutina.</param>
        /// <returns>Retorna un IEnumerator necesario para las corrutinas.</returns>
        public static IEnumerator Insult(Student student, DisruptiveAction action, System.Action onComplete)
        {
            PlayDisruptiveAction(student, action); // Ejecuta la acción disruptiva.
            yield return new WaitForSeconds(2f); // Espera 2 segundos.
            onComplete?.Invoke(); // Llama a la acción final si está definida.
        }

        /// <summary>
        /// Levanta al estudiante de su asiento y lo mueve a una posición específica.
        /// </summary>
        /// <param name="student">El estudiante que se levantará y moverá.</param>
        /// <param name="action">La acción disruptiva a ejecutar.</param>
        /// <param name="destination">La posición a la que se moverá el estudiante.</param>
        /// <param name="onComplete">Acción a ejecutar al finalizar el movimiento.</param>
        public static void StandUpAndMove(Student student, DisruptiveAction action, Vector3 destination, System.Action onComplete)
        {
            PlayDisruptiveAction(student, action); // Ejecuta la acción disruptiva.
            student.MoveTo(destination, 1f, onComplete); // Mueve al estudiante al destino con una velocidad definida.
        }
    }
}
