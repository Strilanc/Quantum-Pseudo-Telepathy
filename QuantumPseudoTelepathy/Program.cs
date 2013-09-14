using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Strilanc.LinqToCollections;

public static class Program {
    private static void FindPrintCircuit(ComplexMatrix target) {
        Console.WriteLine("-----");
        Debug.WriteLine("------");
        var r = from circuit in QuantumGates.CachedShortCircuitSearch()
                where circuit.Value.IsPhased(target)
                select circuit.Key.StringJoin("*");
        foreach (var c in r.Take(4)) {
            Console.WriteLine(c);
            Debug.WriteLine(c);
        }

    }
    public static void Main() {
        FindPrintCircuit(QuantumGates.Alice1);
        FindPrintCircuit(QuantumGates.Alice2);
        FindPrintCircuit(QuantumGates.Alice3);
        FindPrintCircuit(QuantumGates.Bob1);
        FindPrintCircuit(QuantumGates.Bob2);
        FindPrintCircuit(QuantumGates.Bob3);
        CheckAllGameRuns();
        while (true)
            Console.ReadLine();
    }

    public static void CheckAllGameRuns() {
        // test every possible run of the game, to ensure the strategy wins in every case
        var fails = from refereeRowChoice in 3.Range()
                    from refereeColChoice in 3.Range()
                    let results = RunGame(refereeRowChoice, refereeColChoice)
                    from outcome in results.Possibilities.Keys
                    let colsOfRow = outcome.Alice.Cells
                    let rowsOfCol = outcome.Bob.Cells
                    let rowParityIsEven = colsOfRow.Count(e => e) % 2 == 0
                    let colParityIsEven = rowsOfCol.Count(e => e) % 2 == 0
                    let exactlyOneOccupyingCommonGround = colsOfRow[refereeColChoice] != rowsOfCol[refereeRowChoice]
                    where !rowParityIsEven || !colParityIsEven || !exactlyOneOccupyingCommonGround
                    select new { refereeRowChoice, refereeColChoice, results, outcome };

        var fail = fails.FirstOrDefault();
        if (fail != null) throw new InvalidProgramException();
    }
    public static ProbabilityDistribution<WorldState> RunGame(int refereeRowChoice, int refereeColChoice) {
        // alice and bob each get two entangled qubits (alice's qubit 1 is guaranteed to match bob's qubit 1; same for qubits 2)
        var worldState = PreSharedQubitsSuperposition();
                    
        // alice runs her qubits through a quantum circuit, based on the row she must fill
        var intermediateState = AliceRunCircuit(worldState, refereeRowChoice);
        // bob runs his qubits through a quantum circuit, based on the column he must fill
        var finalState = BobRunCircuit(intermediateState, refereeColChoice);
        // (note: The circuit evaluations could have been done in either order. They commute.)
                    
        // and then they measure the respective outputs of their quantum circuits
        return Measure(finalState);
    }
    public static ComplexVector PreSharedQubitsSuperposition() {
        // Alice and Bob each get two entangled bits
        // Alice's qubit #1 is entangled such that it will always agree with Bob's qubit #1
        // Same deal for qubits #2
        var states = from bit1 in new[] { false, true }
                     from bit2 in new[] { false, true }
                     select new WorldState(
                         alice: new PlayerState(wire1: bit1, wire2: bit2),
                         bob:   new PlayerState(wire1: bit1, wire2: bit2));

        // convert our nice list of states into a superposition over 16 unnamed but indexed states inside a complex vector
        var superposition = new ComplexVector(
            (from stateIndex in WorldState.PossibleStateIndexes
             let stateForIndex = WorldState.FromStateIndex(stateIndex)
             let isIncluded = states.Contains(stateForIndex)
             let amplitude = isIncluded ? Complex.One / 2 : Complex.Zero
             select amplitude));

        return superposition;
    }
    public static ComplexVector BobRunCircuit(ComplexVector worldSuperposition, int column) {
        var circuits = new[] { QuantumGates.Bob1, QuantumGates.Bob2, QuantumGates.Bob3 };
        var circuit = circuits[column];

        var circuitInWorld = QuantumGates.NoGate.TensorSquare().TensorProduct(circuit);
        return worldSuperposition * circuitInWorld;
    }
    public static ComplexVector AliceRunCircuit(ComplexVector worldSuperposition, int row) {
        var circuits = new[] { QuantumGates.Alice1, QuantumGates.Alice2, QuantumGates.Alice3 };
        var circuit = circuits[row];

        var circuitInWorld = circuit.TensorProduct(QuantumGates.NoGate.TensorSquare());
        return worldSuperposition * circuitInWorld;
    }
    public static ProbabilityDistribution<WorldState> Measure(ComplexVector worldSuperposition) {
        return new ProbabilityDistribution<WorldState>(
            from stateIndex in WorldState.PossibleStateIndexes
            let amplitude = worldSuperposition.Values[stateIndex]
            let probability = Math.Pow(amplitude.Magnitude, 2)
            let state = WorldState.FromStateIndex(stateIndex)
            select state.KeyVal(probability));
    }
}

public struct WorldState {
    public readonly PlayerState Alice;
    public readonly PlayerState Bob;
    public WorldState(PlayerState alice, PlayerState bob)
        : this() {
        this.Alice = alice;
        this.Bob = bob;
    }

    public static readonly int StateSizeInBits = 4;
    public static readonly int StateSizeInPossibilities = 1 << StateSizeInBits;
    public static readonly IReadOnlyList<int> PossibleStateIndexes = StateSizeInPossibilities.Range();
    public static WorldState FromStateIndex(int index) {
        return new WorldState(
            alice: new PlayerState(
                wire1: (index & (1 << 3)) != 0,
                wire2: (index & (1 << 2)) != 0),
            bob: new PlayerState(
                wire1: (index & (1 << 1)) != 0,
                wire2: (index & (1 << 0)) != 0));
    }
}

public struct PlayerState {
    public readonly bool Wire1;
    public readonly bool Wire2;
    public PlayerState(bool wire1, bool wire2)
        : this() {
        Wire1 = wire1;
        Wire2 = wire2;
    }
    public bool[] Cells { get { return new[] {Wire1, Wire2, Wire1 != Wire2}; } }
}
