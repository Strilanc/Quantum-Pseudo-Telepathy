using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

public static class QuantumGates {
    private static readonly Complex i = Complex.ImaginaryOne;
    public static readonly ComplexMatrix NoGate = ComplexMatrix.FromCellData(
        1, 0,
        0, 1);
    
    // Hadamard gate
    public static readonly ComplexMatrix H = ComplexMatrix.FromCellData(
        1, 1,
        1, -1)/Math.Sqrt(2);
    // Pauli X gate
    public static readonly ComplexMatrix X = ComplexMatrix.FromCellData(
        0, 1, 
        1, 0);
    // Pauli Y gate
    public static readonly ComplexMatrix Y = ComplexMatrix.FromCellData(
        0, -i, 
        i, 0);
    // Pauli Z gate
    public static readonly ComplexMatrix Z = ComplexMatrix.FromCellData(
        1, 0, 
        0, -1);
    // R2 gate = pi/2 phase gate
    public static readonly ComplexMatrix Phase = ComplexMatrix.FromCellData(
        1, 0, 
        0, i);
    // 'Square root of not' gate
    public static readonly ComplexMatrix SqrtNot = ComplexMatrix.FromCellData(
        1, -1, 
        1, 1) / Math.Sqrt(2);
    // Half-mirror gate
    public static readonly ComplexMatrix BeamSplit = ComplexMatrix.FromCellData(
        1, i, 
        i, 1) / Math.Sqrt(2);

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


    //equivalent matrix:
    //
    //|-i        1|
    //|    i  1   |
    //|    i -1   | / sqrt(2)
    //| 1       -i|
    //
    //circuit:
    //                      ____
    //--------⊕---H---⊕---|[00>|---
    //        |        |   | *-i|
    //---R2---.--------.---|____|---
    public static readonly ComplexMatrix Alice1 = NoGate.TensorProduct(Phase) 
                                                * ControlledNot          
                                                * H.TensorProduct(NoGate)     
                                                * ControlledNot          
                                                * Phase00.Dagger();

    //equivalent matrix:
    //
    //| 1  i  i  1|
    //| 1 -i  i -1|
    //|-1 -i  i  1| / 2
    //|-1  i  i -1|
    //
    //circuit:
    //        ____     ____
    //---X---|[00>|---|[11>|---H---
    //       | *i |   | *i |
    //-------|____|---|____|---H---
    public static readonly ComplexMatrix Alice2 = X.TensorProduct(NoGate) 
                                                * Phase00 
                                                * Phase11 
                                                * H.TensorSquare();

    //equivalent matrix:
    //
    //| 1  1  1 -1|
    //| 1  1 -1  1|
    //| 1 -1  1  1| / 2
    //|-1  1  1  1|
    //
    //circuit:
    //
    //---H---⊕---H---
    //       |      
    //-------.--------
    public static readonly ComplexMatrix Alice3 = H.TensorProduct(NoGate)
                                                * ControlledNot
                                                * H.TensorProduct(NoGate);

    //equivalent matrix:
    //
    //| 1 -1  i  i|
    //| i  i  1 -1|
    //| 1  1  i -i| / 2
    //|-1  1  i  i|
    //
    //circuit:
    //                    _____
    //---⧅------⊕---H---|[11>|---
    //           |       |*-i |
    //---|1>*i---.-------|____|---
    public static readonly ComplexMatrix Bob1 = BeamSplit.TensorProduct(Phase) 
                                              * ControlledNot
                                              * H.TensorProduct(NoGate)
                                              * Phase11.Dagger();

    //equivalent matrix:
    //
    //| 1  i -1  i|
    //|-1  i -1 -i|
    //|-1 -i -1  i| / 2
    //| 1 -i -1 -i|
    //
    //circuit:
    //
    //--------⊕---⧅---R2---
    //        |
    //---⧅---.--------R2----
    public static readonly ComplexMatrix Bob2 = NoGate.TensorProduct(BeamSplit) 
                                              * ControlledNot 
                                              * BeamSplit.TensorProduct(NoGate) 
                                              * Phase.TensorSquare();

    //equivalent matrix:
    //
    //| 0  1 -1  0|
    //| 0  1  1  0|
    //|-1  0  0  1| / sqrt(2)
    //| 1  0  0  1|
    //
    //circuit:
    //        ____
    //---X---|    |--------
    //       | +1 |
    //-------|____|---√!---
    public static readonly ComplexMatrix Bob3 = X.TensorProduct(NoGate) 
                                              * PlusOne 
                                              * NoGate.TensorProduct(SqrtNot);

    // -----------------------------------------------------------
    // ----V---- searching for circuits that match a matrix --V---
    // -----------------------------------------------------------

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
                                  (singleWireGate.Key + ".TensorProduct(NoGate)").KeyVal(singleWireGate.Value.TensorProduct(NoGate)),
                                  ("NoGate.TensorProduct(" + singleWireGate.Key + ")").KeyVal(NoGate.TensorProduct(singleWireGate.Value))
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
            {"Phase10i", Phase10},
            {"Phase11", Phase11},
            {"Phase00.Dagger()", Phase00.Dagger()},
            {"Phase01.Dagger()", Phase01.Dagger()},
            {"Phase10i.Dagger()", Phase10.Dagger()},
            {"Phase11.Dagger()", Phase11.Dagger()}
        };

        return eitherWireGates.Concat(twoWireBasicGates).ToArray();
    }).Invoke();

    public static IEnumerable<KeyValuePair<ImmutableList<string>, ComplexMatrix>> CircuitSearch(int maxNumberOfGates) {
        var queue = new Queue<Tuple<ImmutableList<string>, int, ComplexMatrix>>(new[] {
            Tuple.Create(ImmutableList<string>.Empty, 0, NoGate.TensorSquare())
        });
        var seen = new HashSet<ComplexMatrix> { NoGate.TensorSquare() };
        while (queue.Count > 0) {
            var head = queue.Dequeue();
            
            foreach (var nextGate in BasicGatesToSearch) {
                var f = head.Item3 * nextGate.Value;
                if (seen.Add(f)) {
                    var e = head.Item1.Add(nextGate.Key);
                    if (head.Item2 < maxNumberOfGates - 1) {
                        queue.Enqueue(Tuple.Create(e, head.Item2 + 1, f));
                    }
                    yield return e.KeyVal(f);
                }
            }
        }
    }

    private static readonly List<KeyValuePair<ImmutableList<string>, ComplexMatrix>> CachedCircuits = new List<KeyValuePair<ImmutableList<string>, ComplexMatrix>>();
    private static readonly IEnumerator<KeyValuePair<ImmutableList<string>, ComplexMatrix>> CachedCircuitsFeeder = CircuitSearch(5).GetEnumerator();
    private static bool _doneFeedingCircuits;
    public static IEnumerable<KeyValuePair<ImmutableList<string>, ComplexMatrix>> CachedShortCircuitSearch() {
        var index = 0;
        while (true) {
            if (index == CachedCircuits.Count) {
                if (_doneFeedingCircuits) break;
                _doneFeedingCircuits = !CachedCircuitsFeeder.MoveNext();
                if (_doneFeedingCircuits) break;
                CachedCircuits.Add(CachedCircuitsFeeder.Current);
            }

            yield return CachedCircuits[index];
            index += 1;
        }
    }
}
