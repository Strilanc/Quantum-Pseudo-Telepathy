using System;
using System.Numerics;

public static class Gates {
    private static readonly Complex i = Complex.ImaginaryOne;
    public static readonly ComplexMatrix NoGate = ComplexMatrix.FromSquareData(
        1, 0,
        0, 1);
    
    // Hadamard gate
    public static readonly ComplexMatrix H = ComplexMatrix.FromSquareData(
        1, 1,
        1, -1)/Math.Sqrt(2);
    // Pauli X gate
    public static readonly ComplexMatrix X = ComplexMatrix.FromSquareData(
        0, 1, 
        1, 0);
    // Pauli Y gate
    public static readonly ComplexMatrix Y = ComplexMatrix.FromSquareData(
        0, -i, 
        i, 0);
    // Pauli Z gate
    public static readonly ComplexMatrix Z = ComplexMatrix.FromSquareData(
        1, 0, 
        0, -1);
    // R2 gate = pi/2 phase gate
    public static readonly ComplexMatrix Phase = ComplexMatrix.FromSquareData(
        1, 0, 
        0, i);
    // 'Square root of not' gate
    public static readonly ComplexMatrix SqrtNot = ComplexMatrix.FromSquareData(
        1, -1, 
        1, 1) / Math.Sqrt(2);
    // Half-mirror gate
    public static readonly ComplexMatrix BeamSplit = ComplexMatrix.FromSquareData(
        1, i, 
        i, 1) / Math.Sqrt(2);

    public static readonly ComplexMatrix ControlledNot2When1 = ComplexMatrix.FromSquareData(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 0, 1,
        0, 0, 1, 0);
    public static readonly ComplexMatrix ControlledNot1When2 = ComplexMatrix.FromSquareData(
        1, 0, 0, 0,
        0, 0, 0, 1,
        0, 0, 1, 0,
        0, 1, 0, 0);
    public static readonly ComplexMatrix Swap = ComplexMatrix.FromSquareData(
        1, 0, 0, 0,
        0, 0, 1, 0,
        0, 1, 0, 0,
        0, 0, 0, 1);
    public static readonly ComplexMatrix Increment = ComplexMatrix.FromSquareData(
        0, 0, 0, 1,
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0);
    public static readonly ComplexMatrix Decrement = Increment.Dagger();

    public static readonly ComplexMatrix Phase00 = ComplexMatrix.FromSquareData(
        i, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1);
    public static readonly ComplexMatrix Phase01 = ComplexMatrix.FromSquareData(
        1, 0, 0, 0,
        0, i, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1);
    public static readonly ComplexMatrix Phase10 = ComplexMatrix.FromSquareData(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, i, 0,
        0, 0, 0, 1);
    public static readonly ComplexMatrix Phase11 = ComplexMatrix.FromSquareData(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, i);
    public static readonly ComplexMatrix Flip00 = Phase00 * Phase00;
    public static readonly ComplexMatrix Flip01 = Phase01 * Phase01;
    public static readonly ComplexMatrix Flip10 = Phase10 * Phase10;
    public static readonly ComplexMatrix Flip11 = Phase11 * Phase11;
}
