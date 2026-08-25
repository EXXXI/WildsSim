using Microsoft.Extensions.DependencyInjection;
using SimModel.Model;

namespace SimModelTest.Model
{
    /// <summary>
    /// SearchConditionのテスト
    /// </summary>
    public class SearchConditionTest : TestDataSetUp
    {
        /// <summary>
        /// SkillCSVのテスト
        /// </summary>
        [Fact]
        public void SkillCSVTest_normal()
        {
            // テストデータ
            List<Skill> skills = [
                new("防具スキル", 2), 
                new("つよの力", 2) { IsFixed = true }
                ];

            // get
            SearchCondition condg = new() { Skills = skills };
            var getted = condg.SkillCSV;
            Assert.Equal("防具スキル,2,つよの力,2固定", getted);

            // set
            SearchCondition conds = new();
            conds.SkillCSV = getted;
            Assert.Equal(skills.Count, conds.Skills.Count);
            foreach (var skill in skills)
            {
                Assert.Contains(conds.Skills, s => s.Name == skill.Name && s.Level == skill.Level);
            }
        }

        /// <summary>
        /// SkillCSVのテスト
        /// </summary>
        [Theory]
        [InlineData("限界突破強化:なし\r\n武器種:大剣, 最低攻撃力:20\r\n防御力:0\r\n火:0,水:0,雷:0,氷:0,龍:0\r\n防具スキルLv2\r\nつよ防具スキル全Lv1(固定)", "0入力")]
        [InlineData("限界突破強化:なし\r\n武器種:大剣, 最低攻撃力:なし\r\n防御力:0\r\n火:0,水:0,雷:0,氷:0,龍:0\r\n防具スキルLv2\r\nつよ防具スキル全Lv1(固定)", "攻撃null")]
        [InlineData("限界突破強化:あり\r\n武器:スロットのみ_0-0-0\r\n防御力:なし\r\n火:なし,水:なし,雷:なし,氷:なし,龍:なし\r\n防具スキルLv2\r\nつよ防具スキル全Lv1", "デフォルト")]
        [InlineData("限界突破強化:あり\r\n武器:ホープブレイドⅠ\r\n防御力:0\r\n火:0,水:0,雷:0,氷:0,龍:0\r\n防具スキルLv2\r\nつよ防具スキル全Lv1", "武器指定")]
        public void DescriptionTest_normal(string expected, string condName)
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            SearchCondition cond = masters.MyConditions.Where(c => c.DispName == condName).First();

            // テスト
            Assert.Equal(expected, cond.Description);
        }


        /// <summary>
        /// コピーコンストラクタのテスト
        /// </summary>
        [Theory]
        [InlineData("0入力")]
        [InlineData("攻撃null")]
        [InlineData("デフォルト")]
        [InlineData("武器指定")]
        public void CopyTest_normal(string condName)
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            SearchCondition cond = masters.MyConditions.Where(c => c.DispName == condName).First();

            // テスト
            SearchCondition copy = new(cond);
            Assert.Equal(cond.Description, copy.Description);
            Assert.NotEqual(cond.DispName, copy.DispName);
        }

        /// <summary>
        /// AddSkillのテスト(正常系)
        /// </summary>
        [Theory]
        [InlineData(true, 3, 1, false, "武器スキル", 1, false)]
        [InlineData(true, 2, 5, false, "防具スキル", 5, false)]
        [InlineData(false, 2, 4, false, "防具スキル", 1, false)]
        [InlineData(true, 2, 1, true, "防具スキル", 1, true)]
        [InlineData(false, 2, 2, true, "つよ防具スキル", 4, false)]
        [InlineData(true, 2, 4, true, "つよ防具スキル", 4, true)]
        [InlineData(false, 2, 4, false, "防具スキル", 4, false)]
        [InlineData(false, 2, 2, true, "つよ防具スキル", 2, true)]
        public void AddSkillTest_normal(
            bool expected, int count, 
            int expectedLevel, bool expectedIsFixed, 
            string skillName, int skillLevel, bool isFixed)
        {
            // テストデータ
            SearchCondition cond = new();
            cond.AddSkill(new("防具スキル", 4));
            cond.AddSkill(new("つよ防具スキル", 2) { IsFixed = true });

            // テスト
            var result = cond.AddSkill(new(skillName, skillLevel) { IsFixed = isFixed});
            Assert.Equal(expected, result);
            Assert.Equal(count, cond.Skills.Count);
            var skill = cond.Skills.Where(s => s.Name == skillName).First();
            Assert.Equal(expectedLevel, skill.Level);
            Assert.Equal(expectedIsFixed, skill.IsFixed);
        }

        /// <summary>
        /// AddSkillのテスト(異常系)
        /// </summary>
        [Fact]
        public void AddSkillTest_null()
        {
            // テストデータ
            SearchCondition cond = new();

            // テスト
            var result = cond.AddSkill(null);
            Assert.False(result);
        }

        /// <summary>
        /// MakeRelatedCharmsのテスト(正常系)
        /// </summary>
        [Fact]
        public void MakeRelatedCharms_normal()
        {
            // テストデータ
            SearchCondition cond = new();
            cond.Skills.Add(new("武器スキル", 3));
            cond.Skills.Add(new("つよ防具スキル頭", 3));
            cond.Skills.Add(new("つよ防具スキル胴", 3));
            List<CharmCombo> combos = new();
            combos.Add(new CharmCombo() { 
                Group1 = 1, Group2 = 2, Group3 = 2, Rare = 8,
                Slot1 = 1, Slot2 = 1, Slot3 = 1,
                SlotType1 = 1, SlotType2 = 0, SlotType3 = 0
            });
            combos.Add(new CharmCombo() { 
                Group1 = 2, Group2 = 0, Group3 = 0, Rare = 7,
                Slot1 = 0, Slot2 = 0, Slot3 = 0,
                SlotType1 = 0, SlotType2 = 0, SlotType3 = 0
            });
            Dictionary<int, List<Skill>> groups = new();
            groups[0] = new();
            groups[1] = [
                new("武器スキル", 2)
                ];
            groups[2] = [
                new("つよ防具スキル頭", 1),
                new("つよ防具スキル胴", 1),
                new("つよ防具スキル腕", 1),
                new("つよ防具スキル腰", 1),
                new("つよ防具スキル足", 1),
                ];

            // テスト
            var result = cond.MakeRelatedCharms(combos, groups);
            Assert.Equal(11, result.Count);
            Assert.DoesNotContain(result, c => c.Skills.Any(s => s.Name == "つよ防具スキル腰"));
        }

        /// <summary>
        /// MakeRelatedCharmsのテスト(異常系・groupsがnull)
        /// 想定していない呼び出し方のため例外で止まることだけを確認
        /// </summary>
        [Fact]
        public void MakeRelatedCharms_nullGroups()
        {
            // テストデータ
            SearchCondition cond = new();
            cond.Skills.Add(new("武器スキル", 3));
            cond.Skills.Add(new("つよ防具スキル頭", 3));
            cond.Skills.Add(new("つよ防具スキル胴", 3));
            List<CharmCombo> combos = new();
            combos.Add(new CharmCombo()
            {
                Group1 = 1,
                Group2 = 2,
                Group3 = 2,
                Rare = 8,
                Slot1 = 1,
                Slot2 = 1,
                Slot3 = 1,
                SlotType1 = 1,
                SlotType2 = 0,
                SlotType3 = 0
            });
            combos.Add(new CharmCombo()
            {
                Group1 = 2,
                Group2 = 0,
                Group3 = 0,
                Rare = 7,
                Slot1 = 0,
                Slot2 = 0,
                Slot3 = 0,
                SlotType1 = 0,
                SlotType2 = 0,
                SlotType3 = 0
            });

            // テスト
            var ex = Assert.ThrowsAny<Exception>(() => cond.MakeRelatedCharms(combos, null));
        }

        /// <summary>
        /// MakeRelatedCharmsのテスト(異常系・combosがnull)
        /// 想定していない呼び出し方のため例外で止まることだけを確認
        /// </summary>
        [Fact]
        public void MakeRelatedCharms_nullCombos()
        {
            // テストデータ
            SearchCondition cond = new();
            cond.Skills.Add(new("武器スキル", 3));
            cond.Skills.Add(new("つよ防具スキル頭", 3));
            cond.Skills.Add(new("つよ防具スキル胴", 3));
            Dictionary<int, List<Skill>> groups = new();
            groups[0] = new();
            groups[1] = [
                new("武器スキル", 2)
                ];
            groups[2] = [
                new("つよ防具スキル頭", 1),
                new("つよ防具スキル胴", 1),
                new("つよ防具スキル腕", 1),
                new("つよ防具スキル腰", 1),
                new("つよ防具スキル足", 1),
                ];

            // テスト
            var ex = Assert.ThrowsAny<Exception>(() => cond.MakeRelatedCharms(null, groups));
        }

        /// <summary>
        /// MakeRelatedArtiansTestのテスト
        /// </summary>
        [Fact]
        public void MakeRelatedArtiansTest_normal()
        {
            // テストデータ
            SearchCondition cond = new();
            cond.Skills.Add(new("つよの力", 2));
            cond.Skills.Add(new("よわの力", 2));
            cond.Skills.Add(new("つよグループ", 3));
            cond.Skills.Add(new("強欲グループ", 3));

            // テスト
            var result = cond.MakeRelatedArtians();
            Assert.Equal(5, result.Count);
            Assert.DoesNotContain(result, w => w.Skills.Any(s => s.Name == "強欲グループ"));

        }
    }
}
