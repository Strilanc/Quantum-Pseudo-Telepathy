using System;
using System.Numerics;

public static class PseudoTelepathyCircuits {
    private static readonly Complex i = Complex.ImaginaryOne;
    private static readonly bool BuildFromBasicGates = true;

    public static readonly ComplexMatrix AliceTopRow =
        BuildFromBasicGates
        ?
            //circuit:
            //---.---⧅---.---x---
            //   |        |   |
            //---⊕-------⊕---x---
            Gates.ControlledNot2When1
            .Then(Gates.BeamSplit.OnWire1Of2())
            .Then(Gates.ControlledNot2When1)
            .Then(Gates.Swap)
        :
            //matrix (row phases are arbitrary):
            ComplexMatrix.FromCellData(
                1, 0, 0, i,
                0, 1,-i, 0,
                0,-i, 1, 0,
                i, 0, 0, 1)/Math.Sqrt(2);

    public static readonly ComplexMatrix AliceCenterRow =
        BuildFromBasicGates
        ?
            //circuit:
            //---⧅---⊕-------.---
            //        |       |
            //-------.---H---⊕---
            Gates.BeamSplit.OnWire1Of2()
            .Then(Gates.ControlledNot1When2)
            .Then(Gates.H.OnWire2Of2())
            .Then(Gates.ControlledNot2When1)
        :
            //matrix (row phases are arbitrary):
            ComplexMatrix.FromCellData(
                1, i, i, 1,
                1,-i, i,-1,
                1, i,-i,-1,
                1,-i,-i, 1)/2;

    public static readonly ComplexMatrix AliceBottomRow =
        BuildFromBasicGates
        ?
            //circuit:
            //---H---.---H---
            //       |      
            //-------⊕-------
            Gates.H.OnWire1Of2()
            .Then(Gates.ControlledNot2When1)
            .Then(Gates.H.OnWire1Of2())
        :
            //matrix (row phases are arbitrary):
            ComplexMatrix.FromCellData(
                1, 1, 1,-1,
                1, 1,-1, 1,
                1,-1, 1, 1,
               -1, 1, 1, 1) / 2;

    public static readonly ComplexMatrix BobLeftColumn =
        BuildFromBasicGates
        ?
            //circuit:
            //---.---⧅---x---
            //   |        |   
            //---⊕---⧅---x---
            Gates.ControlledNot1When2
            .Then(Gates.BeamSplit.OnBothWires())
            .Then(Gates.Swap)
        :
            //matrix (row phases are arbitrary):
            ComplexMatrix.FromCellData(
                1,-1, i, i,
                1, 1,-i, i,
                1, 1, i,-i,
                1,-1,-i,-i) / 2;

    public static readonly ComplexMatrix BobCenterColumn = 
        BuildFromBasicGates
        ?
            //circuit:
            //-------.---⧅---
            //        |
            //---⧅---⊕--------
            Gates.BeamSplit.OnWire2Of2()
            .Then(Gates.ControlledNot2When1)
            .Then(Gates.BeamSplit.OnWire1Of2())
        :
            //matrix (row phases are arbitrary):
            ComplexMatrix.FromCellData(
                1, i,-1, i,
                1,-i, 1, i,
                1, i, 1,-i,
                1,-i,-1,-i) / 2;

    public static readonly ComplexMatrix BobRightColumn =
        BuildFromBasicGates
        ?
            //circuit:
            //---|‾‾‾|-------
            //   |DEC|
            //---|___|---√!--
            Gates.Decrement
            .Then(Gates.SqrtNot.OnWire2Of2())
        :
            //matrix (row phases are arbitrary):
            ComplexMatrix.FromCellData(
                0, 1,-1, 0,
                0, 1, 1, 0,
                1, 0, 0,-1,
                1, 0, 0, 1) / Math.Sqrt(2);
}
