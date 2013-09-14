using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

public static class Gates {
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

    public static ComplexMatrix OnWire1Of2(this ComplexMatrix gate) {
        return gate.TensorProduct(NoGate);
    }
    public static ComplexMatrix OnWire2Of2(this ComplexMatrix gate) {
        return NoGate.TensorProduct(gate);
    }
    public static ComplexMatrix OnBothWires(this ComplexMatrix gate) {
        return gate.TensorSquare();
    }

    //equivalent matrix:
    //| 1        i|
    //|    i  1   |
    //|    1  i   | / sqrt(2)
    //| i        1|
    //
    //circuit:
    //---⊕---⧅---⊕---x---
    //   |        |    |
    //---.--------.----x---
    public static readonly ComplexMatrix Alice1 = ControlledNot 
                                                * BeamSplit.OnWire1Of2()
                                                * ControlledNot
                                                * Swap;

    //equivalent matrix:
    //| 1  i  i  1|
    //| 1 -i  i -1|
    //| i -1  1 -i| / 2
    //| i  1  1  i|
    //
    //circuit:
    //------------.---⧅---
    //            |
    //---H---⧅---⊕-------
    public static readonly ComplexMatrix Alice2 = H.OnWire2Of2() 
                                                * BeamSplit.OnWire2Of2() 
                                                * ControlledNot2 
                                                * BeamSplit.OnWire1Of2();

    //equivalent matrix:
    //| 1  1  1 -1|
    //| 1  1 -1  1|
    //| 1 -1  1  1| / 2
    //|-1  1  1  1|
    //
    //circuit:
    //---H---⊕---H---
    //       |      
    //-------.--------
    public static readonly ComplexMatrix Alice3 = H.OnWire1Of2()
                                                * ControlledNot
                                                * H.OnWire1Of2();

    //equivalent matrix:
    //| 1 -1  i  i|
    //| i  i  1 -1|
    //| i  i -1  1| / 2
    //|-1  1  i  i|
    //
    //circuit:
    //---⧅---x---⊕---⧅---
    //        |   |    
    //--------x---.--------
    public static readonly ComplexMatrix Bob1 = BeamSplit.OnWire1Of2() 
                                              * Swap 
                                              * ControlledNot 
                                              * BeamSplit.OnWire1Of2();

    //equivalent matrix:
    //| 1  i -1  i|
    //| i  1  i -1|
    //| i -1  i  1| / 2
    //|-1  i  1  i|
    //
    //circuit:
    //--------⊕---⧅---
    //        |
    //---⧅---.--------
    public static readonly ComplexMatrix Bob2 = BeamSplit.OnWire2Of2() 
                                              * ControlledNot 
                                              * BeamSplit.OnWire1Of2();

    //equivalent matrix:
    //|    i -i   |
    //|    i  i   |
    //| i       -i| / sqrt(2)
    //|-i       -i|
    //
    //circuit:
    //---Y---|‾‾‾‾|-------
    //       | +1 |
    //-------|____|---H---
    public static readonly ComplexMatrix Bob3 = Y.OnWire1Of2() 
                                              * PlusOne
                                              * H.OnWire2Of2();

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
                                  (singleWireGate.Key + ".OnWire1Of2()").KeyVal(singleWireGate.Value.OnWire1Of2()),
                                  (singleWireGate.Key + ".OnWire2Of2()").KeyVal(singleWireGate.Value.OnWire2Of2())
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
            Tuple.Create(ImmutableList<string>.Empty, 0, NoGate.OnBothWires())
        });
        var seen = new HashSet<ComplexMatrix> { NoGate.OnBothWires() };
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
    private static readonly IEnumerator<KeyValuePair<ImmutableList<string>, ComplexMatrix>> CachedCircuitsFeeder = CircuitSearch(4).GetEnumerator();
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
