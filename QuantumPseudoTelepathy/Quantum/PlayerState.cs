using System.Diagnostics;

[DebuggerDisplay("{ToString()}")]
public struct PlayerState {
    public readonly bool Wire1;
    public readonly bool Wire2;
    public PlayerState(bool wire1, bool wire2) {
        Wire1 = wire1;
        Wire2 = wire2;
    }
    public bool[] Cells { get { return new[] { Wire1, Wire2, Wire1 ^ Wire2 }; } }
    public override string ToString() {
        return string.Format(
            "{0} {1}",
            Wire1 ? "On" : "Off",
            Wire2 ? "On" : "Off");
    }
}
