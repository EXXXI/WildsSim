using Microsoft.Extensions.DependencyInjection;
using SimModel.Domain;
using SimModel.Model;

namespace SimModelTest.Domain
{
    /// <summary>
    /// CharmAppraiserのテスト
    /// </summary>
    public class CharmAppraiserTest : TestDataSetUp
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CharmAppraiserTest() : base()
        {
            // Base(TestDataSetUp)で別々のDIコンテナを生成し、テストデータを準備
        }

        /// <summary>
        /// IsLeftUpperの通常の挙動を確認
        /// </summary>
        [Theory]
        [MemberData(nameof(IsLeftUpperTestData))]
        public void IsLeftUpperTest_normal(bool expected, Equipment left, Equipment right, bool useDecos)
        {
            // DIコンテナから取得
            var appraiser = ServiceProvider.GetRequiredService<CharmAppraiser>();

            // テスト
            bool actual = appraiser.IsLeftUpper(left, right, useDecos);
            Assert.Equal(expected, actual);

        }

        /// <summary>
        /// 第一引数がnull
        /// 想定していない使い方のため、止まりさえすればOK
        /// </summary>
        [Fact]
        public void IsLeftUpperTest_leftNull()
        {
            // DIコンテナから取得
            var appraiser = ServiceProvider.GetRequiredService<CharmAppraiser>();

            // テスト
            var ex = Assert.ThrowsAny<Exception>(() => appraiser.IsLeftUpper(null, new Equipment()));
        }

        /// <summary>
        /// 第二引数がnull
        /// 想定していない使い方のため、止まりさえすればOK
        /// </summary>
        [Fact]
        public void IsLeftUpperTest_rightNull()
        {
            // DIコンテナから取得
            var appraiser = ServiceProvider.GetRequiredService<CharmAppraiser>();

            // テスト
            var ex = Assert.ThrowsAny<Exception>(() => appraiser.IsLeftUpper(new Equipment(), null));
        }

        /// <summary>
        /// HasUpperCharmの通常の挙動を確認
        /// </summary>
        [Theory]
        [MemberData(nameof(HasUpperCharmTestData))]
        public void HasUpperCharmTest_normal(bool expected, Equipment charm, bool useDecos)
        {
            // DIコンテナから取得
            var appraiser = ServiceProvider.GetRequiredService<CharmAppraiser>();

            // テスト
            Equipment? upCharm = appraiser.HasUpperCharm(charm, useDecos);
            if (expected)
            {
                Assert.NotNull(upCharm);
                // 戻り値の護石が上位互換であることを確認する
                // IsLeftUpperの挙動はIsLeftUpperのテストで確認しているので、ここでは検証しない
                Assert.True(appraiser.IsLeftUpper(upCharm, charm, useDecos));
            }
            else
            {
                Assert.Null(upCharm);
            }
        }

        /// <summary>
        /// 引数がnull
        /// 想定していない使い方のため、止まりさえすればOK
        /// </summary>
        [Fact]
        public void HasUpperCharmTest_null()
        {
            // DIコンテナから取得
            var appraiser = ServiceProvider.GetRequiredService<CharmAppraiser>();

            // テスト
            var ex = Assert.ThrowsAny<Exception>(() => appraiser.HasUpperCharm(null));
        }

        /// <summary>
        /// IsLeftUpperのテストデータ
        /// </summary>
        public static TheoryData<bool, Equipment, Equipment, bool> IsLeftUpperTestData
        {
            get
            {
                Equipment baseCharm = new Equipment()
                {
                    Name = "_baseCharm",
                    Kind = EquipKind.charm,
                    Skills = new List<Skill>() { new Skill("防具スキル", 1) }
                };
                Equipment baseCharm2 = new Equipment()
                {
                    Name = "_baseCharm2",
                    Kind = EquipKind.charm,
                    Skills = new List<Skill>() { new Skill("防具スキル", 1) }
                };
                Equipment s1 = new Equipment()
                {
                    Name = "_s1",
                    Kind = EquipKind.charm,
                    Skills = new List<Skill>() { new Skill("防具スキル", 1) },
                    Slot1 = 1,
                    Slot2 = 1
                };
                Equipment s1skill = new Equipment()
                {
                    Name = "_s1skill",
                    Kind = EquipKind.charm,
                    Skills = new List<Skill>() { new Skill("防具スキル", 1), new Skill("スロ1防具スキル", 2) }
                };
                Equipment s2 = new Equipment()
                {
                    Name = "_s2",
                    Kind = EquipKind.charm,
                    Skills = new List<Skill>() { new Skill("防具スキル", 1) },
                    Slot1 = 2,
                    Slot2 = 1

                };

                // bool expected, Equipment left, Equipment right, bool useDecos
                TheoryData<bool, Equipment, Equipment, bool> testData = new()
                {
                    // 同値はtrue
                    { true, baseCharm, baseCharm2, true },
                    { true, baseCharm2, baseCharm, true },

                     // スロットが多い方が上位互換
                    { true, s1, baseCharm, true},
                    { false, baseCharm, s1, true},

                    // スロットが大きい方が上位互換
                    { true, s2, s1, true},
                    { false, s1, s2, true},

                    // スキルの多い方が上位互換
                    { true, s1skill, baseCharm, true},
                    { false, baseCharm, s1skill, true},

                    // 装飾品によって同じ性能にできる場合、汎用性の高い方(装飾品を使う方)が上位互換
                    { true, s1, s1skill, true},
                    { false, s1skill, s1, true},

                    // 装飾品を使わずに計算する
                    { false, s1, s1skill, false},
                    { false, s1skill, s1, false}
                };

                return testData;
            }
        }

        /// <summary>
        /// HasUpperCharmのテストデータ
        /// </summary>
        public static TheoryData<bool, Equipment, bool> HasUpperCharmTestData
        {
            get
            {
                Equipment s1_3 = new Equipment()
                {
                    Name = "_s1_3",
                    Kind = EquipKind.charm,
                    Skills = new List<Skill>() { new Skill("スロ1防具スキル", 3) }
                };
                Equipment s2_3 = new Equipment()
                {
                    Name = "_s2_3",
                    Kind = EquipKind.charm,
                    Skills = new List<Skill>() { new Skill("スロ2防具スキル", 3) }
                };
                Equipment shortSkill1 = new Equipment()
                {
                    Name = "_short1",
                    Kind = EquipKind.charm,
                    Skills = new List<Skill>() { new Skill("スロ1不足スキル", 1) }
                };
                Equipment shortSkill3 = new Equipment()
                {
                    Name = "_short3",
                    Kind = EquipKind.charm,
                    Skills = new List<Skill>() { new Skill("スロ1不足スキル", 3) }
                };


                // bool expected, Equipment charm, bool useDecos
                TheoryData<bool, Equipment, bool> testData = new()
                {
                    // 他の護石で再現できる
                    { true, s1_3, true },

                    // 他の護石で再現できない
                    { false, s2_3, true },

                     // 装飾品所持数は無視される
                    { true, shortSkill3, true},

                     // 装飾品なしの比較ができる
                    { true, shortSkill1, false},
                    { false, shortSkill3, false},
                };

                return testData;
            }
        }
    }
}
