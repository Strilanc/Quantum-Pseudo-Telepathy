using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public static class Circuits {
    public static ComplexMatrix OnWire1Of2(this ComplexMatrix gate) {
        return gate.TensorProduct(Gates.NoGate);
    }
    public static ComplexMatrix OnWire2Of2(this ComplexMatrix gate) {
        return Gates.NoGate.TensorProduct(gate);
    }
    public static ComplexMatrix OnBothWires(this ComplexMatrix gate) {
        return gate.TensorSquare();
    }
    public static ComplexMatrix Then(this ComplexMatrix firstGate, ComplexMatrix secondGate) {
        return secondGate*firstGate;
    }

    private static readonly KeyValuePair<string, ComplexMatrix>[] BasicGatesToSearch = new Func<KeyValuePair<string, ComplexMatrix>[]>(() => {
        var singleWireBasicGates = new Dictionary<string, ComplexMatrix> {
            {"H", Gates.H},
            {"X", Gates.X},
            {"Y", Gates.Y},
            {"Z", Gates.Z},
            {"Phase", Gates.Phase},
            {"SqrtNot", Gates.SqrtNot},
            {"BeamSplit", Gates.BeamSplit}
        };
        var eitherWireGates = from singleWireGate in singleWireBasicGates
                              from gateOnOneOfTwoWires in new[] {
                                  (singleWireGate.Key + ".OnWire1Of2()").KeyVal(singleWireGate.Value.OnWire1Of2()),
                                  (singleWireGate.Key + ".OnWire2Of2()").KeyVal(singleWireGate.Value.OnWire2Of2())
                              }
                              select gateOnOneOfTwoWires;
        var twoWireBasicGates = new Dictionary<string, ComplexMatrix> {
            {"ControlledNot2When1", Gates.ControlledNot2When1},
            {"ControlledNot1When2", Gates.ControlledNot1When2},
            {"Swap", Gates.Swap},
            {"Increment", Gates.Increment},
            {"Decrement", Gates.Decrement},
            {"Flip00", Gates.Flip00},
            {"Flip01", Gates.Flip01},
            {"Flip10", Gates.Flip10},
            {"Flip11", Gates.Flip11},
            {"Phase00", Gates.Phase00},
            {"Phase01", Gates.Phase01},
            {"Phase10i", Gates.Phase10},
            {"Phase11", Gates.Phase11},
            {"Phase00.Dagger()", Gates.Phase00.Dagger()},
            {"Phase01.Dagger()", Gates.Phase01.Dagger()},
            {"Phase10i.Dagger()", Gates.Phase10.Dagger()},
            {"Phase11.Dagger()", Gates.Phase11.Dagger()}
        };

        return eitherWireGates.Concat(twoWireBasicGates).ToArray();
    }).Invoke();

    public static IEnumerable<KeyValuePair<ImmutableList<string>, ComplexMatrix>> DistinctCircuits(int maxNumberOfGates) {
        var queue = new Queue<Tuple<ImmutableList<string>, int, ComplexMatrix>>(new[] {
            Tuple.Create(ImmutableList<string>.Empty, 0, Gates.NoGate.OnBothWires())
        });
        var seen = new HashSet<ComplexMatrix> { Gates.NoGate.OnBothWires() };
        while (queue.Count > 0) {
            var head = queue.Dequeue();

            foreach (var nextGate in BasicGatesToSearch) {
                var f = head.Item3.Then(nextGate.Value);
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

    private static bool _doneFeedingCircuits;
    private static readonly List<KeyValuePair<ImmutableList<string>, ComplexMatrix>> CachedCircuits = 
        new List<KeyValuePair<ImmutableList<string>, ComplexMatrix>>();
    private static readonly IEnumerator<KeyValuePair<ImmutableList<string>, ComplexMatrix>> CachedCircuitsFeeder = 
        DistinctCircuits(4).GetEnumerator();
    public static IEnumerable<KeyValuePair<ImmutableList<string>, ComplexMatrix>> DistinctCircuitsUpToSize4_Cache() {
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
