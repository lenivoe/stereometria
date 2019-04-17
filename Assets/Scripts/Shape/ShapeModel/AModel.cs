using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NSShapeModel {
    public abstract class AModel : IEquatable<AModel>, IDisposable {
        public AModel(CMShape owner, int id) {
            Owner = owner;
            Id = id;
        }
        public CMShape Owner { get; private set; }
        public int Id { get; private set; }
        
        #region IEquatable<AModel> Support
        public override bool Equals(object obj) { return Equals(obj as AModel); }
        public bool Equals(AModel other) {
            return other != null &&
                   EqualityComparer<CMShape>.Default.Equals(Owner, other.Owner) &&
                   Id == other.Id;
        }
        public override int GetHashCode() {
            var hashCode = -1724269164;
            hashCode = hashCode * -1521134295 + EqualityComparer<CMShape>.Default.GetHashCode(Owner);
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(AModel model1, AModel model2) {
            return ReferenceEquals(model1, null) && ReferenceEquals(model2, null)
                || !ReferenceEquals(model1, null) && !ReferenceEquals(model2, null)
                && model1.Id == model2.Id && model1.Owner == model2.Owner;
        }
        public static bool operator !=(AModel model1, AModel model2) { return !(model1 == model2); }
        #endregion
        
        #region IDisposable Support
        public void Dispose() { Dispose(true); /*GC.SuppressFinalize(this);*/ }
        protected abstract void Dispose(bool disposing);
        #endregion
    }
}
