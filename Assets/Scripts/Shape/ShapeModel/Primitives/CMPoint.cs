using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace NSShapeModel {
    public class CMPoint : AMPrimitive, ILineLinkable {
        public CMPoint(CMShape owner, int id) : base(owner, id) { }

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

        public void Attach(CMLine line) {
            // неизвестно куда вставлять точку, так что она должна быть уже вставлена в линию
            if (!line.Contains(this))
                throw new Exception("Line hasn't point yet (point id: " + Id + ", line id: " + line.Id + ")");
            lines.Add(line.Id, line);
        }
        public void Detach(CMLine line) {
            lines.Remove(line.Id);
            if (line.Contains(this))
                line.Detach(this);
        }
        public void DetachAllLines() {
            var lastLines = lines; // резервируем имеющийся список
            lines = new Dictionary<int, CMLine>(); // разрываем связь точки с прямыми
            foreach (var line in lastLines.Values) // разрываем связь прямых с точкой
                if (line.Contains(this))
                    line.Detach(this);
        }
        
        public bool Contains(CMLine line) { return lines.ContainsKey(line.Id); }
        public CMLine LineAt(int index) { return lines[index]; }
        public int LinesCount { get { return lines.Count; } }

        // возвращает общую прямую, если она есть и null, если ее нет
        public CMLine GetCommonLine(CMPoint point) {
            return lines.Values.FirstOrDefault(line => line.Contains(point));
        }


        private Dictionary<int, CMLine> lines = new Dictionary<int, CMLine>();
    }
}
