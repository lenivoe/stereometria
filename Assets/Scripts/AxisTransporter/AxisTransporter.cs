using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// скрипт для передвижения выбранного объекта посредством перетаскивания за координатные оси
public class AxisTransporter : MonoBehaviour {
    // создание экземпляра объекта Unity на основе шаблона
    public static AxisTransporter Create() {
        return Instantiate(Template).GetComponent<AxisTransporter>();
    }
    
    // переносимый объект
    public Transform Cargo {
        get { return cargo; }
        set {
            cargo = value;
            transform.parent = cargo;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
    private Transform cargo;
    
    // шаблон объекта с данным класом
    private static GameObject Template {
        get {
            if (template == null)
                template = Resources.Load<GameObject>("Transporter");
            return template;
        }
    }
    private static GameObject template = null;

    private const float sensitivity = 0.0001f;
    private Vector3 startCargoPos; // начальная позиция объекта
    private Vector3 axisDir; // вектор выбранного направления перемещения
    private Vector3 mouseDownPos; // позиция курсора во время нажатия кнопки мыши
    private bool isDraging = false; // флаг перетаскивания, true - если объект перетаскивается


    // инициализация обработчиков событий мыши для осей
    private void Awake() {
        for(int i = 0; i < transform.childCount; i++) {
            var axis = transform.GetChild(i).GetComponent<AxisTransporterHelper>();
            axis.MouseDown += Helper_MouseDown;
            axis.MouseUp += Helper_MouseUp;
        }
    }

    // итерация цикла передвижения, выполняется каждый кадр
    private void Update() {
        if(isDraging && MouseController.Inst.IsUsurper(this)) {
            var shift = Input.mousePosition - mouseDownPos;
            var cam = Camera.main.transform;
            var up = cam.up.normalized * shift.y;
            var right = Vector3.Cross(cam.up, cam.forward).normalized * shift.x;
            var mouseDir = up + right;
            
            if (mouseDir.magnitude < 0.0001f)
                return;
            var pathLen = Vector3.Dot(mouseDir, axisDir) * mouseDir.magnitude * sensitivity;
            transform.position = startCargoPos + axisDir * pathLen;
            transform.parent.position = transform.position;
        }
    }

    // нажатие кнопки мыши по оси, начало перетаскивания объекта
    private void Helper_MouseDown(AxisTransporterHelper axis) {
        if (MouseController.Inst.CaptureControl(this)) {
            startCargoPos = cargo.position;
            mouseDownPos = Input.mousePosition;
            axisDir = axis.transform.up.normalized;
            isDraging = true;
        }
    }

    // отпускание кнопки мыши над осью, конец перетаскивания объекта
    private void Helper_MouseUp(AxisTransporterHelper axis) {
        if (MouseController.Inst.CaptureControl(this)) {
            isDraging = false;
            MouseController.Inst.ReleaseControl(this);
        }
    }
}
