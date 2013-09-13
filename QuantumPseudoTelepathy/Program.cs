﻿using System;
using System.Diagnostics;
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
        Console.WriteLine(a1);
        Console.WriteLine(a2);
        Console.WriteLine(a3);
        Console.WriteLine(b1);
        Console.WriteLine(b2);
        Console.WriteLine(b3);
        Debug.WriteLine(a1);
        Debug.WriteLine(a2);
        Debug.WriteLine(a3);
        Debug.WriteLine(b1);
        Debug.WriteLine(b2);
        Debug.WriteLine(b3);
        RunTest();
        while (true)
            Console.ReadLine();
    }

    public static ComplexVector BobRunCircuit(ComplexVector worldSuperposition, int column) {
        var circuits = new[] { Util.Bob1, Util.Bob2, Util.Bob3 };
        var circuit = circuits[column];

        var circuitInWorld = Util.I.TensorSquare().TensorProduct(circuit);
        return worldSuperposition*circuitInWorld;
    }
    public static ComplexVector AliceRunCircuit(ComplexVector worldSuperposition, int row) {
        var circuits = new[] {Util.Alice1, Util.Alice2, Util.Alice3};
        var circuit = circuits[row];
        
        var circuitInWorld = circuit.TensorProduct(Util.I.TensorSquare());
        return worldSuperposition*circuitInWorld;
    }

    public static WorldState IndexToWireStates(this int index) {
        return new WorldState(
            new AliceState(
                wire1: (index & (1 << 3)) != 0,
                wire2: (index & (1 << 2)) != 0),
            new BobState(
                wire1: (index & (1 << 1)) != 0,
                wire2: (index & (1 << 0)) != 0));
    }

    public static ComplexVector PreSharedQubitsSuperposition() {
        // Alice and Bob each get two entangled bits
        // Alice's qubit #1 is entangled such that it will always agree with Bob's qubit #1
        // Same deal for qubits #2
        var states = from bit1 in new[] {false, true}
                     from bit2 in new[] {false, true}
                     select new WorldState(
                         new AliceState(wire1: bit1, wire2: bit2),
                         new BobState(wire1: bit1, wire2: bit2));

        // convert our nice list of states into a superposition over 16 unnamed but indexed states inside a complex vector
        var superposition = new ComplexVector(
            (from stateIndex in (1 << WorldState.StateSizeInBits).Range()
             let stateForIndex = stateIndex.IndexToWireStates()
             let isIncluded = states.Contains(stateForIndex)
             let amplitude = isIncluded ? Complex.One/2 : Complex.Zero
             select amplitude));
        
        return superposition;
    }
    public static ProbabilityDistribution<WorldState> Measure(ComplexVector worldSuperposition) {
        return new ProbabilityDistribution<WorldState>(
            from stateIndex in (1 << WorldState.StateSizeInBits).Range()
            let amplitude = worldSuperposition.Values[stateIndex]
            let probability = Math.Pow(amplitude.Magnitude, 2)
            let state = stateIndex.IndexToWireStates()
            select state.KeyVal(probability));
    }

    public static void RunTest() {
        var As = new[] { Util.Alice1, Util.Alice2, Util.Alice3 };
        var Bs = new[] { Util.Bob1, Util.Bob2, Util.Bob3 };
        if (As.Concat(Bs).Any(e => !e.IsUnitary())) throw new Exception();

        var tests = from row in 3.Range()
                    from col in 3.Range()

                    // do quantum:
                    let worldState = PreSharedQubitsSuperposition()
                    let intermediateState = AliceRunCircuit(worldState, row)
                    let finalState = BobRunCircuit(intermediateState, col)
                    let measure = Measure(finalState)

                    from possibility in measure.Possibilities.Keys
                    let colsOfRow = possibility.Alice.ValuesForColumnsOfRow
                    let rowsOfCol = possibility.Bob.ValuesForRowsOfColumn
                    let rowParityIsEven = colsOfRow.Count(e => e)%2 == 0
                    let colParityIsEven = rowsOfCol.Count(e => e)%2 == 0
                    let exactlyOneOccupyingCommonGround = colsOfRow[col] != rowsOfCol[row]
                    where !rowParityIsEven || !colParityIsEven || !exactlyOneOccupyingCommonGround
                    select 1;
        if (tests.Any()) throw new Exception();
    }
}
public struct WorldState {
    public static readonly int StateSizeInBits = 4;
    public readonly AliceState Alice;
    public readonly BobState Bob;
    public WorldState(AliceState alice, BobState bob)
        : this() {
        this.Alice = alice;
        this.Bob = bob;
    }
}
public struct AliceState {
    public readonly bool Wire1;
    public readonly bool Wire2;
    public AliceState(bool wire1, bool wire2)
        : this() {
        Wire1 = wire1;
        Wire2 = wire2;
    }
    public bool[] ValuesForColumnsOfRow { get { return new[] {Wire1, Wire2, Wire1 != Wire2}; } }
}
public struct BobState {
    public readonly bool Wire1;
    public readonly bool Wire2;
    public BobState(bool wire1, bool wire2)
        : this() {
        Wire1 = wire1;
        Wire2 = wire2;
    }
    public bool[] ValuesForRowsOfColumn { get { return new[] { !Wire1, !Wire2, Wire1 != Wire2 }; } }
}
