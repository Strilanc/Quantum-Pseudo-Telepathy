using System;
using System.Linq;
using System.Numerics;
using Strilanc.LinqToCollections;

public static class Program {
    public static void Main() {
        var a1 = Util.FindCircuitsIsomorphicTo(Util.Alice1).First();
        var a2 = Util.FindCircuitsIsomorphicTo(Util.Alice2).First();
        var a3 = Util.FindCircuitsIsomorphicTo(Util.Alice3).First();
        var b1 = Util.FindCircuitsIsomorphicTo(Util.Bob1).First();
        var b2 = Util.FindCircuitsIsomorphicTo(Util.Bob2).First();
        var b3 = Util.FindCircuitsIsomorphicTo(Util.Bob3).First();
        RunTest();
    }

    public static void RunTest() {

        var As = new[] { Util.Alice1, Util.Alice2, Util.Alice3 };
        var Bs = new[] { Util.Bob1, Util.Bob2, Util.Bob3 };
        if (As.Concat(Bs).Any(e => !e.IsUnitary())) throw new Exception();

        var initialState = new ComplexVector(
            new[] { 0, 1 }.ChooseWithReplacement(4)
            .Select(e => (e[0] != e[2] && e[1] != e[3] ? (Complex)0.5 : 0) 
                       * (e[0] == e[1] ? 1 : -1))
            .ToArray());

        var tests = from row in 3.Range()
                    from col in 3.Range()
                    let finalState = initialState * As[col].TensorProduct(Bs[row])
                    from possibility in finalState.Values.Count.Range()
                    where finalState.Values[possibility].Magnitude > 0.0000001
                    let bits = 4.Range().Select(e => (possibility & (1 << (3 - e))) != 0).ToArray()
                    let rowEntries = new[] { bits[0], bits[1], bits[0] != bits[1] }
                    let colEntries = new[] { bits[2], bits[3], bits[2] == bits[3] }
                    select rowEntries[row] == colEntries[col];
        if (tests.Any(e => !e)) throw new Exception();
    }
}
