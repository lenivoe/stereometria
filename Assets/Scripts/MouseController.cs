using UnityEngine;
using System.Collections;
using System;

// содержит информацию о том, кто владеет контролем над мышью и можно ли его получить
public class MouseController {
    // глобальный экземпляр класса
    public static MouseController Inst { get { return inst; } }
    private static MouseController inst = new MouseController();

    // захватчик контроля
    private object usurper = null;

    // проверка, имеет ли аргумент контроль
    public bool IsUsurper(object value) { return value == usurper; }

    // попытка получить контроль на мышью, возвращает true, если удалось
    public bool CaptureControl(object usurper) {
        if (this.usurper == null)
            this.usurper = usurper;
        return this.usurper == usurper;
    }

    // возвращает контроль захватчиком, если аргумент не захватчик, выдает исключение
    public void ReleaseControl(object usurper) {
        if (this.usurper != usurper)
            throw new ArgumentException("Only owner can release control");
        this.usurper = null;
    }
}
