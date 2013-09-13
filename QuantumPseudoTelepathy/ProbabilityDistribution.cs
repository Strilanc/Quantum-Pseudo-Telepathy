using System;
using System.Collections.Generic;
using System.Linq;

public sealed class ProbabilityDistribution<T> {
    public readonly IReadOnlyDictionary<T, double> Possibilities;
    public ProbabilityDistribution(IEnumerable<KeyValuePair<T, double>> possibilities) {
        if (possibilities == null) throw new ArgumentNullException("possibilities");
        this.Possibilities = 
            possibilities
            .Where(e => e.Value > 0.0000001)
            .ToDictionary(e => e.Key, e => e.Value);

        // well-formed distributions must add up to 100%:
        var totalProbability = Possibilities.Values.Aggregate(0.0, (a, e) => a + e);
        if ((totalProbability - 1).Abs() > 0.0001) throw new ArgumentOutOfRangeException("possibilities", "Probabilities must add up to 1.");
    }
    public T Sample(Random rng) {
        var p = rng.NextDouble();
        foreach (var keyValuePair in Possibilities) {
            p -= keyValuePair.Value;
            if (p < 0) return keyValuePair.Key;
        }
        return Possibilities.Last().Key;
    }
}
