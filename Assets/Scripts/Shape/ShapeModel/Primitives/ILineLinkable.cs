using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NSShapeModel {
    public interface ILineLinkable {
        void Attach(CMLine line);
        void Detach(CMLine line);
        void DetachAllLines();

        bool Contains(CMLine line);
        CMLine LineAt(int index);
        int LinesCount { get; }

    }
}
