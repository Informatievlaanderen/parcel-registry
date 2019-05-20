namespace ParcelRegistry.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using FluentAssertions;
    using Xunit;

    public class CaPaKeyTests
    {
        [Fact]
        public void CapaKeyFromIrregularFormat2Without_()
        {
            var key = CaPaKey.CreateFrom("11001B0009/00A005");
            key.CaPaKeyCrabNotation1.Should().Be("11001_B_0009_A_005_00");
            key.CaPaKeyCrabNotation2.Should().Be("11001B0009/00A005");
            key.VbrCaPaKey.ToString().Should().Be("11001B0009-00A005");
        }

        [Fact]
        public void CapaKeyFromIrregularFormat2Without_ForVbrCaPaKey()
        {
            var key = CaPaKey.CreateFrom("11001B0009-00A005");
            key.CaPaKeyCrabNotation1.Should().Be("11001_B_0009_A_005_00");
            key.CaPaKeyCrabNotation2.Should().Be("11001B0009/00A005");
            key.VbrCaPaKey.ToString().Should().Be("11001B0009-00A005");
        }

        [Fact]
        public void CapaKeyFromIrregularFormat1Without_()
        {
            var key = CaPaKey.CreateFrom("11001_B_0009_A_005_00");
            key.CaPaKeyCrabNotation1.Should().Be("11001_B_0009_A_005_00");
            key.CaPaKeyCrabNotation2.Should().Be("11001B0009/00A005");
            key.VbrCaPaKey.ToString().Should().Be("11001B0009-00A005");
        }

        [Fact]
        public void CapaKeyFromIrregularFormat1Without_ForVbrCaPaKey()
        {
            var key = CaPaKey.CreateFrom("11001B0009-00A005");
            key.CaPaKeyCrabNotation1.Should().Be("11001_B_0009_A_005_00");
            key.CaPaKeyCrabNotation2.Should().Be("11001B0009/00A005");
            key.VbrCaPaKey.ToString().Should().Be("11001B0009-00A005");
        }

        [Fact]
        public void CapaKeyFromIrregularFormat1With_()
        {
            var key = CaPaKey.CreateFrom("11001_B_0213___000_02");
            key.CaPaKeyCrabNotation1.Should().Be("11001_B_0213___000_02");
            key.CaPaKeyCrabNotation2.Should().Be("11001B0213/02_000");
            key.VbrCaPaKey.ToString().Should().Be("11001B0213-02_000");
        }

        [Fact]
        public void CapaKeyFromIrregularFormat1With_ForVbrCaPaKey()
        {
            var key = CaPaKey.CreateFrom("11001B0213-02_000");
            key.CaPaKeyCrabNotation1.Should().Be("11001_B_0213___000_02");
            key.CaPaKeyCrabNotation2.Should().Be("11001B0213/02_000");
            key.VbrCaPaKey.ToString().Should().Be("11001B0213-02_000");
        }

        [Fact]
        public void CapaKeyFromIrregularFormat2With_()
        {
            var key = CaPaKey.CreateFrom("11001B0213/02_000");
            key.CaPaKeyCrabNotation1.Should().Be("11001_B_0213___000_02");
            key.CaPaKeyCrabNotation2.Should().Be("11001B0213/02_000");
            key.VbrCaPaKey.ToString().Should().Be("11001B0213-02_000");
        }

        [Fact]
        public void CapaKeyFromIrregularFormat2With_ForVbrCaPaKey()
        {
            var key = CaPaKey.CreateFrom("11001B0213-02_000");
            key.CaPaKeyCrabNotation1.Should().Be("11001_B_0213___000_02");
            key.CaPaKeyCrabNotation2.Should().Be("11001B0213/02_000");
            key.VbrCaPaKey.ToString().Should().Be("11001B0213-02_000");
        }


        [Fact]
        public void CapaKeyFromIrregularFormat1With0AsEndSeperator()
        {
            var key = CaPaKey.CreateFrom("34372_A_0520_0_000_02");
            key.CaPaKeyCrabNotation1.Should().Be("34372_A_0520_0_000_02");
            key.CaPaKeyCrabNotation2.Should().Be("34372A0520/020000");
            key.VbrCaPaKey.ToString().Should().Be("34372A0520-020000");
        }

        [Fact]
        public void CapaKeyFromIrregularFormat1With0AsEndSeperatorForVbrCaPaKey()
        {
            var key = CaPaKey.CreateFrom("34372A0520-020000");
            key.CaPaKeyCrabNotation1.Should().Be("34372_A_0520_0_000_02");
            key.CaPaKeyCrabNotation2.Should().Be("34372A0520/020000");
            key.VbrCaPaKey.ToString().Should().Be("34372A0520-020000");
        }

        [Fact]
        public void CapaKeyFromIrregularFormat2With0AsEndSeperator()
        {
            var key = CaPaKey.CreateFrom("34372A0520/020000");
            key.CaPaKeyCrabNotation1.Should().Be("34372_A_0520_0_000_02");
            key.CaPaKeyCrabNotation2.Should().Be("34372A0520/020000");
            key.VbrCaPaKey.ToString().Should().Be("34372A0520-020000");
        }

        [Fact]
        public void CapaKeyFromIrregularFormat2With0AsEndSeperatorForVbrCaPaKey()
        {
            var key = CaPaKey.CreateFrom("34372A0520-020000");
            key.CaPaKeyCrabNotation1.Should().Be("34372_A_0520_0_000_02");
            key.CaPaKeyCrabNotation2.Should().Be("34372A0520/020000");
            key.VbrCaPaKey.ToString().Should().Be("34372A0520-020000");
        }

        [Fact]
        public void CapaKeyFromUnknownWith_IrregularFormat()
        {
            var key = CaPaKey.CreateFrom("123456_abcdef");
            key.CaPaKeyCrabNotation1.Should().BeNullOrEmpty();
            key.CaPaKeyCrabNotation2.Should().Be("123456_abcdef");
            key.VbrCaPaKey.ToString().Should().Be("123456_abcdef");
        }

        [Fact]
        public void CapaKeyFromUnknownWith_IrregularFormatForVbrCaPaKey()
        {
            var key = CaPaKey.CreateFrom("123456_abcdef");
            key.CaPaKeyCrabNotation1.Should().BeNullOrEmpty();
            key.CaPaKeyCrabNotation2.Should().Be("123456_abcdef");
            key.VbrCaPaKey.ToString().Should().Be("123456_abcdef");
        }

        [Fact]
        public void CapaKeyWithEqualSizeAsKnownFormatWith_()
        {
            var key = CaPaKey.CreateFrom("1_123456123456abcdef1");
            key.CaPaKeyCrabNotation1.Should().BeNullOrEmpty();
            key.CaPaKeyCrabNotation2.Should().Be("1_123456123456abcdef1");
            key.VbrCaPaKey.ToString().Should().Be("1_123456123456abcdef1");
        }

        [Fact]
        public void CapaKeyWithEqualSizeAsKnownFormatWith_ForVbrCaPaKey()
        {
            var key = CaPaKey.CreateFrom("1_123456123456abcdef1");
            key.CaPaKeyCrabNotation1.Should().BeNullOrEmpty();
            key.CaPaKeyCrabNotation2.Should().Be("1_123456123456abcdef1");
            key.VbrCaPaKey.ToString().Should().Be("1_123456123456abcdef1");
        }

        [Fact]
        public void CapaKeyWithEqualSizeAsKnownFormatWithSlash()
        {
            var key = CaPaKey.CreateFrom("1/123456123456abcdef1");
            key.CaPaKeyCrabNotation1.Should().BeNullOrEmpty();
            key.CaPaKeyCrabNotation2.Should().Be("1/123456123456abcdef1");
            key.VbrCaPaKey.ToString().Should().Be("1-123456123456abcdef1");
        }

        [Fact]
        public void CapaKeyWithEqualSizeAsKnownFormatWithSlashForVbrCaPaKey()
        {
            var key = CaPaKey.CreateFrom("1-123456123456abcdef1");
            key.CaPaKeyCrabNotation1.Should().BeNullOrEmpty();
            key.CaPaKeyCrabNotation2.Should().Be("1/123456123456abcdef1");
            key.VbrCaPaKey.ToString().Should().Be("1-123456123456abcdef1");
        }

        [Fact]
        public void CapaKeyWithEqualSizeAsKnownFormatWithSameAmountOf_()
        {
            var key = CaPaKey.CreateFrom("1_1_2_3_23456abcdef_1");
            key.CaPaKeyCrabNotation1.Should().BeNullOrEmpty();
            key.CaPaKeyCrabNotation2.Should().Be("1_1_2_3_23456abcdef_1");
            key.VbrCaPaKey.ToString().Should().Be("1_1_2_3_23456abcdef_1");
        }

        [Fact]
        public void CapaKeyWithEqualSizeAsKnownFormatWithSameAmountOf_ForVbrCaPaKey()
        {
            var key = CaPaKey.CreateFrom("1_1_2_3_23456abcdef_1");
            key.CaPaKeyCrabNotation1.Should().BeNullOrEmpty();
            key.CaPaKeyCrabNotation2.Should().Be("1_1_2_3_23456abcdef_1");
            key.VbrCaPaKey.ToString().Should().Be("1_1_2_3_23456abcdef_1");
        }

        [Fact]
        public void CapaKeyWithEqualSizeAsKnownFormatWithSameAmountOfSlashes()
        {
            var key = CaPaKey.CreateFrom("1/1/2/3/2345def/1");
            key.CaPaKeyCrabNotation1.Should().BeNullOrEmpty();
            key.CaPaKeyCrabNotation2.Should().Be("1/1/2/3/2345def/1");
            key.VbrCaPaKey.ToString().Should().Be("1-1-2-3-2345def-1");
        }

        [Fact]
        public void CapaKeyWithEqualSizeAsKnownFormatWithSameAmountOfSlashesForVbrCaPaKey()
        {
            var key = CaPaKey.CreateFrom("1-1-2-3-2345def-1");
            key.CaPaKeyCrabNotation1.Should().BeNullOrEmpty();
            key.CaPaKeyCrabNotation2.Should().Be("1/1/2/3/2345def/1");
            key.VbrCaPaKey.ToString().Should().Be("1-1-2-3-2345def-1");
        }

        [Fact]
        public void CapaKeyFromUnknownWithout_IrregularFormat()
        {
            var key = CaPaKey.CreateFrom("123456/abcdef");
            key.CaPaKeyCrabNotation1.Should().BeNullOrEmpty();
            key.CaPaKeyCrabNotation2.Should().Be("123456/abcdef");
            key.VbrCaPaKey.ToString().Should().Be("123456-abcdef");
        }

        [Fact]
        public void CapaKeyFromUnknownWithout_IrregularFormatForVbrCaPaKey()
        {
            var key = CaPaKey.CreateFrom("123456-abcdef");
            key.CaPaKeyCrabNotation1.Should().BeNullOrEmpty();
            key.CaPaKeyCrabNotation2.Should().Be("123456/abcdef");
            key.VbrCaPaKey.ToString().Should().Be("123456-abcdef");
        }

        [Fact]
        public void DistinctSameCaPaKeysDifferentNotationReturnsOne()
        {
            var key = CaPaKey.CreateFrom("11001B0213/02_000");
            key.CaPaKeyCrabNotation1.Should().Be("11001_B_0213___000_02");
            key.CaPaKeyCrabNotation2.Should().Be("11001B0213/02_000");
            key.VbrCaPaKey.ToString().Should().Be("11001B0213-02_000");

            var key2 = CaPaKey.CreateFrom("11001_B_0213___000_02");
            key2.CaPaKeyCrabNotation1.Should().Be("11001_B_0213___000_02");
            key2.CaPaKeyCrabNotation2.Should().Be("11001B0213/02_000");
            key2.VbrCaPaKey.ToString().Should().Be("11001B0213-02_000");

            var list = new List<CaPaKey> {key2, key};

            list.Distinct().Count().Should().Be(1);
        }
    }
}
