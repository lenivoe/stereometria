using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using NSShapeModel;

namespace NSShapeView {
    public class CVShape : MonoBehaviour {
        public interface IData { }
        public class LineDividingData : IData {
            public LineDividingData(float ratio) { Ratio = ratio; }
            public float Ratio { get; private set; }
        }
        public class AngleSettingData : IData {
            public AngleSettingData(int angle) { AngleValue = angle; }
            public int AngleValue { get; private set; }
        }

        public static Vector3 CvtAxes(Vector3 vec) { return new Vector3(vec.y, vec.z, -vec.x); }
        public static Vector3 DecvtAxes(Vector3 vec) { return new Vector3(-vec.z, vec.x, vec.y); }


        // pointsPos - coordinates of the shape's points
        public CVShape Init(CMShape model, IEnumerable<Vector3> pointsPos) {
            if (this.model != null)
                throw new Exception("Second initialization of the shape view");


            pointsObj = new GameObject("points").transform;
            linesObj = new GameObject("lines").transform;
            anglesObj = new GameObject("angles").transform;
            pointsObj.parent = linesObj.parent = anglesObj.parent = transform;

            nextPointsPos = new Queue<Vector3>(pointsPos);
            pointsPos = null;

            this.model = model;
            model.PointCreated += Model_PointCreated;
            model.LineCreated += Model_LineCreated;
            
            return this;
        }
        

        public void Apply(IData data) {
            switch (CurState) {
                case State.CreatePoint:
                    if (curAction != null && curAction.IsDone) {
                        actionController.Apply(curAction);
                        var tranporter = (curAction as CCreatePointAction).CreatedPointView
                            .GetComponentInChildren<AxisTransporter>().gameObject;
                        GameObject.Destroy(tranporter);
                    }
                    break;
                case State.SetAngle:
                    if (activeSegments.Count == 2) {
                        int? angle = null;
                        if(data != null)
                            angle = (data as AngleSettingData).AngleValue;
                        var action = new CAngleSettingAction(this, activeSegments.Top, activeSegments.Bottom, angle);
                        actionController.Apply(action);
                    }
                    break;
                case State.BuildLine:
                    if(curAction != null && curAction.IsDone)
                        actionController.Apply(curAction);
                    break;
                case State.DivideLine:
                    if (activeSegments.Count == 1) {
                        var action = new CDivideLineAction(this, activeSegments.Top, (data as LineDividingData).Ratio);
                        actionController.Apply(action);
                    }
                    break;
            }
            ResetSelection();
            curAction = null;
        }
        public void Abort() {
            CancelCurAction();
            ResetSelection();
            CurState = State.Static;
        }
        public void CancelChanges() {
            CancelCurAction();
            ResetSelection();
            if (actionController.CanUndo)
                actionController.Undo();
        }
        public void RecoverChages() {
            CancelCurAction();
            ResetSelection();
            if (actionController.CanRedo)
                actionController.Redo();
        }

        // состояние редактирования фигуры пользователем
        public enum State { Static, CreatePoint, SetAngle, BuildLine, DivideLine }
        public State CurState {
            get { return curState; }
            set {
                curState = value;
                int maxPoints = 0;
                int maxSegments = 0;
                switch(curState) {
                    case State.CreatePoint:
                        curAction = new CCreatePointAction(this, CvtAxes(transform.localPosition));
                        curAction.Do();
                        AxisTransporter.Create().Cargo = (curAction as CCreatePointAction).CreatedPointView.transform;
                        break;
                    case State.SetAngle: maxSegments = 2; maxPoints = 0; break;
                    case State.BuildLine: maxSegments = 1; maxPoints = 2; break;
                    case State.DivideLine: maxSegments = 1; maxPoints = 0; break;
                }
                activePoints.Reset(maxPoints);
                activeSegments.Reset(maxSegments);
            }
        }




        // обработчик события создания точки модели
        private void Model_PointCreated(CMShape shape, CMPoint point) {
            // создание вида для очередной точки
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz123456789";
            string pointName = letters[pointViewDict.Count].ToString();
            CVPoint pointView = CVPoint.Create(pointName, point, pointsObj, nextPointsPos.Dequeue());
            pointView.MouseDown += PointView_MouseDown;
            pointViewDict.Add(point.Id, pointView);
        }
        // обработчик события создания прямой модели
        private void Model_LineCreated(CMShape shape, CMLine line) {
            // создание вида для очередной линии
            CVLine lineView = new CVLine(linesObj, line);
            lineView.Selected += LineView_Selected;
            for (int j = 0; j < line.PointsCount; j++)
                lineView.AddPoint(pointViewDict[line.PointAt(j).Id]);
            lineViewDict.Add(line.Id, lineView);
        }



        // стек выполненных действий с возможностью их отмены и восстановления
        private class CShapeActionController {
            // добавляет действие в стек и выполняет его, если это не было сделано
            public void Apply(AShapeAction action) {
                if (action == null)
                    return;
                if (actions.Count > 0)
                    while (actions.Last != pHead)
                        actions.RemoveLast();
                pHead = actions.AddLast(action);
                if (!action.IsDone)
                    action.Do();
            }
            // отменяет последнее действие
            public void Undo() {
                pHead.Value.Undo();
                if (pHead.Previous != null)
                    pHead = pHead.Previous;
            }
            // выполняет последнее отмененное действие
            public void Redo() {
                if (pHead.Value.IsDone)
                    pHead = pHead.Next;
                pHead.Value.Do();
            }

            public bool CanUndo { get { return pHead != null && (pHead.Previous != null || pHead.Value.IsDone); } }
            public bool CanRedo { get { return pHead != null && (pHead.Next != null || !pHead.Value.IsDone); } }


            private LinkedList<AShapeAction> actions = new LinkedList<AShapeAction>(); // стек действий
            private LinkedListNode<AShapeAction> pHead; // вершина стека действий
        }
        
        // представляет действие над фигурой, которое можно отменить
        private abstract class AShapeAction {
            public AShapeAction(CVShape owner) { Owner = owner; }

            public abstract void Do();
            public abstract void Undo();
            public abstract bool IsDone { get; }

            public CVShape Owner { get; private set; }


            protected class CVPointWrap {
                public CVPointWrap(CVShape owner, int pointId = -1) {
                    this.owner = owner;
                    Id = pointId;
                }
                public CVPointWrap(CVShape owner, CVPoint point) : this(owner, point.Model.Id) { }

                public CVPoint Value { get { return Id < 0 ? null : owner.pointViewDict[Id]; } }
                public int Id { get; private set; }    // id выбранной точки

                private CVShape owner;
            }
            protected class CVSegmentWrap {
                public CVSegmentWrap(CVShape owner, int lineId, int segmentInd) {
                    this.owner = owner;
                    Id = lineId;
                    Index = segmentInd;
                }
                public CVSegmentWrap(CVShape owner) : this(owner, -1, -1) { }
                public CVSegmentWrap(CVShape owner, CVLineSegment segment) 
                    : this(owner, segment.Model.Id, segment.IndexInLine) { }

                public CVLineSegment Value {
                    get { return Id < 0 ? null : owner.lineViewDict[Id].SegmentAt(Index); }
                }
                public int Id { get; private set; }     // id линии выбранного сегмента
                public int Index { get; private set; } // индекс выбранного сегмента

                private CVShape owner;
            }
        }

        // действие создания точки
        private class CCreatePointAction : AShapeAction {
            public CVPoint CreatedPointView { get; set; }

            public CCreatePointAction(CVShape owner, Vector3 pointPosition) : base(owner) {
                pointPos = pointPosition;
            }

            public override bool IsDone { get { return CreatedPointView != null; } }

            public override void Do() {
                if (IsDone)
                    throw new Exception("CCreatePointAction already was done (point id: " + CreatedPointView.Model.Id + ")");
                Owner.nextPointsPos.Enqueue(pointPos); // добавление позиции точки в очередь
                CMPoint point = Owner.model.CreatePoint(); // !! вид для точки создается в обработчике события CMShape.PointCreated
                CreatedPointView = Owner.pointViewDict[point.Id]; // получаем созданный вид точки
            }

            public override void Undo() {
                if (!IsDone)
                    throw new Exception("CCreatePointAction wasn't done yet (point position: " + pointPos + ")");

                Owner.activePoints.Remove(CreatedPointView); // удаление view точки из списка выделения

                CreatedPointView.Model.Owner.Remove(CreatedPointView.Model); // удаление модели точки
                Owner.pointViewDict.Remove(CreatedPointView.Model.Id); // удаление view точки из view модели
                GameObject.Destroy(CreatedPointView.gameObject); // удаление самого view точки
                CreatedPointView = null;
            }

            private Vector3 pointPos;
        }

        // действие деления отрезка
        private class CDivideLineAction : AShapeAction {
            public CDivideLineAction(CVShape owner, CVLine line, int segmentIndex, float ratio) 
                : this(owner, line, segmentIndex, Vector3.zero) { this.ratio = ratio; }

            public CDivideLineAction(CVShape owner, CVLineSegment segment, float ratio)
                : this(owner, segment.Owner, segment.IndexInLine, ratio) { }

            public CDivideLineAction(CVShape owner, CVLine line, int segmentIndex, Vector3 pointPosition) : base(owner) {
                pointAction = new CCreatePointAction(owner, pointPosition);
                segment = new CVSegmentWrap(owner, line.Model.Id, segmentIndex);
                ratio = 0;
            }

            public CDivideLineAction(CVShape owner, CVLineSegment segment, Vector3 pointPosition)
                : this(owner, segment.Owner, segment.IndexInLine, pointPosition) { }

            public override void Do() {
                if (IsDone)
                    throw new Exception("CDivideLineAction already was done (line id: "
                        + segment.Id + ", point id: " + pointAction.CreatedPointView.Model.Id + ")");
                pointAction.Do();
                if (ratio != 0)
                    segment.Value.Owner.InsertPoint(pointAction.CreatedPointView, segment.Index, ratio);
                else
                    segment.Value.Owner.InsertPoint(pointAction.CreatedPointView, segment.Index);
            }

            public override void Undo() {
                if (!IsDone)
                    throw new Exception("CDivideLineAction wasn't done yet (line id: " + segment.Id + ")");
                segment.Value.Owner.RemovePoint(pointAction.CreatedPointView);
                pointAction.Undo();
            }

            public override bool IsDone { get { return pointAction.IsDone; } }


            private CCreatePointAction pointAction;
            private CVSegmentWrap segment;
            private float ratio;
        }
        
        // действие построения линии
        private class CBuildLineAction : AShapeAction {
            public CBuildLineAction(CVShape owner, CVPoint point1, CVPoint point2) : base(owner) {
                this.point1 = new CVPointWrap(owner, point1);
                this.point2 = new CVPointWrap(owner, point2);
            }
            public CBuildLineAction(CVShape owner, CVPoint point, CVLineSegment segment, Vector3 hitPos) : base(owner) {
                point1 = new CVPointWrap(owner, point);
                this.segment = new CVSegmentWrap(owner, segment);
                this.hitPos = segment.Owner.CalcHitPosition(this.segment.Index, hitPos);
                divideAction = new CDivideLineAction(Owner, segment.Owner, this.segment.Index, this.hitPos);
            }

            public override void Do() {
                if (IsDone)
                    throw new Exception("CBuildLineAction already was done (new line id: " + createdLineView.Model.Id + ")");

                bool isAnjoinedLine = IsBuildToLine
                    ? point1.Value.Model.Contains(segment.Value.Owner.Model)
                    : point1.Value.Model.GetCommonLine(point2.Value.Model) != null;
                if (isAnjoinedLine) // выйти, если новая линия будет совпадать с какой-либо существующей
                    return;

                // если были выбраны точка и отрезок, создать вторую точку
                if (IsBuildToLine) {
                    divideAction.Do();
                    var newPoint = segment.Value.Owner.PointAt(segment.Value.IndexInLine + 1);
                    point2 = new CVPointWrap(Owner, newPoint);
                }

                CMLine line = Owner.model.CreateLine(point1.Id, point2.Id);
                var lineView = Owner.lineViewDict[line.Id];
                createdLineView = lineView;
            }
            public override void Undo() {
                if (!IsDone)
                    throw new Exception("CBuildLineAction already was undone");

                Owner.lineViewDict.Remove(createdLineView.Model.Id);
                createdLineView.Dispose();
                if (divideAction != null)
                    divideAction.Undo();
                createdLineView = null;
            }
            public override bool IsDone { get { return createdLineView != null; } }


            private bool IsBuildToLine { get { return divideAction != null; } }
            
            private Vector3 hitPos;
            private CVLine createdLineView;
            private CDivideLineAction divideAction;

            private CVPointWrap point1, point2;
            private CVSegmentWrap segment;
        }
        
        // действия установки угла
        private class CAngleSettingAction : AShapeAction {
            public CAngleSettingAction(CVShape owner, CVLineSegment segment1, CVLineSegment segment2, int? angle = null) : base(owner) {
                this.segment1 = new CVSegmentWrap(Owner, segment1);
                this.segment2 = new CVSegmentWrap(Owner, segment2);
                this.angle = angle;
            }

            public override bool IsDone { get { return createdAngle != null; } }
            public override void Do() {
                CMAngle model = Owner.model.CreateAngle(segment1.Value.Model, segment2.Value.Model, angle);
                createdAngle = CVAngle.Create(model, Owner.anglesObj, segment1.Value, segment2.Value);
            }
            public override void Undo() {
                GameObject.Destroy(createdAngle.gameObject);
                createdAngle = null;
            }


            private CVSegmentWrap segment1, segment2;
            private int? angle;
            private CVAngle createdAngle;
        }

        // список выделенных элементов
        private class CActiveViewList<T> : IEnumerable<T> where T : AVPrimitive {
            public CActiveViewList(int maxCount) { MaxCount = maxCount; }

            // добавляет, либо исключает примитив
            public void ToXor(T primitive) {
                var node = list.Find(primitive);
                if (node == null) {
                    Add(primitive);
                } else {
                    Remove(node);
                }
            }
            public void Remove(T primitive) {
                var node = list.Find(primitive);
                if (node != null)
                    Remove(node);
            }

            public void Clear() {
                foreach (var primitive in list)
                    primitive.IsFocused = false;
                list.Clear();
            }
            public void Reset(int newMaxCount) {
                Clear();
                MaxCount = newMaxCount;
            }

            public T Top { get { return list.First == null ? null : list.First.Value; } }
            public T Bottom { get { return list.Last == null ? null : list.Last.Value; } }

            public int MaxCount { get; private set; }
            public int Count { get { return list.Count; } }

            public IEnumerator<T> GetEnumerator() { return list.GetEnumerator(); }


            IEnumerator IEnumerable.GetEnumerator() { return list.GetEnumerator(); }

            private void Add(T primitive) {
                if (Count > MaxCount)
                    throw new Exception(string.Format(
                        "CPrimitiveViewStack overflow. (count: {0}, max count: {1})", Count, MaxCount));

                primitive.IsFocused = true;
                list.AddFirst(primitive);

                if (Count > MaxCount)
                    Remove(list.Last);
            }
            private void Remove(LinkedListNode<T> node) {
                node.Value.IsFocused = false;
                list.Remove(node);
            }
            
            private LinkedList<T> list = new LinkedList<T>();
        }
        

        // сбросить выделение со всех выбранных объектов
        private void ResetSelection() {
            activePoints.Clear();
            activeSegments.Clear();
        }
        private void CancelCurAction() {
            if (curAction != null && curAction.IsDone)
                curAction.Undo();
            curAction = null;
        }


        // обработчик события выбора отрезка
        private void LineView_Selected(CVLine view, CVLineSegment segment) {
            StartCoroutine(LineView_Selected_Coroutine(view, segment));
        }
        // отложенная обработка события выбора отрезка
        private IEnumerator LineView_Selected_Coroutine(CVLine view, CVLineSegment segment) {
            switch (CurState) {
                case State.SetAngle:
                    if (activeSegments.Count != 0 && segment.GetCommonPoint(activeSegments.Top) == null)
                        activeSegments.Clear();
                    activeSegments.ToXor(segment);
                    break;
                case State.BuildLine:
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (curAction != null && curAction.IsDone) {
                        curAction.Undo();
                        yield return null;
                    }
                    RaycastHit raycastHit = new RaycastHit();
                    if (activePoints.Top != null && Physics.Raycast(ray, out raycastHit)) {
                        CVLineSegment correctSegment = raycastHit.transform.gameObject.GetComponent<CVLineSegment>();
                        curAction = new CBuildLineAction(this, activePoints.Top, correctSegment, raycastHit.point);
                        curAction.Do();
                    }
                    break;
                case State.DivideLine:
                    activeSegments.ToXor(segment);
                    break;
                case State.Static:
                    break;
            }
            yield return null;
        }
        
        // обработчик собития нажатия по точке
        private void PointView_MouseDown(AVPrimitive primitive) {
            StartCoroutine(PointView_MouseDown_Coroutine((CVPoint)primitive));
        }
        // отложенная обработка нажатия по точке
        private IEnumerator PointView_MouseDown_Coroutine(CVPoint point) {
            switch (CurState) {
                case State.BuildLine:
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (curAction != null && curAction.IsDone) {
                        curAction.Undo();
                        yield return null;
                    }
                    
                    RaycastHit raycastHit = new RaycastHit();
                    if (Physics.Raycast(ray, out raycastHit)) {
                        CVPoint correctPoint = raycastHit.transform.gameObject.GetComponent<CVPoint>();
                        if (correctPoint != null) {
                            activePoints.ToXor(correctPoint);
                            if (activePoints.Count == activePoints.MaxCount) {
                                curAction = new CBuildLineAction(this, activePoints.Bottom, correctPoint);
                                curAction.Do();
                                activePoints.Remove(correctPoint);
                            }
                        }
                    }
                    
                    break;
            }
            
            yield return null;
        }

        // объекты родители для всех представлений точек и линий
        private Transform pointsObj;
        private Transform linesObj;
        private Transform anglesObj;

        // (логическая) модель трехмерной фигуры
        private CMShape model;

        private State curState = State.Static;
        
        // связь между id моделей и их представлением
        private Dictionary<int, CVPoint> pointViewDict = new Dictionary<int, CVPoint>();
        private Dictionary<int, CVLine> lineViewDict = new Dictionary<int, CVLine>();

        private CShapeActionController actionController = new CShapeActionController();
        private AShapeAction curAction;

        private CActiveViewList<CVLineSegment> activeSegments = new CActiveViewList<CVLineSegment>(2);
        private CActiveViewList<CVPoint> activePoints = new CActiveViewList<CVPoint>(2);

        private Queue<Vector3> nextPointsPos = null; // очередь позиций новых view для точек
    }
}
