using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public static class Util {
    private static readonly Complex i = Complex.ImaginaryOne;
    public static readonly ComplexMatrix I = ComplexMatrix.MakeIdentity(2);
    public static readonly ComplexMatrix H = ComplexMatrix.MakeUnitaryHadamard(1);
    public static readonly ComplexMatrix X = ComplexMatrix.FromCellData(0, 1, 1, 0);
    public static readonly ComplexMatrix Y = ComplexMatrix.FromCellData(0, -i, i, 0);
    public static readonly ComplexMatrix Z = ComplexMatrix.FromCellData(1, 0, 0, -1);
    public static readonly ComplexMatrix Phase = ComplexMatrix.FromCellData(1, 0, 0, i);
    public static readonly ComplexMatrix SqrtNot = ComplexMatrix.FromCellData(1, -1, 1, 1) / Math.Sqrt(2);
    public static readonly ComplexMatrix BeamSplit = ComplexMatrix.FromCellData(1, i, i, 1) / Math.Sqrt(2);

    public static readonly ComplexMatrix ControlledNot = ComplexMatrix.FromCellData(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 0, 1,
        0, 0, 1, 0);
    public static readonly ComplexMatrix ControlledNot2 = ComplexMatrix.FromCellData(
        0, 1, 0, 0,
        1, 0, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1);
    public static readonly ComplexMatrix Swap = ComplexMatrix.FromCellData(
        1, 0, 0, 0,
        0, 0, 1, 0,
        0, 1, 0, 0,
        0, 0, 0, 1);
    public static readonly ComplexMatrix PlusOne = ComplexMatrix.FromCellData(
        0, 0, 0, 1,
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0);

    public static readonly ComplexMatrix Phase00 = ComplexMatrix.FromCellData(
        i, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1);
    public static readonly ComplexMatrix Phase01 = ComplexMatrix.FromCellData(
        1, 0, 0, 0,
        0, i, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1);
    public static readonly ComplexMatrix Phase10 = ComplexMatrix.FromCellData(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, i, 0,
        0, 0, 0, 1);
    public static readonly ComplexMatrix Phase11 = ComplexMatrix.FromCellData(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, i);
    public static readonly ComplexMatrix Flip00 = Phase00 * Phase00;
    public static readonly ComplexMatrix Flip01 = Phase01 * Phase01;
    public static readonly ComplexMatrix Flip10 = Phase10 * Phase10;
    public static readonly ComplexMatrix Flip11 = Phase11 * Phase11;

    public static readonly ComplexMatrix Alice1 = PlusOne
                                                * Swap
                                                * BeamSplit.TensorProduct(I)
                                                * ControlledNot
                                                * Phase01.Dagger();
                                              
    public static readonly ComplexMatrix Alice2 = Phase00
                                                * Phase11
                                                * I.TensorProduct(X)
                                                * H.TensorSquare();
                                              
    public static readonly ComplexMatrix Alice3 = Z.TensorProduct(H)
                                                * PlusOne
                                                * I.TensorProduct(SqrtNot);

    public static readonly ComplexMatrix Bob1 = BeamSplit.TensorProduct(Phase)
                                              * ControlledNot2
                                              * SqrtNot.TensorProduct(I)
                                              * Phase00.Dagger();
                                            
    public static readonly ComplexMatrix Bob2 = I.TensorProduct(SqrtNot)
                                              * Flip01
                                              * BeamSplit.TensorSquare()
                                              * Phase.TensorProduct(I);
                                            
    public static readonly ComplexMatrix Bob3 = PlusOne
                                              * I.TensorProduct(H);

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
        return string.Format("√{0} ⋅ ∠{1}°", (c.Magnitude*c.Magnitude).ToString(af ?? "0.##"), (c.Phase*180/Math.PI).ToString(pf ?? "0.#"));
    }
    public static KeyValuePair<TKey, TVal> KeyVal<TKey, TVal>(this TKey key, TVal val) {
        return new KeyValuePair<TKey, TVal>(key, val);
    } 

    private static readonly KeyValuePair<string, ComplexMatrix>[] BasicGatesToSearch = new Func<KeyValuePair<string, ComplexMatrix>[]>(() => {
        var singleWireBasicGates = new Dictionary<string, ComplexMatrix> {
            {"H", H},
            {"X", X},
            {"Y", Y},
            {"Z", Z},
            {"Phase", Phase},
            {"SqrtNot", SqrtNot},
            {"BeamSplit", BeamSplit}
        };
        var eitherWireGates = from singleWireGate in singleWireBasicGates
                              from gateOnOneOfTwoWires in new[] {
                                  (singleWireGate.Key + ".TensorProduct(I)").KeyVal(singleWireGate.Value.TensorProduct(I)),
                                  ("I.TensorProduct(" + singleWireGate.Key + ")").KeyVal(I.TensorProduct(singleWireGate.Value))
                              }
                              select gateOnOneOfTwoWires;
        var twoWireBasicGates = new Dictionary<string, ComplexMatrix> {
            {"ControlledNot", ControlledNot},
            {"ControlledNot2", ControlledNot2},
            {"Swap", Swap},
            {"PlusOne", PlusOne},
            {"Flip00", Flip00},
            {"Flip01", Flip01},
            {"Flip10", Flip10},
            {"Flip11", Flip11},
            {"Phase00", Phase00},
            {"Phase01", Phase01},
            {"phase10i", Phase10},
            {"Phase11", Phase11},
            {"Phase00.Dagger()", Phase00.Dagger()},
            {"Phase01.Dagger()", Phase01.Dagger()},
            {"phase10i.Dagger()", Phase10.Dagger()},
            {"Phase11.Dagger()", Phase11.Dagger()}
        };

        return eitherWireGates.Concat(twoWireBasicGates).ToArray();
    }).Invoke();

    public static IEnumerable<KeyValuePair<string, ComplexMatrix>> CircuitSearch() {
        var seen = new HashSet<ComplexMatrix> {I};
        foreach (var head in BasicGatesToSearch) {
            seen.Add(head.Value);
            yield return head;
        }
        foreach (var head in CircuitSearch()) {
            foreach (var nextGate in BasicGatesToSearch) {
                var f = head.Value * nextGate.Value;
                if (seen.Add(f)) {
                    yield return new KeyValuePair<string, ComplexMatrix>(head.Key + " * " + nextGate.Key, f);
                }
            }
        }
    }

    public static IEnumerable<string> FindCircuitsIsomorphicTo(ComplexMatrix target) {
        return from circuit in CircuitSearch()
               where circuit.Value.IsPhased(target)
               select circuit.Key;
    }
}
