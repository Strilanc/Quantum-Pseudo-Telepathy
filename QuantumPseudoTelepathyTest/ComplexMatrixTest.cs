using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ComplexMatrixTest {
    [TestMethod]
    public void TestIsMultiPhase() {
        var i = Complex.ImaginaryOne;
        var r1 = ComplexMatrix.FromCellData(1, 0, 0, 1);
        var r2 = ComplexMatrix.FromCellData(i, 0, 0, 1);
        var r3 = ComplexMatrix.FromCellData(0, 1, 1, 0);
        var r4 = ComplexMatrix.FromCellData(1, 1, 1, -1)/Math.Sqrt(2);

        Assert.IsTrue(r1.IsMultiRowPhased(r1));
        Assert.IsTrue(r1.IsMultiRowPhased(r2));
        Assert.IsTrue(!r1.IsMultiRowPhased(r3));
        Assert.IsTrue(!r1.IsMultiRowPhased(r4));

        Assert.IsTrue(!r2.IsMultiRowPhased(r3));
        Assert.IsTrue(!r2.IsMultiRowPhased(r4));

        Assert.IsTrue(!r3.IsMultiRowPhased(r4));
        Assert.IsTrue(r3.IsMultiRowPhased(r3*i));
    }
}
