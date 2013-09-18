using System.Collections.Generic;
using System.Diagnostics;
using Strilanc.LinqToCollections;

[DebuggerDisplay("{ToString()}")]
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

    public override string ToString() {
        return string.Format(
            "Alice: {0}, Bob: {1}",
            Alice,
            Bob);
    }
}
