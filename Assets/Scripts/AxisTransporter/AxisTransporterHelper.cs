using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AxisTransporterHelper : MonoBehaviour {
    public event Action<AxisTransporterHelper> MouseDown;
    public event Action<AxisTransporterHelper> MouseUp;

    private void OnMouseDown() {
        if (MouseDown != null) MouseDown(this);
    }
    private void OnMouseUp() {
        if (MouseUp != null) MouseUp(this);
    }
}
