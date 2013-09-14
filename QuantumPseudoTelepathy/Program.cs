using System;
using System.Diagnostics;
using System.Linq;

public static class Program {

    public static void Main() {
        QuantumPseudoTelepathy.CheckAllGameRuns();

        FindPrintCircuitMatchingMatrix(QuantumGates.Bob3);
        FindPrintCircuitMatchingMatrix(QuantumGates.Bob2);
        FindPrintCircuitMatchingMatrix(QuantumGates.Bob1);
        FindPrintCircuitMatchingMatrix(QuantumGates.Alice1);
        FindPrintCircuitMatchingMatrix(QuantumGates.Alice2);
        FindPrintCircuitMatchingMatrix(QuantumGates.Alice3);

        while (true)
            Console.ReadLine();
    }

    private static void FindPrintCircuitMatchingMatrix(ComplexMatrix target) {
        var targetDesc = 
            "-----" + Environment.NewLine 
            + "Searching for circuits that match:" + Environment.NewLine 
            + "-----" + Environment.NewLine 
            + target.ToString() + Environment.NewLine 
            + " ----- (may take awhile)...";
        Console.WriteLine(targetDesc);
        Debug.WriteLine(targetDesc);

        var r = from circuit in QuantumGates.CachedShortCircuitSearch()
                where circuit.Value.IsPhased(target)
                select circuit.Key.StringJoin("*");
        foreach (var c in r.Take(4)) {
            Console.WriteLine(c);
            Debug.WriteLine(c);
        }
    }
}
