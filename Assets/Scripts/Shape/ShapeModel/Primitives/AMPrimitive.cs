using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NSShapeModel {
    public abstract class AMPrimitive : AModel {
        public AMPrimitive(CMShape owner, int id) : base(owner, id) { }
        
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
