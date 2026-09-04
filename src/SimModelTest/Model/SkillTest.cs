using SimModel.Model;

namespace SimModelTest.Model
{
    /// <summary>
    /// Skillのテスト
    /// </summary>
    public class SkillTest : TestDataSetUp
    {
        /// <summary>
        /// コンストラクタのテスト
        /// </summary>
        [Theory]
        [InlineData("武器スキル", 1, "武器スキル(テスト)", false, "武器スキル", 1, null, null)]
        [InlineData("つよグループ", 2, "グループスキル", true, "つよグループ", 2, null, null)]
        [InlineData("dummy", 99, "未分類", false, "dummy", 99, null, null)]
        [InlineData("武器スキル", 1, "testCategory", false, "武器スキル", 1, "testCategory", null)]
        [InlineData("つよグループ", 2, "グループスキル", false, "つよグループ", 2, null, false)]
        // TODO: 名前nullや負のLevelも現状は許可。必要に応じて変更
        [InlineData(null, -1, "未分類", false, null, -1, null, null)] 
        public void SkillTest_normal(
            string? expectedName, int expectedLevel, string expectedCategory, bool ExpectedCanWithArtian, 
            string? skillName, int skillLevel, string? category, bool? canWithArtian)
        {
            // テストデータ
            Skill skill = new(skillName, skillLevel, category, canWithArtian);

            // テスト
            Assert.Equal(expectedName, skill.Name);
            Assert.Equal(expectedLevel, skill.Level);
            Assert.Equal(expectedCategory, skill.Category);
            Assert.Equal(ExpectedCanWithArtian, skill.CanWithArtian);
        }

        /// <summary>
        /// プロパティのテスト
        /// </summary>
        [Theory]
        [InlineData(6, "防具スキルLv2", "防具スキル", 2)]
        [InlineData(4, "つよⅠ(つよの力Lv2)", "つよの力", 2)]
        [InlineData(0, "dummyLv2", "dummy", 2)]
        [InlineData(0, "", null, 2)]
        [InlineData(6, "", "防具スキル", 0)]
        public void PropertyTest_normal(
            int expectedMaxLevel, string expectedDescription, 
            string? skillName, int skillLevel)
        {
            // テストデータ
            Skill skill = new(skillName, skillLevel);

            // テスト
            Assert.Equal(expectedMaxLevel, skill.MaxLevel);
            Assert.Equal(expectedDescription, skill.Description);
        }

        [Theory]
        [InlineData(false, "防具スキル", 2, null)]
        [InlineData(true, "つよの力", 1, null)]
        [InlineData(false, "つよの力", 3, 2)]
        [InlineData(true, "つよの力", 2, 3)]
        [InlineData(false, "つよグループ", 3, null)]
        [InlineData(true, "つよグループ", 2, null)]
        public void IsHideLevel_normal(bool expected, string? skillName, int skillLevel, int? argLevel)
        {
            // テストデータ
            Skill skill = new(skillName, skillLevel);

            // テスト
            var result = skill.IsHideLevel(argLevel);
            Assert.Equal(expected, result);
        }
    }
}
