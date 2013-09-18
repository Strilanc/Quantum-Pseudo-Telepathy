using System;
using System.Linq;
using System.Numerics;
using Strilanc.LinqToCollections;

public static class PseudoTelepathy {
    public static void CheckAllGameRuns() {
        // test every possible run of the game, to ensure the strategy wins in every case
        var fails = from refereeRowChoice in 3.Range()
                    from refereeColChoice in 3.Range()
                    let results = PlayGame(refereeRowChoice, refereeColChoice)
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

        // if we reach here, then the strategy wins 100% of the time
        Console.WriteLine("Winning Strategy: Check!");
    }

    public static void RunAndPrintSampleGame(Random rng) {
        Console.WriteLine("+---------------------------------------------------------------");
        var refRow = rng.Next(3);
        var refCol = rng.Next(3);
        Console.WriteLine("| Ref picked row = {0}, col = {1}", refRow, refCol);

        Console.WriteLine("| ");
        Console.WriteLine("| Ref tells Alice the row and Bob the col.");
        Console.WriteLine("| They each pick a circuit and run their qubits through.");
        var quantumResult = PlayGame(refRow, refCol);

        Console.WriteLine("| They measure the outputs of their circuits:");
        var result = quantumResult.Sample(rng);
        Console.WriteLine("| " + result);

        var cells = (from row in 3.Range()
                     select (from col in 3.Range()
                             let isUnusedCell = row != refRow && col != refCol
                             let isCommonCell = row == refRow && col == refCol
                             select isUnusedCell ? " --"
                                  : isCommonCell ? ">"
                                  : " "
                             ).ToArray()
                     ).ToArray();

        foreach (var i in 3.Range()) {
            if (result.Alice.Cells[i]) cells[refRow][i] += "A";
            if (result.Bob.Cells[i]) cells[i][refCol] += "B";
        }

        Console.WriteLine("| ");
        Console.WriteLine("| Alice independently places {0} tokens", result.Alice.Cells.Count(e => e));
        Console.WriteLine("| Bob independently places {0} tokens", result.Bob.Cells.Count(e => e));
        Console.WriteLine("| The result:");
        Console.WriteLine("| " + 
            cells
            .Select(row => row.Select(cell => cell.PadRight(3)).StringJoin(" |"))
            .StringJoin(Environment.NewLine + "| ----+----+----" + Environment.NewLine + "| "));
        var win = result.Alice.Cells.Count(e => e)%2 == 0
                  && result.Bob.Cells.Count(e => e)%2 == 0
                  && result.Alice.Cells[refCol] != result.Bob.Cells[refRow];
        Console.WriteLine("| ");
        Console.WriteLine("+---------------------------------------------------------------");
        Console.WriteLine("| " + (win ? "They Won!" : "They Lose :("));
        Console.WriteLine("+---------------------------------------------------------------");
        Console.WriteLine();
    }

    public static ProbabilityDistribution<WorldState> PlayGame(int refereeRowChoice, int refereeColChoice) {
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
                         bob: new PlayerState(wire1: bit1, wire2: bit2));

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
        var circuits = new[] {
            PseudoTelepathyCircuits.BobLeftColumn, 
            PseudoTelepathyCircuits.BobCenterColumn, 
            PseudoTelepathyCircuits.BobRightColumn
        };
        
        var circuit = circuits[column];
        var circuitInWorld = Gates.NoGate.OnBothWires().TensorProduct(circuit);
        
        return worldSuperposition * circuitInWorld;
    }

    public static ComplexVector AliceRunCircuit(ComplexVector worldSuperposition, int row) {
        var circuits = new[] {
            PseudoTelepathyCircuits.AliceTopRow, 
            PseudoTelepathyCircuits.AliceCenterRow, 
            PseudoTelepathyCircuits.AliceBottomRow
        };
        
        var circuit = circuits[row];
        var circuitInWorld = circuit.TensorProduct(Gates.NoGate.OnBothWires());
        
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
