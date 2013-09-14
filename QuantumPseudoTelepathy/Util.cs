using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public static class Util {
    public static double Abs(this double value) {
        return Math.Abs(value);
    }

    public static Complex Sum(this IEnumerable<Complex> values) {
        return values.Aggregate(Complex.Zero, (a, e) => a + e);
    }

    public static IEnumerable<IReadOnlyList<T>> ChooseWithReplacement<T>(this IReadOnlyList<T> items, int numberOfItemsToDraw) {
        if (items == null) throw new ArgumentNullException("items");
        if (numberOfItemsToDraw < 0) throw new ArgumentOutOfRangeException("numberOfItemsToDraw", "numberOfItemsToDraw < 0");
        if (numberOfItemsToDraw == 0) return new[] {new T[0]};

        return from tail in items.ChooseWithReplacement(numberOfItemsToDraw - 1)
               from head in items
               select new[] {head}.Concat(tail).ToArray();
    }
    public static string StringJoin<T>(this IEnumerable<T> items, string separator) {
        if (items == null) throw new ArgumentNullException("items");
        return string.Join(separator, items);
    }
    public static string ToPrettyString(this Complex c, string f = null) {
        f = f ?? "0.###";
        var vr = c.Real;
        var vi = c.Imaginary;
        if (Math.Abs(vi) < 0.0001) return vr.ToString(f);
        if (Math.Abs(vr) < 0.0001)
            return vi == 1 ? "i"
                 : vi == -1 ? "-i"
                 : vi.ToString(f) + "i";
        return String.Format(
            "{0}{1}{2}",
            vr == 0 ? "" : vr.ToString(f),
            vi < 0 ? "-" : "+",
            vi == 1 || vi == -1 ? "i" : Math.Abs(vi).ToString(f) + "i");
    }
    public static string ToMagPhaseString(this Complex c, string af = null, string pf = null) {
        return string.Format("√{0} ⋅ ∠{1}°", (c.Magnitude * c.Magnitude).ToString(af ?? "0.##"), (c.Phase * 180 / Math.PI).ToString(pf ?? "0.#"));
    }
    public static KeyValuePair<TKey, TVal> KeyVal<TKey, TVal>(this TKey key, TVal val) {
        return new KeyValuePair<TKey, TVal>(key, val);
    } 
}
