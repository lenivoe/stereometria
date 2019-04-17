using System;
using System.Collections.Generic;
using UnityEngine;
using NSShapeModel;

namespace NSShapeView {
    public class CVLabel : MonoBehaviour {
        public static CVLabel Create(Transform parent, string text) {
            var labelObj = new GameObject();
            labelObj.transform.parent = parent;
            var label = labelObj.AddComponent<CVLabel>();
            label.Text = text;

            return label;
        }

        public string Text {
            get { return textMesh.text; }
            set {
                textMesh.text = value;
                name = "label " + value;
                for (int i = textMesh.transform.childCount; i-- > 0;) // помнишь, что так работают с uint? я помню
                    textMesh.transform.GetChild(i).GetComponent<TextMesh>().text = value;
            }
        }


        private void Awake() {
            name = "label";

            // Trasform init
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            // init text game object
            var textObj = new GameObject("text");
            textObj.transform.parent = transform;
            textObj.transform.localPosition = Vector3.zero;
            textObj.transform.localRotation = Quaternion.identity;
            textObj.transform.localScale = Vector3.one;

            // TextRenderer init
            var renderer = textObj.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            // TextMesh init
            this.textMesh = textObj.AddComponent<TextMesh>();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.color = mainColor;
            textMesh.fontSize = 20;
            textMesh.characterSize = 0.5f;

            // create text outline
            const int partsCount = 8;
            const float offset = 0.04f;
            // first outline part
            TextMesh firstOutline = Instantiate(textObj, textObj.transform).GetComponent<TextMesh>();
            firstOutline.name = "outline " + 0;
            firstOutline.color = outlineColor;
            firstOutline.transform.localPosition = GetOffset(0, offset);
            // other outline parts
            for (int i = 1; i < partsCount; i++) {
                TextMesh outline = Instantiate(firstOutline, textObj.transform);
                outline.name = "outline " + i;
                outline.transform.localPosition = GetOffset(i, offset);
            }
        }
        
        private void Start() { cam = Camera.main; }

        private void Update() {
            // поворот текста в камеру
            Vector3 forward = transform.position - cam.transform.position;
            transform.rotation = Quaternion.LookRotation(forward, cam.transform.up);
        }

        private static Vector3 GetOffset(int i, float offset) {
            float[] vecX = new float[] { 0, 1, 1, 1, 0, -1, -1, -1 };
            float[] vecY = new float[] { 1, 1, 0, -1, -1, -1, 0, 1 };

            return new Vector3(vecX[i], vecY[i], 1) * offset;
        }


        private readonly Color mainColor = Color.white;
        private readonly Color outlineColor = new Color(0.25f, 0.25f, 0); // темножелтый

        private TextMesh textMesh;
        private Camera cam;
    }
}
