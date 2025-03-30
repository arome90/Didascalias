using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLight : MonoBehaviour
{
    public Light lightToToggle;

    public void LightToggle()
    {
        lightToToggle.enabled = !lightToToggle.enabled;
    }
}
