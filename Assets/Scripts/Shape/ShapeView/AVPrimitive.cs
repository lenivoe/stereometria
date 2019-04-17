using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NSShapeView {
    public abstract class AVPrimitive : MonoBehaviour {
        public bool IsFocused {
            get { return isFocused; }
            set {
                isFocused = value;
                SetCurMaterial();
            }
        }
        public bool IsHovered {
            get { return isHovered; }
            set {
                isHovered = value;
                SetCurMaterial();
            }
        }
        public bool IsAutoFocuse { get; set; }
        public bool IsAutoHover { get; set; }

        public event Action<AVPrimitive> MouseDown;


        protected virtual void Awake() { ActualRenderer.material = BaseMaterial; }

        protected virtual void OnMouseDown() {
            if (IsAutoFocuse)
                IsFocused = !IsFocused;
            if (MouseDown != null)
                MouseDown(this);
        }
        protected virtual void OnMouseEnter() {
            if (IsAutoHover)
                IsHovered = true;
        }
        protected virtual void OnMouseExit() {
            if (IsAutoHover)
                IsHovered = false;
        }

        protected virtual Material BaseMaterial {
            get {
                if (baseMaterial == null)
                    baseMaterial = CreateMaterial();
                return baseMaterial;
            }
        }
        protected virtual Material HoverMaterial {
            get {
                if (hoverMaterial == null)
                    hoverMaterial = CreateMaterial();
                return hoverMaterial;
            }
        }        
        protected virtual Material FocuseMaterial {
            get {
                if (focuseMaterial == null)
                    focuseMaterial = CreateMaterial();
                return focuseMaterial;
            }
        }
        protected virtual Material FocuseHoverMaterial {
            get {
                if (focuseHoverMaterial == null)
                    focuseHoverMaterial = CreateMaterial();
                return focuseHoverMaterial;
            }
        }

        protected Renderer ActualRenderer {
            get {
                if (actualRenderer == null)
                    actualRenderer = GetComponent<Renderer>();
                return actualRenderer;
            }
        }


        private Material CreateMaterial() {
            var material = new Material(Shader.Find("Legacy Shaders/Diffuse"));
            material.color = new Color(1, 0, 1, 1); // pink
            return material;
        }
        private void SetCurMaterial() {
            if (isFocused)
                ActualRenderer.material = isHovered ? FocuseHoverMaterial : FocuseMaterial;
            else
                ActualRenderer.material = isHovered ? HoverMaterial : BaseMaterial;
        }

        private bool isFocused = false;
        private bool isHovered = false;
        private Material baseMaterial = null;
        private Material hoverMaterial = null;
        private Material focuseMaterial = null;
        private Material focuseHoverMaterial = null;
        private Renderer actualRenderer = null;

    }
}
