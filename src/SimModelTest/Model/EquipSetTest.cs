using Microsoft.Extensions.DependencyInjection;
using SimModel.Model;

namespace SimModelTest.Model
{
    /// <summary>
    /// EquipSetのテスト
    /// </summary>
    public class EquipSetTest : TestDataSetUp
    {
        /// <summary>
        /// 防御・耐性値のテスト
        /// </summary>
        [Theory]
        [InlineData(80, 144, -25, -25, -25, -25, -40, "歯抜けセット")]
        [InlineData(75, 125, 0, 0, 0, 0, 0, "ワンセット")]
        public void DefRegistTest_normal(
            int mindef, int maxDef, 
            int fire, int water, int thunder, int ice, int dragon,
            string setName)
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            EquipSet set = masters.MySets.Where(s => s.Name == setName).First();

            // テスト
            Assert.Equal(mindef, set.Mindef);
            Assert.Equal(maxDef, set.Maxdef);
            Assert.Equal(fire, set.Fire);
            Assert.Equal(water, set.Water);
            Assert.Equal(thunder, set.Thunder);
            Assert.Equal(ice, set.Ice);
            Assert.Equal(dragon, set.Dragon);
        }

        /// <summary>
        /// Skillsのテスト
        /// </summary>
        [Fact]
        public void Skills_normal()
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            EquipSet set = masters.MySets.Where(s => s.Name == "つよセット").First();

            // テスト
            Assert.Equal(6, set.Skills.Where(s => s.Name == "防具スキル").First().Level);
            Assert.Equal(3, set.Skills.Where(s => s.Name == "つよグループ").First().Level);
            Assert.Equal(4, set.Skills.Where(s => s.Name == "つよの力").First().Level);
        }

        /// <summary>
        /// GlpkRowNameのテスト
        /// </summary>
        [Fact]
        public void GlpkRowName_normal()
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            EquipSet set = masters.MySets.Where(s => s.Name == "歯抜けセット").First();

            // テスト
            Assert.Equal("スロットのみ_0-0-0,つよ頭,つよ胴,つよ腕,つよ腰,,テスト護石Ⅰ", set.GlpkRowName);
        }

        /// <summary>
        /// ExistingEquipsWithOutDecosのテスト
        /// </summary>
        [Theory]
        [InlineData(6, "歯抜けセット")]
        [InlineData(7, "ワンセット")]
        public void ExistingEquipsWithOutDecos_normal(int expectedCount, string setName)
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            EquipSet set = masters.MySets.Where(s => s.Name == setName).First();

            // テスト
            var equips = set.ExistingEquipsWithOutDecos();
            Assert.Equal(expectedCount, equips.Count);
        }

        /// <summary>
        /// DecoNameCSVのテスト
        /// DecoNameCSVMultiLineもここで確認
        /// </summary>
        [Fact]
        public void DecoNameCSV_normal()
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            EquipSet set = masters.MySets.Where(s => s.Name == "ワンセット").First();

            // テスト
            Assert.Equal("防二珠【２】,防二珠【２】,防二珠【２】,防二珠【２】,防二珠【２】", set.DecoNameCSV);
            Assert.Equal("防二珠【２】,\n防二珠【２】,防二珠【２】,\n防二珠【２】,防二珠【２】", set.DecoNameCSVMultiLine);
        }

        /// <summary>
        /// SkillsDispのテスト
        /// SkillsDispMultiLineもここで確認
        /// </summary>
        [Fact]
        public void SkillsDisp_normal()
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            EquipSet set = masters.MySets.Where(s => s.Name == "ワンセット").First();

            // テスト
            Assert.Equal("防具スキルLv6, スロ2防具スキルLv5, 強欲Ⅱ(強欲の力Lv4), 激運(強欲グループLv3), つよ防具スキル頭Lv1, つよ防具スキル胴Lv1, つよ防具スキル腕Lv1, つよ防具スキル腰Lv1, つよ防具スキル足Lv1", set.SkillsDisp);
            Assert.Equal("防具スキルLv6,スロ2防具スキルLv5,強欲Ⅱ(強欲の力Lv4),\n激運(強欲グループLv3),つよ防具スキル頭Lv1,つよ防具スキル胴Lv1,\nつよ防具スキル腕Lv1,つよ防具スキル腰Lv1,つよ防具スキル足Lv1", set.SkillsDispMultiLine);
        }

        /// <summary>
        /// Descriptionのテスト
        /// </summary>
        [Theory]
        [InlineData("防御:75→125,火:0,水:0,雷:0,氷:0,龍:0\n武器：スロットのみ_0-0-0,0-0-0\n頭：セット頭,2-1-0→3-2-1\n胴：セット胴,2-1-0→3-2-1\n腕：セット腕,2-1-0→3-2-1\n腰：セット腰,2-1-0→3-2-1\n足：セット足,2-1-0→3-2-1\n護石：テスト護石Ⅰ,0-0-0\n装飾品：防二珠【２】,防二珠【２】,防二珠【２】,防二珠【２】,防二珠【２】\n-----------\n防具スキルLv6\nスロ2防具スキルLv5\n強欲Ⅱ(強欲の力Lv4)\n激運(強欲グループLv3)\nつよ防具スキル頭Lv1\nつよ防具スキル胴Lv1\nつよ防具スキル腕Lv1\nつよ防具スキル腰Lv1\nつよ防具スキル足Lv1", "ワンセット")]
        [InlineData("防御:75→175(限界突破),火:0,水:0,雷:0,氷:0,龍:0\n武器：スロットのみ_0-0-0,0-0-0\n頭：セット頭,2-1-0→3-2-1\n胴：セット胴,2-1-0→3-2-1\n腕：セット腕,2-1-0→3-2-1\n腰：セット腰,2-1-0→3-2-1\n足：セット足,2-1-0→3-2-1\n護石：テスト護石Ⅰ,0-0-0\n装飾品：防三珠【３】,防三珠【３】,防三珠【３】,防三珠【３】,防三珠【３】\n-----------\n防具スキルLv6\nスロ3防具スキルLv5\n強欲Ⅱ(強欲の力Lv4)\n激運(強欲グループLv3)\nつよ防具スキル頭Lv1\nつよ防具スキル胴Lv1\nつよ防具スキル腕Lv1\nつよ防具スキル腰Lv1\nつよ防具スキル足Lv1", "ワンセット限界突破")]
        public void Description_normal(string expected, string setName)
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            EquipSet set = masters.MySets.Where(s => s.Name == setName).First();

            // テスト
            Assert.Equal(expected, set.Description);
        }
    }
}
