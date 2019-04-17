using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using NSShapeView;

public class UIMenuController : MonoBehaviour {
    public void CreatePoint() {
        GoTo(pointMenu);
        ShapeView.CurState = CVShape.State.CreatePoint;
    }
    public void SetAngle() {
        GoTo(angleMenu);
        ShapeView.CurState = CVShape.State.SetAngle;
    }
    public void BuildLine() {
        GoTo(lineMenu);
        ShapeView.CurState = CVShape.State.BuildLine;
    }
    public void DivideLine() {
        GoTo(divideMenu);
        ShapeView.CurState = CVShape.State.DivideLine;
    }
    public void CrossLines() {
        GoTo(crossLinesMenu);
        //ShapeView.CurState = CShapeView.State.CrossLines;
    }

    public void ToStartScene() {
        SceneManager.LoadScene(0);
    }

    public void Back() {
        menuStack.Pop().SetActive(false);
        menuStack.Peek().SetActive(true);
        ShapeView.Abort();
    }

    public void Apply() {
        CVShape.IData data = null;
        switch(ShapeView.CurState) {
            case CVShape.State.DivideLine:
                var firstField = divideMenu.transform.Find("FirstPart").GetComponent<InputField>();
                var secondField = divideMenu.transform.Find("SecondPart").GetComponent<InputField>();
                try {
                    float first = (float)Convert.ToInt32(firstField.text);
                    float second = (float)Convert.ToInt32(secondField.text);
                    data = new CVShape.LineDividingData(first / (first + second));
                } catch(FormatException ) {
                    firstField.text = secondField.text = "";
                    return;
                }
                break;
            case CVShape.State.SetAngle:
                var angleValueField =  angleMenu.transform.Find("AngleValue").GetComponent<InputField>();
                int result = 0;
                if (!int.TryParse(angleValueField.text, out result))
                    return;
                data = new CVShape.AngleSettingData(result);
                break;
        }
        ShapeView.Apply(data);
    }

    
    private void GoTo(GameObject menu) {
        menuStack.Peek().SetActive(false);
        menuStack.Push(menu);
        menuStack.Peek().SetActive(true);
    }

    private CVShape ShapeView {
        get {
            if (shapeView == null)
                shapeView = FindObjectOfType<CVShape>();
            return shapeView;
        }
    }
    private CVShape shapeView = null;

    private void Awake() {
        canvas = GetComponent<Canvas>();
        for(int i = 0; i < transform.childCount; i++) {
            GameObject child = transform.GetChild(i).gameObject;
            switch(child.name) {
                case mainMenuName: mainMenu = child; break;
                case pointMenuName: pointMenu = child; break;
                case angleMenuName: angleMenu = child; break;
                case lineMenuName: lineMenu = child; break;
                case divideMenuName: divideMenu = child; break;
                case crossLinesMenuName: crossLinesMenu = child; break;
            }
        }

        mainMenu.SetActive(true);
        pointMenu.SetActive(false);
        angleMenu.SetActive(false);
        lineMenu.SetActive(false);
        divideMenu.SetActive(false);
        crossLinesMenu.SetActive(false);

        menuStack.Push(mainMenu);
    }
    

    public Canvas canvas { get; private set; }
    public GameObject mainMenu { get; private set; }
    public GameObject pointMenu { get; private set; }
    public GameObject angleMenu { get; private set; }
    public GameObject lineMenu { get; private set; }
    public GameObject divideMenu { get; private set; }
    public GameObject crossLinesMenu { get; private set; }


    private const string mainMenuName = "MainMenu";
    private const string pointMenuName = "PointMenu";
    private const string angleMenuName = "AngleMenu";
    private const string lineMenuName = "LineMenu";
    private const string divideMenuName = "DivideMenu";
    private const string crossLinesMenuName = "CrossLinesMenu";

    private Stack<GameObject> menuStack = new Stack<GameObject>();
}
