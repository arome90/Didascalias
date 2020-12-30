# Classroom VR (Español)
Trabajo de Fin de Grado para el Grado en Ingeniería Informática de la Universidad Complutense de Madrid realizado por Mario Bocos Corredor, Alejandro Díaz Nieto y Álvaro López García.


## VR
Hemos realizado la integración de Oculus siguiendo la documentacion oficial de Oculus en https://developer.oculus.com/documentation/unity/unity-gs-overview/.

Hay 2 ramas en el repositorio:
### master
Los 3 escenarios se manejan como cualquier videojuego normal, con teclado (WASD) para moverse y ratón para mover la camara y navegar por los menús.
### Oculus
La escena "Escenario2" contiene modificaciones realizadas por Meriem El Yamri (miembro del equipo de investigación), que nos ayudó al tener acceso a las gafas de Realidad Virtual.

Estas modificaciones consisten en meter el rig de Oculus en la escena (Player -> Look Root -> OVRCameraRig) y una cámara (Player -> Look Root -> OVRCameraRig -> TrackingSpace -> CenterEyeAnchor -> UI Camera) para renderizar el canvas, el cual está ahora en World Space para una correcta visualización.

Con estas modificaciones, es posible utlizar las gafas para ver dicho escenario. Sin embargo, es necesario mapear los controles de movimiento del teclado y ratón (WASD para moverse y click para navegar por los menús, ya que el canvas ya no trackea eventos) para que el juego sea manejable mediante los controladores de Realidad Virtual.

---

# Classroom VR (English)
End of Degree Project for Computer Engineering at Universidad Complutense de Madrid made by Mario Bocos Corredor, Alejandro Díaz Nieto and Álvaro López García.

## VR
We have performed the Oculus integraton by following the official Oculus Documentation at https://developer.oculus.com/documentation/unity/unity-gs-overview/.

There are 2 branches in this repository:
### master
All 3 scenarios are played as in any other video game, by using the keyboard (WASD) to move around and the mouse to move the camera and navigate the menus.
### Oculus
The "Escenario2" scene contains modifications made by Meriem El Yamri (research team member), who helped us as she had access to the Virtual Reality goggles.

These modifications consist on including the Oculus rig in the scene (Player -> Look Root -> OVRCameraRig) and a camera (Player -> Look Root -> OVRCameraRig -> TrackingSpace -> CenterEyeAnchor -> UI Camera) to render the canvas, which is now in World Space for a correct visualization.

With these medifications, it is possible to use the goggles to observe that scenario. However, it is necessary to map the keyboard and mouse movement controls (WASD to move around and click to navigate through the menus, since the canvas does not track events anymore) for the game to be playable through the Virtual Reality Controllers.
