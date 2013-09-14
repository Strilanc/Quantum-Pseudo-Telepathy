using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using Strilanc.LinqToCollections;

///<summary>A list of complex numbers.</summary>
[DebuggerDisplay("{ToString()}")]
public struct ComplexVector {
    private readonly Complex[] _values;
    public IReadOnlyList<Complex> Values { get { return _values ?? ReadOnlyList.Empty<Complex>(); }}

    public ComplexVector(IEnumerable<Complex> values) {
        if (values == null) throw new ArgumentNullException("values");
        this._values = values.ToArray();
    }

    public bool IsPhased(ComplexVector other) {
        var vals = Values;
        var c = vals.Count.Range().FirstOrDefault(i => vals[i] != 0);
        if (vals[c] == 0) return other == this;
        return this * (other.Values[c] / vals[c]) == other;
    }
    public static ComplexVector operator *(ComplexVector vector, Complex scalar) {
        return scalar*vector;
    }
    public ComplexVector TensorProduct(ComplexVector other) {
        return new ComplexVector(from v1 in this.Values
                                 from v2 in other.Values
                                 select v1*v2);
    }
    public static ComplexVector operator *(Complex scalar, ComplexVector vector) {
        return new ComplexVector(vector.Values.Select(e => e*scalar));
    }
    public static Complex operator *(ComplexVector vector1, ComplexVector vector2) {
        return vector1.Values.Zip(vector2.Values, (e1, e2) => e1 * e2).Sum();
    }
    public static ComplexVector operator +(ComplexVector vector1, ComplexVector vector2) {
        return new ComplexVector(vector1.Values.Zip(vector2.Values, (e1, e2) => e1 + e2));
    }
    public static bool operator ==(ComplexVector v1, ComplexVector v2) {
        return v1.Values.Count == v2.Values.Count 
            && v1.Values.Zip(v2.Values, (e1, e2) => (e1 - e2).Magnitude < 0.00001)
                        .All(e => e);
    }
    public static bool operator !=(ComplexVector v1, ComplexVector v2) {
        return !(v1 == v2);
    }
    public override bool Equals(object obj) {
        return obj is ComplexVector && (ComplexVector)obj == this;
    }
    public override int GetHashCode() {
        return Values.Aggregate(Values.Count.GetHashCode(), (a, e) => a * 3 + Math.Round(e.Real * 1000).GetHashCode() * 5 + Math.Round(e.Imaginary * 1000).GetHashCode());
    }
    public override string ToString() {
        var b = (int)Math.Round(Math.Log(_values.Length, 2));
        if (_values.Length == 1 << b && b > 2 && _values.Count(e => e != 0) < 20) {
            return string.Join(" + ", _values.Zip(new[] { 0, 1 }.ChooseWithReplacement(b), (v, i) => v == 0 ? "" : v.ToPrettyString() + "|" + string.Join("", i.Reverse()) + ">").Where(e => e != ""));
        }
        return String.Format("<{0}>", Values.Select(e => e.ToPrettyString()).StringJoin(", "));
    }
}
