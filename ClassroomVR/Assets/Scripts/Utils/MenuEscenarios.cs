using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuEscenarios : MonoBehaviour
{
    public void PlayEscenario1() {
        SceneManager.LoadScene("Escenario1");
    }
    public void PlayEscenario2()
    {
        SceneManager.LoadScene("Escenario2");
    }
    public void PlayEscenario3() {
        SceneManager.LoadScene("Escenario3");
    }
    public void CloseGame()
    {
       // UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
}
