using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ComplexMatrixTest {
    [TestMethod]
    public void TestIsMultiPhase() {
        var i = Complex.ImaginaryOne;
        var r1 = ComplexMatrix.FromSquareData(1, 0, 0, 1);
        var r2 = ComplexMatrix.FromSquareData(i, 0, 0, 1);
        var r3 = ComplexMatrix.FromSquareData(0, 1, 1, 0);
        var r4 = ComplexMatrix.FromSquareData(1, 1, 1, -1)/Math.Sqrt(2);

        Assert.IsTrue(r1.IsMultiRowPhased(r1));
        Assert.IsTrue(r1.IsMultiRowPhased(r2));
        Assert.IsTrue(!r1.IsMultiRowPhased(r3));
        Assert.IsTrue(!r1.IsMultiRowPhased(r4));

        Assert.IsTrue(!r2.IsMultiRowPhased(r3));
        Assert.IsTrue(!r2.IsMultiRowPhased(r4));

        Assert.IsTrue(!r3.IsMultiRowPhased(r4));
        Assert.IsTrue(r3.IsMultiRowPhased(r3*i));
    }
    [TestMethod]
    public void TestMultiplication() {
        var M1 = ComplexMatrix.FromSquareData(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 0, 1,
                0, 0, 1, 0);
        var M2 = ComplexMatrix.FromSquareData(
                1, 0, 0, 0,
                0, 0, 1, 0,
                0, 1, 0, 0,
                0, 0, 0, 1);
        var M3 = ComplexMatrix.FromSquareData(
                1, 0, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1,
                0, 1, 0, 0);
        Assert.AreEqual(M1*M2, M3);
    }
    [TestMethod]
    public void TestVectorMultiplication() {
        var M1 = ComplexMatrix.FromSquareData(
                10, 11, 12,
                13, 14, 15,
                16, 17, 18);
        var V = new ComplexVector(new Complex[] {-1,2,3});
        var V2 = new ComplexVector(new Complex[] { 48, 60, 72 });
        Assert.AreEqual(M1 * V, V2);
    }
    [TestMethod]
    public void TestVectorMultiplication2() {
        var M1 = ComplexMatrix.FromSquareData(
                10, 11, 12,
                13, 14, 15,
                16, 17, 18);
        var V = new ComplexVector(new Complex[] { -1, 2, 3 });
        var V2 = new ComplexVector(new Complex[] { 64, 68, 72 });
        Assert.AreEqual(V * M1, V2);
    }
    [TestMethod]
    public void TestMultiplication2() {
        var M1 = ComplexMatrix.FromSquareData(
                -1, 2, 3,
                4, 5, 6,
                7, 8, 9);
        var M2 = ComplexMatrix.FromSquareData(
                10, 11, 12,
                13, 14, 15,
                16, 17, 18);
        var M3 = ComplexMatrix.FromSquareData(
                64, 68, 72,
                201, 216, 231,
                318, 342, 366);
        Assert.AreEqual(M1 * M2, M3);
    }
}
