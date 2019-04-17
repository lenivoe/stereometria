using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace NSShapeModel {
    public class CMLine : AMPrimitive, IPointLinkable {
        public CMLine(CMShape owner, int id, IEnumerable<int> pointsId) : base(owner, id) {
            foreach (var cur in pointsId)
                PushPointBack(cur);
        }

        #region IDisposable Support
        protected override void Dispose(bool disposing) {
            if (disposed) return;
            if (disposing) 
                DetachAllPoints();
            disposed = true;
            base.Dispose(disposing);
        }
        private bool disposed = false; // Для определения избыточных вызовов
        #endregion


        public CMPoint FirstPoint { get { return PointAt(0); } }
        public CMPoint LastPoint { get { return PointAt(pointsList.Count - 1); } }

        public void PushPointFront(CMPoint point) { PushPointFront(point.Id); }
        public void PushPointBack(CMPoint point) { PushPointBack(point.Id); }

        public void Insert(int index, CMPoint point) {
            pointsDict.Add(point.Id, point);
            pointsList.Insert(index, point);
            point.Attach(this);
        }
        public void Detach(CMPoint point) {
            pointsDict.Remove(point.Id);
            pointsList.Remove(point);
            if (point.Contains(this))
                point.Detach(this);
        }
        public void DetachAllPoints() {
            // очистка pointsDict, чтобы HasPoint возвращал 0, 
            // чтобы point.DetachLine не пытался вызвать CLineModel.DetachPoint
            pointsDict.Clear();
            foreach (var point in pointsList)
                if (point.Contains(this))
                    point.Detach(this);
            pointsList.Clear();
        }

        public bool Contains(CMPoint point) { return pointsDict.ContainsKey(point.Id); }
        public CMPoint PointAt(int index) { return pointsList[index]; }
        public int PointsCount { get { return pointsList.Count; } }
        
        public CMPoint GetCommonPoint(CMLine line) {
            return pointsDict.Values.FirstOrDefault(point => point.Contains(line));
        }
        public int DetectDir(CMPoint start, CMPoint end) {
            if (!Contains(start) || !Contains(end))
                throw new ArgumentException("Both points must be attached to the line (line id: " + Id + ")");
            if (start == end) return 0;
            var result = pointsList.Find(point => point == start || point == end);
            return result == start ? 1 : -1;
        }


        private void InsertPoint(int index, int pointId) { Insert(index, Owner[pointId] as CMPoint); }
        private void PushPointFront(int pointId) { InsertPoint(0, pointId); }
        private void PushPointBack(int pointId) { InsertPoint(PointsCount, pointId); }


        private Dictionary<int, CMPoint> pointsDict = new Dictionary<int, CMPoint>();
        private List<CMPoint> pointsList = new List<CMPoint>();
    }
}
