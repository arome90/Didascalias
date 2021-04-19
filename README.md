# Classroom VR (Español)
Trabajo de Fin de Grado para el Grado en Ingeniería Informática de la Universidad Complutense de Madrid realizado por Antonio Luis Suarez, Daniel Lopez, Sandra Alonso y Andrés Puente.


## VR
Hemos realizado la integración de Oculus siguiendo la documentacion oficial de Oculus en https://developer.oculus.com/documentation/unity/unity-gs-overview/.

## Estructura del proyecto
El proyecto de Unity consta de 2 escenas principales, ambas escenas tienen un objeto prefab "GameManager" que gestiona el desarrollo de la app. Este objeto es singleton y se va pasando de escena a escena actualizando su informacion.

### MenuScene
El manager de esta escena es "MenuManager"

### PlayScene
El manager de esta escena es "MySceneManager"

---

# WIP
- Quitar el componente de PoseBuilder al player normal, ya que solo al vr se le detectara la posicion (dejarlo mientras pruebas)
- Arreglar/modificar el poseBuilder para la deteccion de la apertura de las manos.
- Arreglar menus (textos y panelFinal)
- Pausar app con opciones --DONE
- Escena inicio
- Manos
- Feedback final (camino, emoPose, "voz") --emoPose DONE
- Grabar mas emoPose
- Player con vr no detecta colision con objetos
- Gravar animacion de sentarse para los alumnos ;)
- Confirmar que se puede eliminar el script "PlayerMovement" (deberia poder quitarse)

---

# Classroom VR (English)
End of Degree Project for Computer Engineering at Universidad Complutense de Madrid made by Antonio Luis Suarez, Daniel Lopez, Sandra Alonso y Andrés Puente.

## VR
We have performed the Oculus integraton by following the official Oculus Documentation at https://developer.oculus.com/documentation/unity/unity-gs-overview/.



