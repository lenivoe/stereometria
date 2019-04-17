using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NSShapeView {
    public abstract class AVCarcassElements : AVPrimitive {
        protected override void Awake() {
            base.Awake();

            BaseMaterial.color = Color.black;
            HoverMaterial.color = new Color(0.8f, 0.4f, 0, 1);
            FocuseHoverMaterial.color = 
                FocuseMaterial.color = new Color(1, 0.6f, 0.2f, 1);

            IsAutoFocuse = false;
            IsAutoHover = true;
        }
    }
}
