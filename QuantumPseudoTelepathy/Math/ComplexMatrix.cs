using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Strilanc.LinqToCollections;

[DebuggerDisplay("{ToString()}")]
public struct ComplexMatrix {
    private readonly Complex[][] _columns;
    private ComplexMatrix(IEnumerable<IEnumerable<Complex>> columns) {
        if (columns == null) throw new ArgumentNullException("columns");
        this._columns = columns.Select(e => e.ToArray()).ToArray();

        var span = _columns.Length;
        if (_columns.Any(e => e.Length != span)) throw new ArgumentOutOfRangeException("columns", "Not square");
    }

    public int Span { get { return _columns == null ? 0 : _columns.Length; } }
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

    public static ComplexMatrix FromColumns(IEnumerable<IEnumerable<Complex>> columns) {
        return new ComplexMatrix(columns);
    }
    public static ComplexMatrix FromSquareData(params Complex[] cells) {
        if (cells == null) throw new ArgumentNullException("cells");

        var size = (int)Math.Round(Math.Sqrt(cells.Length));
        if (size * size != cells.Length) throw new ArgumentOutOfRangeException("cells", "cells.Length is not square");

        var cols = cells.Deinterleave(size);
        return FromColumns(cols);
    }

    public bool IsIdentity() {
        return Columns.Select((e, i) => e.Select((f, j) => (f - (j == i ? 1 : 0)).Magnitude < 0.0001).All(f => f)).All(e => e);
    }
    public bool IsUnitary() {
        return (this*this.Dagger()).IsIdentity();
    }
    /// <summary>Determines if this * c == other, for some c where |c| == 1</summary>
    public bool IsPhased(ComplexMatrix other) {
        var self = this;
        return (from c in self.Span.Range()
                from r in self.Span.Range()
                let v = self.Columns[c][r]
                where v != 0
                let ov = other.Columns[c][r]
                select self * (ov / v) == other
                ).FirstOrDefault();
    }
    /// <summary>Determines adjusting the row phases of other can make it equal to this.</summary>
    public bool IsMultiRowPhased(ComplexMatrix other) {
        return Rows.Zip(other.Rows, (c1, c2) => new ComplexVector(c1).IsPhased(new ComplexVector(c2))).All(e => e);
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

    ///<summary>Returns the result of conjugating the elements of and tranposing this matrix.</summary>
    public ComplexMatrix Dagger() {
        return FromColumns(this.Rows.Select(e => e.Select(x => new Complex(x.Real, -x.Imaginary))));
    }

    public static ComplexVector operator *(ComplexMatrix matrix, ComplexVector vector) {
        return new ComplexVector(
            matrix.Rows
            .Select(r => new ComplexVector(r) * vector));
    }
    public static ComplexVector operator *(ComplexVector vector, ComplexMatrix matrix) {
        return new ComplexVector(
            matrix.Columns
            .Select(r => new ComplexVector(r) * vector));
    }
    public static ComplexMatrix operator -(ComplexMatrix matrix) {
        return matrix * -1;
    }
    public static ComplexMatrix operator *(ComplexMatrix matrix, Complex scale) {
        return FromColumns(matrix.Columns.Select(e => e.Select(c => c * scale)));
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
        return FromColumns(matrix.Columns.Select(e => e.Select(c => c / scale)));
    }
    public static ComplexMatrix operator *(ComplexMatrix left, ComplexMatrix right) {
        if (left.Span != right.Span) throw new ArgumentOutOfRangeException();
        return FromColumns(
            from c in right.Columns
            select from r in left.Rows
                   select new ComplexVector(r)*new ComplexVector(c));
    }
    public override bool Equals(object obj) {
        return obj is ComplexMatrix && (ComplexMatrix)obj == this;
    }
    public override int GetHashCode() {
        // This is a hack that works in the cases that I needed it to work in.
        // It groups most approximately equal matrices, but not ones across the third digit after decimal boundaries.
        return Columns.SelectMany(e => e).Aggregate(
            Span.GetHashCode(), 
            (a, e) => a*3 + Math.Round(e.Real*1000).GetHashCode()*5 + Math.Round(e.Imaginary*1000).GetHashCode());
    }
    public override string ToString() {
        return Rows.Select(r => r.Select(c => "| " + c.ToPrettyString().PadRight(6))
                                 .StringJoin("")
                                 + " |")
                   .StringJoin(Environment.NewLine);
    }
}
