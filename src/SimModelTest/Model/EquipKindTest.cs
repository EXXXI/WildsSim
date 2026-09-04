using SimModel.Model;

namespace SimModelTest.Model
{
    /// <summary>
    /// EquipKind(正確にはEquipKindExt)のテスト
    /// </summary>
    public class EquipKindTest
    {
        /// <summary>
        /// Strのテスト
        /// </summary>
        [Theory]
        [InlineData("武器", EquipKind.weapon)]
        [InlineData("頭", EquipKind.head)]
        [InlineData("胴", EquipKind.body)]
        [InlineData("腕", EquipKind.arm)]
        [InlineData("腰", EquipKind.waist)]
        [InlineData("足", EquipKind.leg)]
        [InlineData("装飾品", EquipKind.deco)]
        [InlineData("護石", EquipKind.charm)]
        [InlineData("", EquipKind.error)]
        public void StrTest_normal(string expected, EquipKind equipKind)
        {
            Assert.Equal(expected, equipKind.Str());
        }

        /// <summary>
        /// StrWithColonのテスト
        /// </summary>
        [Theory]
        [InlineData("武器：", EquipKind.weapon)]
        [InlineData("頭：", EquipKind.head)]
        [InlineData("胴：", EquipKind.body)]
        [InlineData("腕：", EquipKind.arm)]
        [InlineData("腰：", EquipKind.waist)]
        [InlineData("足：", EquipKind.leg)]
        [InlineData("装飾品：", EquipKind.deco)]
        [InlineData("護石：", EquipKind.charm)]
        [InlineData("：", EquipKind.error)]
        public void StrWithColonTest_normal(string expected, EquipKind equipKind)
        {
            Assert.Equal(expected, equipKind.StrWithColon());
        }

        /// <summary>
        /// ToEquipKindのテスト
        /// </summary>
        [Theory]
        [InlineData("武器", EquipKind.weapon)]
        [InlineData("頭", EquipKind.head)]
        [InlineData("胴", EquipKind.body)]
        [InlineData("腕", EquipKind.arm)]
        [InlineData("腰", EquipKind.waist)]
        [InlineData("足", EquipKind.leg)]
        [InlineData("脚", EquipKind.leg)]
        [InlineData("装飾品", EquipKind.deco)]
        [InlineData("護石", EquipKind.charm)]
        [InlineData("", EquipKind.error)]
        [InlineData(null, EquipKind.error)]
        public void ToEquipKindTest_normal(string? str, EquipKind expected)
        {
            Assert.Equal(expected, str.ToEquipKind());
        }
    }
}
