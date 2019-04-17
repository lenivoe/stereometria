using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace NSShapeModel {
    public class CMAngle : AMRelation {
        public enum AngleDir { FrontFront, BackFront, BackBack, FrontBack }

        public CMAngle(int id, CMLine line1, CMLine line2, int? angle = null) : base(line1.Owner, id) {
            Line1 = line1;
            Line2 = line2;
            Point = line1.GetCommonPoint(line2);
            this.angle = angle;
        }
        public CMAngle(int id, CMPoint start, CMPoint end1, CMPoint end2, int? angle = null) : base(start.Owner, id) {
            Point = start;
            Line1 = start.GetCommonLine(end1);
            Line2 = start.GetCommonLine(end2);
            this.angle = angle;
            if (Line1.DetectDir(start, end1) > 1)
                Dir = (Line2.DetectDir(start, end2) > 1 ? AngleDir.FrontFront : AngleDir.FrontBack);
            else
                Dir = (Line2.DetectDir(start, end2) > 1 ? AngleDir.BackFront : AngleDir.BackBack);
        }

        public override bool IsCorrect { get { return true; } }
        public override int Count { get { return lines.Length; } }
        public override bool Contains(AMPrimitive model) { return lines.Contains(model); }
        public override AMPrimitive PrimitiveAt(int index) { return lines[index]; }

        public AngleDir Dir { get; private set; }
        public CMPoint Point { get; private set; }
        public CMLine Line1 { get; private set; }
        public CMLine Line2 { get; private set; }
        public int Angle { get { return angle.Value; } }
        public bool HasValue { get { return angle.HasValue; } }
        public bool IsRightAngle { get { return HasValue && Angle == 90; } }


        private int? angle = null;
        private CMLine[] lines = new CMLine[3];
    }
}
