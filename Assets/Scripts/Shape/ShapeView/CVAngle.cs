using System;
using System.Collections.Generic;
using UnityEngine;
using NSShapeModel;

namespace NSShapeView {
    public class CVAngle : MonoBehaviour {
        public static CVAngle Create(CMAngle model, Transform parent,
            CVLineSegment segment1, CVLineSegment segment2)
        {
            var name = string.Format("angle {0}: {1} <-> {2}", model.Id, segment1.Model.Id, segment2.Model.Id);
            return new GameObject(name).AddComponent<CVAngle>().Init(model, parent, segment1, segment2);
        }

        public CMAngle Model { get; private set; }


        private CVAngle Init(CMAngle model, Transform parent,
            CVLineSegment segment1, CVLineSegment segment2)
        {
            CVPoint point = segment1.GetCommonPoint(segment2);
            // функция вычисления направлений линий, образующих угол
            Func<CVLineSegment, Vector3> getDir
                = segment => segment.RealDirection.normalized * (point == segment.FirstPoint ? 1 : -1);
            // создание обозначения угла
            const float radius = 0.2f;
            Vector3[] positions = null;
            if(model.IsRightAngle)
                positions = CalcRightAngleContour(radius, getDir(segment1), getDir(segment2));
            else
                positions = CalcAngleContour(radius, getDir(segment1), getDir(segment2));
            CreateLineRenderer(transform, positions);

            // привязка обозначения угла к месту угла
            transform.parent = parent;
            transform.localPosition = point.RealPosition;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            Model = model;

            return this;
        }

        private void OnDestroy() { if (Model != null) Model.Dispose(); }

        private static LineRenderer CreateLineRenderer(Transform parent, Vector3[] positions) {
            LineRenderer lineRenderer = new GameObject("lineRenderer").AddComponent<LineRenderer>();
            lineRenderer.transform.parent = parent;
            lineRenderer.transform.localPosition = Vector3.zero;
            lineRenderer.startWidth = lineRenderer.endWidth = contourWidth;
            lineRenderer.useWorldSpace = false;

            // создание текстуры для LineRenderer
            var tex2d = new Texture2D(1, 6);
            tex2d.SetPixel(0, 0, Color.black);
            tex2d.SetPixel(0, tex2d.height - 1, Color.black);
            for (int i = 1; i < tex2d.height - 1; i++)
                tex2d.SetPixel(0, i, Color.yellow);
            tex2d.Apply();

            // создание и применение материала
            var material = new Material(Shader.Find("Unlit/Texture"));
            material.mainTexture = tex2d;
            material.color = new Color(1, 0, 1, 1); // pink
            lineRenderer.material = material;

            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);

            return lineRenderer;
        }

        private Vector3[] CalcAngleContour(float radius, Vector3 dir1, Vector3 dir2) {
            const int pointsCount = 16;
            Vector3[] positions = new Vector3[pointsCount];
            // взять точку центра плоскости
            for (int i = 0; i < pointsCount; i++) {
                var dir = Vector3.Lerp(dir1, dir2, (float)i / (pointsCount - 1)).normalized;
                positions[i] = dir * radius;
            }
            return positions;
        }
        private Vector3[] CalcRightAngleContour(float radius, Vector3 dir1, Vector3 dir2) {
            const int pointsCount = 3;
            Vector3[] positions = new Vector3[pointsCount];
            positions[0] = dir1.normalized * radius;
            positions[2] = dir2.normalized * radius;
            positions[1] = positions[0] + positions[2];
            return positions;
        }


        private const float contourWidth = 0.08f;

    }
}
