using System;
using System.Collections.Generic;
using UnityEngine;
using NSShapeModel;


namespace NSShapeView {
    public class CVLine : IDisposable {
        public CVLine(Transform segmentsOwner, CMLine model) {
            Model = model;
            SegmentsOwner = segmentsOwner;
        }

        public void Dispose() {
            foreach (var segment in segments)
                GameObject.Destroy(segment.gameObject);
            segments.Clear();
            points.Clear();
            Selected = null;
            Model.Dispose();
            Model = null;
        }

        public void AddPoint(CVPoint point) {
            if(point.Model.Id != Model.PointAt(points.Count).Id)
                throw new Exception("line model doesn't contain this point (line id: " 
                    + Model.Id + ", point id: " + point.Model.Id);
            points.Add(point);
            if (points.Count > 1) {
                CreateSegment(points[points.Count - 2].Position, point.Position);
            }
        }
        public void InsertPoint(CVPoint point, int segmentIndex) {
            Vector3 startPos = points[segmentIndex].Position;
            Vector3 endPos = points[segmentIndex + 1].Position;

            Model.Insert(segmentIndex + 1, point.Model);
            points.Insert(segmentIndex + 1, point);

            segments[segmentIndex].SetPositions(startPos, point.Position); // modify segment like part 1
            CreateSegment(point.Position, endPos, segmentIndex + 1); // create segment like part 2
        }
        public void InsertPoint(CVPoint point, int segmentIndex, float ratio) {
            point.Position = Vector3.Lerp(points[segmentIndex].Position, points[segmentIndex + 1].Position, ratio);
            InsertPoint(point, segmentIndex);
        }
        public void RemovePoint(CVPoint point) {
            int pointInd = points.IndexOf(point);
            if (pointInd < 0)
                throw new Exception("CLineView hasn't CPointView (line model id: "
                    + Model.Id + ", point model id" + point.Model.Id + ")");

            Model.Detach(point.Model);
            points.RemoveAt(pointInd); // индексы и количество сдвинулись на один!
            if (points.Count == 0)
                return;

            RemoveSegment(pointInd - (pointInd == points.Count ? 1 : 0)); // если точка последняя, удалить отрезок перед ней
            if(pointInd > 0 && points.Count > 1)
                segments[pointInd - 1].SetPositions(points[pointInd - 1].Position, points[pointInd].Position);
        }

        public CVLineSegment SegmentAt(int index) { return segments[index]; }
        public CVPoint PointAt(int index) { return points[index]; }
        // количество отрезков
        public int Count { get { return segments.Count; } }
        public int SegmentIndex(CVLineSegment lineSegment) { return segments.IndexOf(lineSegment); }

        public Vector3 RealDirection {
            get {
                return points[points.Count - 1].transform.localPosition 
                    - points[0].transform.localPosition;
            }
        }
        public Vector3 Direction { get { return CVShape.DecvtAxes(RealDirection); } }

        public Vector3 CalcHitPosition(int segmentIndex, Vector3 hitPosition) {
            Vector3 startPos = points[segmentIndex].Position;
            Vector3 endPos = points[segmentIndex + 1].Position;
            Transform space = SegmentAt(segmentIndex).transform.parent;
            Vector3 hitPos = CVShape.DecvtAxes(space.worldToLocalMatrix * hitPosition);

            Vector3 direction = endPos - startPos;
            Vector3 diffrent = startPos - hitPos;
            float t = -Vector3.Dot(direction, diffrent) / direction.sqrMagnitude;
            return t * direction + startPos;
        }

        public CMLine Model { get; private set; }
        public Transform SegmentsOwner { get; private set; }

        public event Action<CVLine, CVLineSegment> Selected;


        private void CreateSegment(Vector3 startPos, Vector3 endPos) {
            CreateSegment(startPos, endPos, segments.Count);
        }
        private void CreateSegment(Vector3 startPos, Vector3 endPos, int index) {
            var segment = CVLineSegment.Create(this, SegmentsOwner, startPos, endPos);
            segment.MouseDown += Segment_MouseDown;
            segments.Insert(index, segment);
        }
        private void Segment_MouseDown(AVPrimitive lineSegment) {
            if (Selected != null)
                Selected(this, (CVLineSegment)lineSegment);
        }

        private void RemoveSegment(int index) {
            var segment = segments[index];
            segment.MouseDown -= Segment_MouseDown;
            segments.RemoveAt(index);
            GameObject.Destroy(segment.gameObject);
        }

        private List<CVLineSegment> segments = new List<CVLineSegment>(1);
        private List<CVPoint> points = new List<CVPoint>(2);
    }
}
