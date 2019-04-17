using System;
using System.Collections.Generic;
using UnityEngine;
using NSShapeModel;


namespace NSShapeView {
    public class CVLineSegment : AVCarcassElements {
        public static CVLineSegment Create(CVLine line, Transform parent, Vector3 localStartPos, Vector3 localEndPos) {
            return GameObject.CreatePrimitive(PrimitiveType.Cylinder)
                .AddComponent<CVLineSegment>()
                .Init(line, parent, localStartPos, localEndPos);
        }
        
        public void SetPositions(Vector3 localStartPos, Vector3 localEndPos) {
            const float width = 0.03f;
            localStartPos = CVShape.CvtAxes(localStartPos);
            localEndPos = CVShape.CvtAxes(localEndPos);
            Vector3 localDiff = transform.parent.rotation * (localEndPos - localStartPos);
            transform.localPosition = (localStartPos + localEndPos) * 0.5f;
            transform.localScale = new Vector3(width, localDiff.magnitude * 0.5f, width);
            transform.up = localDiff.normalized;
            StartCoroutine(CalcSegmentName());
        }
        public CVPoint FirstPoint { get { return Owner.PointAt(IndexInLine); } }
        public CVPoint LastPoint { get { return Owner.PointAt(IndexInLine + 1); } }

        public Vector3 RealDirection { get { return Owner.RealDirection; } }
        public Vector3 Direction { get { return Owner.Direction; } }

        public CVPoint GetCommonPoint(CVLineSegment segment) {
            if (FirstPoint == segment.FirstPoint || FirstPoint == segment.LastPoint)
                return FirstPoint;
            if (LastPoint == segment.FirstPoint || LastPoint == segment.LastPoint)
                return LastPoint;
            return null;
        }

        public CVLine Owner { get; private set; }
        public CMLine Model { get { return Owner.Model; } }
        public int IndexInLine {
            get {
                if(lastIndex >= Owner.Count || Owner.SegmentAt(lastIndex) != this)
                    lastIndex = Owner.SegmentIndex(this);
                return lastIndex;
            }
        }

        public string Name { get { return FirstPoint.Name + LastPoint.Name; } }

        
        private CVLineSegment Init(CVLine line, Transform parentObj, Vector3 localPos1, Vector3 localPos2) {
            Owner = line;
            transform.parent = parentObj;

            SetPositions(localPos1, localPos2);
            var collider = transform.GetComponent<CapsuleCollider>();
            collider.radius = 2;
            collider.height = 1.9f;
            name = string.Format("line {0}({1})", Model.Id, Owner.Count);

            StartCoroutine(CalcSegmentName());

            return this;
        }

        private void OnDestroy() { if (Owner.Count == 0 && Model != null) { Model.Dispose(); } }

        System.Collections.IEnumerator CalcSegmentName() {
            yield return null;
            name = "line " + Name;
        }


        private int lastIndex = 0;
    }
}
