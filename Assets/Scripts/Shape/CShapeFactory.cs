using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NSShapeModel;
using NSShapeView;

/*
 * Фабрика модели
 */
public class CShapeFactory : MonoBehaviour {
    public CVShape shapeView;
    
    // списки индексов точек, входящих в линии
    private int[][] linesOfPoints = {
        new int[] { 0, 1 }, new int[] { 0, 2 }, new int[] { 0, 3 }, new int[] { 0, 4 },
        new int[] { 1, 4, 2 }, new int[] { 1, 3 },
        new int[] { 2, 3 }
    };

    // списки индексов линий, входящих в плоскости
    private int[][] planesOfLines = {
        new int[]{ 0, 2, 5 }, new int[]{ 1, 2, 6 },
        new int[]{ 4, 5, 6 }, new int[]{ 0, 1, 3, 4 }
    };

    // позиции вершин для CShapeView
    private Vector3[] verticesPos = new Vector3[] {
        new Vector3(-1,0,0), new Vector3(-1,3,0),
        new Vector3(1,0,0), new Vector3(-1,0,1),
        new Vector3(0,1.5f,0),
    };


    private void Start() {
        int pointsCount = verticesPos.Length;
        CMShape model = new CMShape();
        shapeView.Init(model, verticesPos);
        shapeView.name = "Shape";
        model.Init(pointsCount, linesOfPoints, planesOfLines);
    }
}
