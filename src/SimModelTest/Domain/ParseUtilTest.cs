using SimModel.Domain;

namespace SimModelTest.Domain
{
    /// <summary>
    /// ParseUtilのテスト
    /// </summary>
    public class ParseUtilTest
    {
        /// <summary>
        /// Parseのテスト
        /// </summary>
        [Theory]
        [InlineData(1, "1")]
        [InlineData(0, "")]
        [InlineData(0, "1.34")]
        [InlineData(0, "aaaa")]
        [InlineData(0, null)]
        public void ParseTest_normal(int expected, string? str)
        {
            var result = ParseUtil.Parse(str);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Parseのテスト(def付き)
        /// </summary>
        [Theory]
        [InlineData(1, "1", 2)]
        [InlineData(2, "", 2)]
        [InlineData(2, "1.34", 2)]
        [InlineData(2, "aaaa", 2)]
        [InlineData(2, null, 2)]
        public void ParseTest_def(int expected, string? str, int def)
        {
            var result = ParseUtil.Parse(str, def);
            Assert.Equal(expected, result);
        }
    }
}
