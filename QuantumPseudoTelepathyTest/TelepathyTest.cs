using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class TelepathyTest {
    [TestMethod]
    public void TestTelepathy() {
        PseudoTelepathy.CheckAllGameRuns();
    }
    [TestMethod]
    public void TestIsUnitary() {
        Assert.IsTrue(PseudoTelepathyCircuits.AliceBottomRow.IsUnitary());
        Assert.IsTrue(PseudoTelepathyCircuits.AliceCenterRow.IsUnitary());
        Assert.IsTrue(PseudoTelepathyCircuits.AliceTopRow.IsUnitary());
        Assert.IsTrue(PseudoTelepathyCircuits.BobLeftColumn.IsUnitary());
        Assert.IsTrue(PseudoTelepathyCircuits.BobCenterColumn.IsUnitary());
        Assert.IsTrue(PseudoTelepathyCircuits.BobRightColumn.IsUnitary());
    }
}
