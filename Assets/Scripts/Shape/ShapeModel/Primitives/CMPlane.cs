using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace NSShapeModel {
    public class CMPlane : AMPrimitive, ILineLinkable {
        public CMPlane(CMShape owner, int id, IEnumerable<int> linesId) : base(owner, id) {
            foreach (var lineId in linesId)
                Attach(owner.PrimitiveAt(lineId) as CMLine);
        }

        #region IDisposable Support
        protected override void Dispose(bool disposing) {
            if (disposed) return;
            if (disposing)
                DetachAllLines();
            disposed = true;
            base.Dispose(disposing);
        }
        private bool disposed = false; // Для определения избыточных вызовов
        #endregion

        #region ILineLinkable Support
        public void Attach(CMLine line) { lines.Add(line.Id, line); }
        public void Detach(CMLine line) { lines.Remove(line.Id); }
        public void DetachAllLines() { lines.Clear(); }
        
        public bool Contains(CMLine line) { return lines.ContainsKey(line.Id); }
        public CMLine LineAt(int index) { return lines[index]; }
        public int LinesCount { get { return lines.Count; } }
        #endregion
        

        public bool Contains(CMPoint point) { return lines.Values.Any(line => line.Contains(point)); }


        private Dictionary<int, CMLine> lines = new Dictionary<int, CMLine>();
    }
}
