using System;
using System.Collections.Generic;
using UnityEngine;
using NSShapeModel;

namespace NSShapeView {
    public class CVPoint : AVCarcassElements {
        protected override void Awake() {
            base.Awake();
            BaseMaterial.color = new Color(0.4f, 0.4f, 0.75f);
        }


        public static CVPoint Create(string name, CMPoint pointModel, Transform parent, Vector3 localPos) {
            return GameObject.CreatePrimitive(PrimitiveType.Sphere)
                .AddComponent<CVPoint>()
                .Init(name, pointModel, parent, localPos);
        }

        public Vector3 RealPosition {
            get { return transform.localPosition; }
            set { transform.localPosition = value; }
        }
        public Vector3 Position {
            get { return CVShape.DecvtAxes(RealPosition); }
            set { RealPosition = CVShape.CvtAxes(value); }
        }
        
        public CMPoint Model { get; private set; }

        public string Name { get { return label.Text; } }
        
        private CVPoint Init(string name, CMPoint pointModel, Transform parent, Vector3 localPos) {
            Model = pointModel;

            transform.parent = parent;
            transform.localScale = Vector3.one * 0.12f;
            transform.localRotation = Quaternion.identity;
            transform.GetComponent<SphereCollider>().radius = 1;
            Position = localPos;
            
            label = CVLabel.Create(transform, name);

            this.name = "point " + Name;
            return this;
        }

        
        private CVLabel CreateLabel(string text) {
            var labelObj = new GameObject();
            labelObj.transform.parent = transform;
            var label = labelObj.AddComponent<CVLabel>();
            label.Text = text;

            return label;
        }

        private void OnDestroy() { if (Model != null) Model.Dispose(); }


        private CVLabel label;
    }
}
