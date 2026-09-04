using SimModel.Model;

namespace SimModelTest.Model
{
    /// <summary>
    /// Decoのテスト
    /// </summary>
    public class DecoTest : TestDataSetUp
    {
        /// <summary>
        /// DexoCategoryのテスト(正常系・初期状態)
        /// </summary>
        [Fact]
        public void DecoCategory_init()
        {
            Deco deco = new();
            var decoCategory = deco.DecoCategory;

            Assert.Equal("未分類", decoCategory);
        }

        /// <summary>
        /// DexoCategoryのテスト(正常系・スキルあり未設定)
        /// </summary>
        [Fact]
        public void DecoCategory_skill()
        {
            Deco deco = new();
            deco.Skills.Add(new("防具スキル", 1));
            var decoCategory = deco.DecoCategory;

            Assert.Equal("防具スキル(テスト)", decoCategory);
        }

        /// <summary>
        /// DexoCategoryのテスト(正常系・明示)
        /// </summary>
        [Fact]
        public void DecoCategory_setted()
        {
            Deco deco = new();
            deco.Skills.Add(new("防具スキル", 1));
            deco.DecoCategory = "テスト";
            var decoCategory = deco.DecoCategory;

            Assert.Equal("テスト", decoCategory);
        }
    }
}
