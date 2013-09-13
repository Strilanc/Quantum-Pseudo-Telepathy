using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Strilanc.LinqToCollections;

[DebuggerDisplay("{ToString()}")]
public struct ComplexMatrix {
    private readonly IReadOnlyList<IReadOnlyList<Complex>> _columns;
    private ComplexMatrix(IReadOnlyList<IReadOnlyList<Complex>> columns) {
        if (columns == null) throw new ArgumentNullException("columns");
        if (columns.Any(col => col.Count != columns.Count)) throw new ArgumentException("Not square");
        this._columns = columns;
    }

    public static ComplexMatrix FromColumns(IEnumerable<IEnumerable<Complex>> columns) {
        return new ComplexMatrix(columns.Select(e => e.ToArray()).ToArray());
    }
    public static ComplexMatrix MakeSinglePhaseInverter(int size, int flippedState) {
        return FromCellData((from i in size.Range()
                             from j in size.Range()
                             select (i == j ? Complex.One : 0)*(i == flippedState ? -1 : 1)).ToArray());
    }
    public bool IsIdentity() {
        return Columns.Select((e, i) => e.Select((f, j) => (f - (j == i ? 1 : 0)).Magnitude < 0.0001).All(f => f)).All(e => e);
    }
    public bool IsUnitary() {
        return (this*this.Dagger()).IsIdentity();
    }
    public static ComplexMatrix FromCellData(params Complex[] cells) {
        var size = (int)Math.Sqrt(cells.Length);
        var cols = cells.Deinterleave(size);
        return FromColumns(cols);
    }
    public static ComplexMatrix MakeIdentity(int size) {
        return FromCellData((from i in size.Range()
                             from j in size.Range()
                             select i == j ? Complex.One : 0).ToArray());
    }
    public static ComplexMatrix MakeHadamard(int power) {
        if (power == 0) return FromCellData(1);
        var h = MakeHadamard(power - 1);
        return FromMatrixData(h, h, h, -h);
    }
    public static ComplexMatrix MakeUnitaryHadamard(int power) {
        return MakeHadamard(power)*Math.Pow(0.5, 0.5*power);
    }
    public static ComplexMatrix FromMatrixData(params ComplexMatrix[] cells) {
        var inSize = cells.First().Span;
        var outSize = (int)Math.Sqrt(cells.Length);
        var size = inSize * outSize;
        return FromColumns(size.Range().Select(c => size.Range().Select(r => cells.Deinterleave(outSize)[c / inSize][r / inSize].Columns[c % inSize][r % inSize]).ToArray()).ToArray());
    }
    public ComplexMatrix TensorSquare() {
        return this.TensorProduct(this);
    }
    public ComplexMatrix TensorProduct(ComplexMatrix other) {
        var s = this;
        return FromColumns(
            from c1 in s.Columns
            from c2 in other.Columns
            select from r1 in c1
                   from r2 in c2
                   select r1 * r2);
    }
    public ComplexMatrix Transpose() {
        return FromColumns(Rows);
    }
    public ComplexMatrix ExpandToApplyToMoreWires(int wireCount, int[] correspondingWiresForEachState) {
        if ((1 << correspondingWiresForEachState.Length) != Span) throw new ArgumentOutOfRangeException();
        if ((1 << wireCount) < Span) throw new ArgumentOutOfRangeException();

        var otherWires = wireCount.Range().Except(correspondingWiresForEachState).ToArray();
        var x = this;
        var result = from col in new[] {0, 1}.ChooseWithReplacement(wireCount)
                     select from row in new[] {0, 1}.ChooseWithReplacement(wireCount)
                            let r = correspondingWiresForEachState.Select((e, i) => row[e] << i).Sum()
                            let c = correspondingWiresForEachState.Select((e, i) => col[e] << i).Sum()
                            let xr = otherWires.Select((e, i) => row[e] << i).Sum()
                            let xc = otherWires.Select((e, i) => col[e] << i).Sum()
                            select xr == xc ? x.Columns[c][r] : 0;
        return FromColumns(result.Select(e => e.ToArray()).ToArray());
    }

    public ComplexMatrix Dagger() {
        return FromColumns(this.Rows.Select(e => e.Select(x => new Complex(x.Real, -x.Imaginary))));
    }
    public int Span { get { return _columns == null ? 0 : _columns.Count; } }
    public IReadOnlyList<IReadOnlyList<Complex>> Columns { get { return _columns ?? ReadOnlyList.Empty<IReadOnlyList<Complex>>(); } }
    public IReadOnlyList<IReadOnlyList<Complex>> Rows {
        get {
            var r = Columns;
            return new AnonymousReadOnlyList<IReadOnlyList<Complex>>(
                r.Count,
                row => new AnonymousReadOnlyList<Complex>(
                    r.Count,
                    col => r[col][row]));
        }
    }
    public bool IsPhased(ComplexMatrix other) {
        var c = other.Columns.First().First()/this.Columns.First().First();
        return (this * c) == other;
    }
    public bool IsSuperSimple() {
        return Columns.Select((e, i) => (e[i].Magnitude - 1).Abs() < 0.00001).All(e => e);
    }
    public bool IsSimple() {
        return Columns.All(e => e.Count(c => c != 0) == 1);
    }
    public static ComplexVector operator *(ComplexVector vector, ComplexMatrix matrix) {
        return new ComplexVector(
            matrix.Rows
            .Select(r => new ComplexVector(r) * vector)
            .ToArray());
    }
    public static ComplexMatrix operator -(ComplexMatrix matrix) {
        return matrix * -1;
    }
    public static ComplexMatrix operator *(ComplexMatrix matrix, Complex scale) {
        return FromColumns(matrix.Columns.Select(e => e.Select(c => c * scale).ToArray()).ToArray());
    }
    public static ComplexMatrix operator *(Complex scale, ComplexMatrix matrix) {
        return matrix*scale;
    }
    public static bool operator ==(ComplexMatrix v1, ComplexMatrix v2) {
        return v1.Span == v2.Span
               && v1.Span.Range().All(c => v1.Span.Range().All(r => (v1.Columns[c][r] - v2.Columns[c][r]).Magnitude < 0.00001));
    }
    public static bool operator !=(ComplexMatrix v1, ComplexMatrix v2) {
        return !(v1 == v2);
    }
    public static ComplexMatrix operator /(ComplexMatrix matrix, Complex scale) {
        return FromColumns(matrix.Columns.Select(e => e.Select(c => c / scale).ToArray()).ToArray());
    }
    public static ComplexMatrix operator *(ComplexMatrix left, ComplexMatrix right) {
        return new ComplexMatrix(left.Columns.Select(c => (new ComplexVector(c) * right).Values).ToArray());
    }
    public override bool Equals(object obj) {
        return obj is ComplexMatrix && (ComplexMatrix)obj == this;
    }
    public override int GetHashCode() {
        return Columns.SelectMany(e => e).Aggregate(Span.GetHashCode(), (a, e) => a*3 + Math.Round(e.Real*1000).GetHashCode()*5 + Math.Round(e.Imaginary*1000).GetHashCode());
    }
    public override string ToString() {
        return Rows.Select(r => r.Select(c => "| " + c.ToPrettyString().PadRight(6)).StringJoin("") + " |").StringJoin(Environment.NewLine);
    }
}