using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using Strilanc.LinqToCollections;

///<summary>A list of complex numbers.</summary>
[DebuggerDisplay("{ToString()}")]
public struct ComplexVector {
    private readonly IReadOnlyList<Complex> _values;
    public IReadOnlyList<Complex> Values { get { return _values ?? ReadOnlyList.Empty<Complex>(); }}

    public ComplexVector(IEnumerable<Complex> values) {
        if (values == null) throw new ArgumentNullException("values");
        this._values = values.ToArray();
    }

    public static ComplexVector operator *(ComplexVector vector, Complex scalar) {
        return scalar*vector;
    }
    public static ComplexVector operator *(Complex scalar, ComplexVector vector) {
        return new ComplexVector(vector.Values.Select(e => e*scalar).ToArray());
    }
    public static Complex operator *(ComplexVector vector1, ComplexVector vector2) {
        return vector1.Values.Zip(vector2.Values, (e1, e2) => e1 * e2).Sum();
    }
    public static ComplexVector operator +(ComplexVector vector1, ComplexVector vector2) {
        return new ComplexVector(vector1.Values.Zip(vector2.Values, (e1, e2) => e1 + e2).ToArray());
    }
    public override string ToString() {
        var b = (int)Math.Round(Math.Log(_values.Count, 2));
        if (_values.Count == 1 << b && b > 2 && _values.Count(e => e != 0) < 20) {
            return string.Join(" + ", _values.Zip(new[] { 0, 1 }.ChooseWithReplacement(b), (v, i) => v == 0 ? "" : v.ToPrettyString() + "|" + string.Join("", i.Reverse()) + ">").Where(e => e != ""));
        }
        return String.Format("<{0}>", Values.Select(e => e.ToPrettyString()).StringJoin(", "));
    }
}
