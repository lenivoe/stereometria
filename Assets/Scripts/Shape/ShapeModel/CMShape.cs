using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace NSShapeModel {
    public class CMShape {
        public event Action<CMShape, CMPoint>   PointCreated;
        public event Action<CMShape, CMLine>    LineCreated;
        public event Action<CMShape, CMPlane>   PlaneCreated;
        public event Action<CMShape, CMAngle>   AngleCreated;


        public void Init(int pointsCount, int[][] pointsConnections, int[][] linesConnections) {
            int capacity = pointsCount + pointsConnections.Length + linesConnections.Length;
            primitives = new Dictionary<int, AMPrimitive>(capacity);
            // создание точек
            for (int i = 0; i < pointsCount; i++)
                CreatePoint();
            // создание линий
            foreach (int[] pointsId in pointsConnections)
                CreateLine(pointsId);
            // создание плоскостей (индексы линий преобразуются в их id)
            foreach (int[] linesInds in linesConnections)
                CreatePlane(linesInds.Select(x => x + pointsCount));
        }


        public bool Contains(AMPrimitive primitive) {
            return primitive.Owner == this && primitives.ContainsKey(primitive.Id);
        }
        public bool Contains(AMRelation relation) {
            return relation.Owner == this && relations.ContainsKey(relation.Id);
        }

        public AMPrimitive PrimitiveAt(int id) { return primitives[id]; }
        public AMPrimitive this[int id] { get { return PrimitiveAt(id); } }

        public void Remove(AMPrimitive primitive) {
            primitives.Remove(primitive.Id);
            primitive.Dispose();
            if (primitive.Id + 1 == nextId) nextId--;
        }
        public void Remove(AMRelation relation) {
            relations.Remove(relation.Id);
            relation.Dispose();
            if (relation.Id + 1 == nextId) nextId--;
        }

        public CMPoint CreatePoint() { return CreatePoint(nextId++); }
        public CMLine CreateLine(IEnumerable<int> pointsId) { return CreateLine(nextId++, pointsId); }
        public CMLine CreateLine(params int[] pointsId) { return CreateLine((IEnumerable<int>)pointsId); }
        public CMPlane CreatePlane(IEnumerable<int> pointsId) { return CreatePlane(nextId++, pointsId); }
        public CMPlane CreatePlane(params int[] pointsId) { return CreatePlane(pointsId); }

        
        public CMAngle CreateAngle(CMLine line1, CMLine line2, int? angle = null) {
            return CreateAngle(nextId++, line1, line2, angle);
        }


        
        private CMPoint CreatePoint(int id) {
            CMPoint point = new CMPoint(this, id);
            primitives.Add(point.Id, point);
            if (PointCreated != null) PointCreated(this, point);
            return point;
        }
        private CMLine CreateLine(int id, IEnumerable<int> pointsId) {
            CMLine line = new CMLine(this, id, pointsId);
            primitives.Add(line.Id, line);
            if (LineCreated != null) LineCreated(this, line);
            return line;
        }
        private CMPlane CreatePlane(int id, IEnumerable<int> pointsId) {
            var model = new CMPlane(this, id, pointsId);
            primitives.Add(model.Id, model);
            if (PlaneCreated != null) PlaneCreated(this, model);
            return model;
        }
        private CMAngle CreateAngle(int id, CMLine line1, CMLine line2, int? angle = null) {
            var model = new CMAngle(id, line1, line2, angle);
            relations.Add(model.Id, model);
            if (AngleCreated != null) AngleCreated(this, model);
            return model;
        }
        
        
        private int nextId = 0; // счетчик, хранящий следующий id
        
        private Dictionary<int, AMPrimitive> primitives = null;
        private Dictionary<int, AMRelation> relations = new Dictionary<int, AMRelation>();

    }
}
