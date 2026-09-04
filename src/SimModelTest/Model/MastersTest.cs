using Microsoft.Extensions.DependencyInjection;
using SimModel.Model;

namespace SimModelTest.Model
{
    /// <summary>
    /// Mastersのテスト
    /// </summary>
    public class MastersTest :TestDataSetUp
    {
        /// <summary>
        /// AllEquipmentsのテスト
        /// </summary>
        [Fact]
        public void AllEquipMentsTest_normal()
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            Weapon temp = new() { Name = "allEquipTest" };

            // テスト
            Assert.DoesNotContain(temp, masters.AllEquipments);
            masters.Artians.Add(temp);
            Assert.DoesNotContain(temp, masters.AllEquipments);
            masters.ClearAllEquipmentsCache();
            Assert.Contains(temp, masters.AllEquipments);
        }

        /// <summary>
        /// IsSkillNameとSkillMaxLevelのテスト
        /// </summary>
        [Theory]
        [InlineData(true, 6, "防具スキル")]
        [InlineData(true, 4, "つよの力")]
        [InlineData(false, 0, "")]
        [InlineData(false, 0, "dummy")]
        [InlineData(false, 0, null)]
        public void SkillTest_normal(bool isSkill, int maxLevel, string? skillName)
        {
            // テスト
            Assert.Equal(isSkill, Masters.IsSkillName(skillName));
            Assert.Equal(maxLevel, Masters.SkillMaxLevel(skillName));
        }

    }
}
