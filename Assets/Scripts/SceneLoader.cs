using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {
    public float rotationSpeed = 1;
    public GameObject space;


    public void LoadEditorScene() {
        SceneManager.LoadScene(1);
    }

    public void Quit() {
        Application.Quit();
    }


	private void Update () {
        space.transform.Rotate(space.transform.up, rotationSpeed);
	}
}
