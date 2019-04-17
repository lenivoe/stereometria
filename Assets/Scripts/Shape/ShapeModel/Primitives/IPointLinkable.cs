using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NSShapeModel {
    public interface IPointLinkable {
        void Insert(int index, CMPoint point);
        void Detach(CMPoint point);
        void DetachAllPoints();

        bool Contains(CMPoint point);
        CMPoint PointAt(int index);
        int PointsCount { get; }

    }
}
