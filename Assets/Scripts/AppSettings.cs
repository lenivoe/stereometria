using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppSettings : MonoBehaviour {
	private void Awake () {
        Resolution[] resolutions = Screen.resolutions;
        Resolution maxResolution = resolutions[0];
        foreach(var resolution in resolutions) {
            if (maxResolution.width < resolution.width)
                maxResolution = resolution;
        }
        Screen.SetResolution(maxResolution.width, maxResolution.height, true);
	}

    private void Update() {
        bool needQuit = Input.GetKeyDown(KeyCode.Escape) || Input.GetKey(KeyCode.AltGr) && Input.GetKey(KeyCode.F4);
        if (needQuit)
            Application.Quit();
    }
}
