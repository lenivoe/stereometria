using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NSShapeModel {
    public abstract class AMRelation : AModel {
        public AMRelation(CMShape owner, int id) : base(owner, id) { }
        
        public abstract bool IsCorrect { get; }
        public abstract int Count { get; }
        public abstract bool Contains(AMPrimitive model);
        public abstract AMPrimitive PrimitiveAt(int index);

        #region IDisposable Support
        protected override void Dispose(bool disposing) {
            if (isDisposed) return;
            if (disposing) {
                if (Owner != null && Owner.Contains(this))
                    Owner.Remove(this);
            }
            isDisposed = true;
        }
        private bool isDisposed = false; // Для определения избыточных вызовов
        #endregion
    }
}
