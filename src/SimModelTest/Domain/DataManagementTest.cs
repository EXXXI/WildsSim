using Microsoft.Extensions.DependencyInjection;
using SimModel.Config;
using SimModel.Domain;
using SimModel.Model;

namespace SimModelTest.Domain
{
    /// <summary>
    /// DataManagementのテスト
    /// </summary>
    public class DataManagementTest : TestDataSetUp
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DataManagementTest() : base() 
        {
            // Base(TestDataSetUp)で別々のDIコンテナを生成し、テストデータを準備
        }   

        #region LoadData

        /// <summary>
        /// LoadDataのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadDataTest_normal()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();
            var logicConfig = ServiceProvider.GetRequiredService<LogicConfig>();
            
            // テスト
            // 既にコンストラクタ内で呼び出されてはいるが、冪等性確認も兼ねて再度呼び出し
            dataManagement.LoadData();

            // 詳細なデータの確認はFileOperationのテストで実施する
            // ここでは空でないことだけ確認する
            Assert.NotEmpty(Masters.DefUpgrades);
            Assert.NotEmpty(Masters.Heads);
            Assert.NotEmpty(Masters.Bodys);
            Assert.NotEmpty(Masters.Arms);
            Assert.NotEmpty(Masters.Waists);
            Assert.NotEmpty(Masters.Legs);
            Assert.NotEmpty(Masters.Charms);
            Assert.NotEmpty(masters.Decos);
            Assert.NotEmpty(Masters.Weapons);
            Assert.NotEmpty(Masters.Skills);
            Assert.NotEmpty(Masters.ShiningCharmCombos);
            Assert.NotEmpty(Masters.ShiningCharmGroups);
            Assert.NotEmpty(masters.AdditionalCharms);
            Assert.NotEmpty(masters.Artians);
            Assert.NotEmpty(masters.Cludes);
            Assert.NotEmpty(masters.RecentSkillNames);
            Assert.NotEmpty(masters.MyConditions);
            Assert.NotEmpty(masters.MySets);
            // マイセット情報反映のために保存が行われていることを確認
            Assert.Single(WriteLog["save/additionalCharm.csv"]);
            Assert.Single(WriteLog["save/artian.csv"]);
            // 未実装スキルが除外されていることを確認
            Assert.DoesNotContain(Masters.Skills, s => s.Name == "未実装スキル");
            // 下位互換護石の計算が行われていることを確認
            var armorCharm = Assert.Single(masters.AdditionalCharms, charm => charm.DispName == "3(防)-2(防)-1(防)");
            Assert.NotNull(armorCharm.Upper);
            Assert.Equal("3(両)-2(両)-1(両)", armorCharm.Upper.Value.Item1.DispName);
            Assert.True(armorCharm.Upper.Value.Item2);
            var sameCharm = masters.AdditionalCharms.Where(c => c.DispName == "未実装スキルLv3, 0-0-0").First();
            Assert.NotNull(sameCharm.Upper); 
            Assert.False(sameCharm.Upper.Value.Item2);
        }

        /// <summary>
        /// LoadDataのテスト(正常系・下位互換検出なし)
        /// </summary>
        [Fact]
        public void LoadDataTest_noCalcUppser()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();
            var logicConfig = ServiceProvider.GetRequiredService<LogicConfig>();

            // テスト
            // 既にFixture内で呼び出されてはいるが、冪等性確認も兼ねて再度呼び出し
            logicConfig.UseCalcUpperCharm = false;
            dataManagement.LoadData();

            // 下位互換護石の計算が行われていないことを確認
            var armorCharm = Assert.Single(masters.AdditionalCharms, charm => charm.DispName == "3(防)-2(防)-1(防)");
            Assert.Null(armorCharm.Upper);
        }

        #endregion

        #region Clude関連

        /// <summary>
        /// AddExcludeのテスト(正常系)
        /// </summary>
        [Theory]
        [InlineData("よわ頭", true)] // 単純な追加
        [InlineData("つよ頭", true)] // 書き換え
        [InlineData("", false)] // 空文字は追加されない
        [InlineData("存在しない装備名", false)] // 存在しない装備名は追加されない
        [InlineData("スロットのみ_3-3-3", false)] // スロット指定用武器は追加されない
        [InlineData(null, false)] // nullは追加されない
        public void AddExclude_normal(string? name, bool isEffective)
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // 既存データテスト用に追加
            masters.Cludes.Add(new() { Name = "つよ頭", Kind = CludeKind.include });

            // テスト
            var result = dataManagement.AddExclude(name);
            if (isEffective)
            {
                var clude = Assert.Single(masters.Cludes, c => c.Name == name);
                Assert.Equal(clude, result);
                Assert.Equal(CludeKind.exclude, clude.Kind);
                Assert.Single(WriteLog["save/clude.csv"]);
            }
            else
            {
                Assert.Null(result);
                Assert.DoesNotContain(masters.Cludes, c => c.Name == name && c.Kind == CludeKind.exclude);
            }
        }

        /// <summary>
        /// AddIncludeのテスト(正常系)
        /// </summary>
        [Theory]
        [InlineData("よわ頭", true)] // 単純な追加
        [InlineData("よわ腕", true)] // 同部位の固定が存在する場合
        [InlineData("つよ頭", true)] // 書き換え
        [InlineData("", false)] // 空文字は追加されない
        [InlineData("存在しない装備名", false)] // 存在しない装備名は追加されない
        [InlineData("スキルつき大剣", false)] // 武器は追加されない
        [InlineData("武一珠【１】", false)] // 装飾品は追加されない
        [InlineData(null, false)] // nullは追加されない
        public void AddInclude_normal(string? name, bool isEffective)
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // 既存データテスト用に追加
            masters.Cludes.Add(new() { Name = "つよ腕", Kind = CludeKind.include });
            masters.Cludes.Add(new() { Name = "つよ頭", Kind = CludeKind.exclude });

            // テスト
            var result = dataManagement.AddInclude(name);
            if (isEffective)
            {
                var clude = Assert.Single(masters.Cludes, c => c.Name == name);
                Assert.Equal(clude, result);
                Assert.Equal(CludeKind.include, clude.Kind);
                Assert.Single(WriteLog["save/clude.csv"]);
            }
            else
            {
                Assert.Null(result);
                Assert.DoesNotContain(masters.Cludes, c => c.Name == name && c.Kind == CludeKind.exclude);
            }
            // 同一部位の固定が2つ無いことを確認(テストデータにしていた腕を確認)
            Assert.Single(masters.Cludes, c => masters.GetEquipByName(c.Name)?.Kind == EquipKind.arm && c.Kind == CludeKind.include); // includeは残っていないことを確認
        }

        /// <summary>
        /// DeleteCludeのテスト(正常系)
        /// doSaveがfalseのテストは他からの呼び出しで確認するためここでは実施しない
        /// </summary>
        [Theory]
        [InlineData("つよ腕", true)] // 固定の削除
        [InlineData("つよ頭", true)] // 除外の削除
        [InlineData("よわ頭", false)] // 空撃ち
        [InlineData("未実装頭", true)] // 除外固定にのみ存在する装備
        [InlineData("", false)] // 空文字は無視
        [InlineData("存在しない装備名", false)] // 存在しない装備名は無視
        [InlineData(null, false)] // nullは無視
        public void DeleteClude_normal(string? name, bool isEffective)
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // 既存データテスト用に追加
            masters.Cludes.Add(new() { Name = "つよ腕", Kind = CludeKind.include });
            masters.Cludes.Add(new() { Name = "つよ頭", Kind = CludeKind.exclude });
            int cludeCount = masters.Cludes.Count();

            // テスト
            dataManagement.DeleteClude(name);
            Assert.DoesNotContain(masters.Cludes, c => c.Name == name);
            if (isEffective)
            {
                Assert.Equal(cludeCount - 1, masters.Cludes.Count());
            }
            else
            {
                Assert.Equal(cludeCount, masters.Cludes.Count());
            }
            Assert.Single(WriteLog["save/clude.csv"]);
        }

        /// <summary>
        /// DeleteAllCludeのテスト(正常系)
        /// doSaveがfalseのテストは他からの呼び出しで確認するためここでは実施しない
        /// </summary>
        [Fact]
        public void DeleteAllClude_normal()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // 既存データテスト用に追加
            masters.Cludes.Add(new() { Name = "つよ腕", Kind = CludeKind.include });
            masters.Cludes.Add(new() { Name = "つよ頭", Kind = CludeKind.exclude });

            // テスト
            dataManagement.DeleteAllClude();
            Assert.Empty(masters.Cludes);
            Assert.Single(WriteLog["save/clude.csv"]);
        }

        /// <summary>
        /// DeleteAllArmorCludeのテスト(正常系)
        /// doSaveがfalseのテストは他からの呼び出しで確認するためここでは実施しない
        /// </summary>
        [Fact]
        public void DeleteAllArmorClude_normal()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // 既存データテスト用に追加
            masters.Cludes.Add(new() { Name = "つよ腕", Kind = CludeKind.include });
            masters.Cludes.Add(new() { Name = "スキルつき大剣", Kind = CludeKind.exclude });

            // テスト
            dataManagement.DeleteAllArmorClude();
            Assert.DoesNotContain(masters.Cludes, c =>
                masters.GetEquipByName(c.Name)?.Kind != EquipKind.weapon &&
                masters.GetEquipByName(c.Name)?.Kind != null);
            Assert.Contains(masters.Cludes, c =>
                masters.GetEquipByName(c.Name)?.Kind == EquipKind.weapon);
            Assert.Contains(masters.Cludes, c =>
                masters.GetEquipByName(c.Name)?.Kind == null);

            Assert.Single(WriteLog["save/clude.csv"]);
        }

        /// <summary>
        /// DeleteAllWeaponCludeのテスト(正常系)
        /// doSaveがfalseのテストは他からの呼び出しで確認するためここでは実施しない
        /// </summary>
        [Fact]
        public void DeleteAllWeaponClude_normal()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // 既存データテスト用に追加
            masters.Cludes.Add(new() { Name = "つよ腕", Kind = CludeKind.include });
            masters.Cludes.Add(new() { Name = "スキルつき大剣", Kind = CludeKind.exclude });

            // テスト
            dataManagement.DeleteAllWeaponClude();
            Assert.DoesNotContain(masters.Cludes, c =>
                masters.GetEquipByName(c.Name)?.Kind == EquipKind.weapon);
            Assert.Contains(masters.Cludes, c =>
                masters.GetEquipByName(c.Name)?.Kind != EquipKind.weapon &&
                masters.GetEquipByName(c.Name)?.Kind != null);
            Assert.Contains(masters.Cludes, c =>
                masters.GetEquipByName(c.Name)?.Kind == null);

            Assert.Single(WriteLog["save/clude.csv"]);
        }

        #endregion

        #region MySet関連

        /// <summary>
        /// AddMySetのテストデータ
        /// </summary>
        public static TheoryData<bool, EquipSet> AddMySetTestData
        {
            get
            {
                // bool isEffective, EquipSet condition 
                TheoryData<bool, EquipSet> testData = new()
                {
                    // 通常
                    { true, new() },
                    { true, new() { Name = "testData" } },
                    // 仮想装備
                    { false, new() { Charm = new() { Name = "virtual" } } },
                    { false, new() { Weapon = new() { Name = "virtual" } } },
                     // null
                    { false, null }
                };

                return testData;
            }
        }

        /// <summary>
        /// AddMySetのテスト
        /// </summary>
        [Theory]
        [MemberData(nameof(AddMySetTestData))]
        public void AddMySet_normal(bool isEffective, EquipSet set)
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();
            var logicConfig = ServiceProvider.GetRequiredService<LogicConfig>();

            // 付けられるべき名前
            string expectedName;
            string? setName = set?.Name;
            if (string.IsNullOrEmpty(setName))
            {
                expectedName = logicConfig.DefaultMySetName;
            }
            else
            {
                expectedName = setName;
            }

            // テスト
            var result = dataManagement.AddMySet(set);
            if (isEffective)
            {
                Assert.Contains(set, masters.MySets);
                Assert.Equal(set, result);
                Assert.Equal(expectedName, set.Name);
                Assert.Single(WriteLog["save/myset.csv"]);
                Assert.Single(WriteLog["save/additionalCharm.csv"]);
                Assert.Single(WriteLog["save/artian.csv"]);
            }
            else
            {
                Assert.DoesNotContain(set, masters.MySets);
                Assert.Null(result);
                Assert.Empty(WriteLog);
            }
        }

        /// <summary>
        /// DeleteMySetのテスト(正常系)
        /// </summary>
        [Fact]
        public void DeleteMySet_normal()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            var set = masters.MySets[1];
            var count = masters.MySets.Count;

            // テスト
            dataManagement.DeleteMySet(set);
            Assert.DoesNotContain(set, masters.MySets);
            Assert.Equal(count - 1, masters.MySets.Count);
            Assert.Single(WriteLog["save/myset.csv"]);
            Assert.Single(WriteLog["save/additionalCharm.csv"]);
            Assert.Single(WriteLog["save/artian.csv"]);
        }

        /// <summary>
        /// DeleteMySetのテスト(異常系・存在しないデータ)
        /// </summary>
        [Fact]
        public void DeleteMySet_notExist()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            EquipSet set = new();
            int count = masters.MySets.Count;

            // テスト
            dataManagement.DeleteMySet(set);
            Assert.DoesNotContain(set, masters.MySets);
            Assert.Equal(count, masters.MySets.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// DeleteMySetのテスト(異常系・null)
        /// </summary>
        [Fact]
        public void DeleteMySet_null()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            int count = masters.MySets.Count;

            // テスト
            dataManagement.DeleteMySet(null);
            Assert.Equal(count, masters.MySets.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// MoveMySetのテスト
        /// </summary>
        [Theory]
        [InlineData(true, 1, 2)]
        [InlineData(false, 1, 200)]
        [InlineData(false, 100, 2)]
        [InlineData(false, -1, 2)]
        [InlineData(false, 1, -2)]
        [InlineData(false, 1, 1)]
        public void MoveMySet_normal(bool isEffective, int dropIndex, int targetIndex)
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            EquipSet? dropSet = null;
            if (isEffective)
            {
                dropSet = masters.MySets[dropIndex];
            }
            int count = masters.MySets.Count;

            // テスト
            dataManagement.MoveMySet(dropIndex, targetIndex);
            Assert.Equal(count, masters.MySets.Count);
            if (isEffective)
            {
                Assert.Equal(dropSet, masters.MySets[targetIndex]);
                Assert.Single(WriteLog["save/myset.csv"]);
                Assert.Single(WriteLog["save/additionalCharm.csv"]);
                Assert.Single(WriteLog["save/artian.csv"]);
            }
            else
            {
                Assert.Empty(WriteLog);
            }
        }

        /// <summary>
        /// ChangeNameOfMySetのテスト(正常系)
        /// </summary>
        [Fact]
        public void ChangeNameOfMySet_normal()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            var set = masters.MySets[1];
            var oldName = set.Name;
            var name = "RenameTest";
            var count = masters.MySets.Count;

            // テスト
            dataManagement.ChangeNameOfMySet(name, set);
            Assert.Single(masters.MySets, set => set.Name == name);
            Assert.DoesNotContain(masters.MySets, set => set.Name == oldName);
            Assert.Equal(count, masters.MySets.Count);
            Assert.Single(WriteLog["save/myset.csv"]);
            Assert.Single(WriteLog["save/additionalCharm.csv"]);
            Assert.Single(WriteLog["save/artian.csv"]);
        }

        /// <summary>
        /// ChangeNameOfMySetのテスト(異常系・名前がnull)
        /// </summary>
        [Fact]
        public void ChangeNameOfMySet_nullName()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            var set = masters.MySets[1];
            var oldName = set.Name;
            var count = masters.MySets.Count;

            // テスト
            dataManagement.ChangeNameOfMySet(null, set);
            Assert.Single(masters.MySets, set => set.Name == oldName);
            Assert.Equal(count, masters.MySets.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// ChangeNameOfMySetのテスト(異常系・名前が空)
        /// </summary>
        [Fact]
        public void ChangeNameOfMySet_emptyName()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            var count = masters.MySets.Count;
            var name = "RenameTest";

            // テスト
            dataManagement.ChangeNameOfMySet(name, null);
            Assert.Equal(count, masters.MySets.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// ChangeNameOfMySetのテスト(異常系・setがnull)
        /// </summary>
        [Fact]
        public void ChangeNameOfMySet_nullSet()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            var set = masters.MySets[1];
            var oldName = set.Name;
            var count = masters.MySets.Count;

            // テスト
            dataManagement.ChangeNameOfMySet(string.Empty, set);
            Assert.Single(masters.MySets, set => set.Name == oldName);
            Assert.Equal(count, masters.MySets.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// ChangeNameOfMySetのテスト(異常系・名前変更なし)
        /// </summary>
        [Fact]
        public void ChangeNameOfMySet_sameName()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            var set = masters.MySets[1];
            var oldName = set.Name;
            var count = masters.MySets.Count;
            var name = oldName;

            // テスト
            dataManagement.ChangeNameOfMySet(name, set);
            Assert.Single(masters.MySets, set => set.Name == oldName);
            Assert.Equal(count, masters.MySets.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// ChangeNameOfMySetのテスト(異常系・存在しないSet)
        /// </summary>
        [Fact]
        public void ChangeNameOfMySet_notExistSet()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            var set = new EquipSet();
            var count = masters.MySets.Count;
            var name = "RenameTest";

            // テスト
            dataManagement.ChangeNameOfMySet(name, set);
            Assert.Equal(count, masters.MySets.Count);
            Assert.Empty(WriteLog);
        }

        #endregion

        #region UpdateRecentSkill

        /// <summary>
        /// UpdateRecentSkillのテスト
        /// </summary>
        [Theory]
        [InlineData(4, 20, 3)] // 通常
        [InlineData(3, 3, 3)] // 上限オーバー
        [InlineData(1, 1, 0)] // 追加なしで上限オーバー 
        [InlineData(2, 0, 2)] // 追加分だけで上限オーバー
        public void UpdateRecentSkill_normal(int expectedCount, int max, int addSkillCount)
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();
            var logicConfig = ServiceProvider.GetRequiredService<LogicConfig>();

            // テストデータ
            logicConfig.MaxRecentSkillCount = max;
            List <Skill> skills = new();
            for (int i = 0; i < addSkillCount; i++)
            {
                if (i == 0)
                {
                    Skill skill = new("防具スキル", 1);
                    skills.Add(skill);
                }
                else
                {
                    Skill skill = new("防具スキル" + i, 1);
                    skills.Add(skill);
                }
            }

            // テスト
            dataManagement.UpdateRecentSkill(skills);
            if (addSkillCount > 0)
            {
                var armor = Assert.Single(masters.RecentSkillNames, s => s == "防具スキル");
                Assert.Equal(armor, masters.RecentSkillNames[0]);
            }
            else if (max > 0)
            {
                var weapon = Assert.Single(masters.RecentSkillNames, s => s == "武器スキル");
                Assert.Equal(weapon, masters.RecentSkillNames[0]);
            }
            Assert.Equal(expectedCount, masters.RecentSkillNames.Count);
            Assert.Single(WriteLog["save/recentSkill.csv"]);
        }

        #endregion

        #region MyCondition関連

        /// <summary>
        /// AddMyConditionのテスト(正常系)
        /// </summary>
        [Fact]
        public void AddMyCondition_normal()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition cond = new();
            int count = masters.MyConditions.Count;

            // テスト
            dataManagement.AddMyCondition(cond);
            Assert.Single(masters.MyConditions, c => c == cond);
            Assert.Equal(count + 1, masters.MyConditions.Count);
            Assert.Single(WriteLog["save/condition.csv"]);
        }

        /// <summary>
        /// AddMyConditionのテスト(異常系・null)
        /// </summary>
        [Fact]
        public void AddMyCondition_null()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            int count = masters.MyConditions.Count;

            // テスト
            dataManagement.AddMyCondition(null);
            Assert.Equal(count, masters.MyConditions.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// DeleteMyConditionのテスト(正常系)
        /// </summary>
        [Fact]
        public void DeleteMyCondition_normal()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition cond = masters.MyConditions[1];
            int count = masters.MyConditions.Count;

            // テスト
            dataManagement.DeleteMyCondition(cond);
            Assert.DoesNotContain(cond, masters.MyConditions);
            Assert.Equal(count - 1, masters.MyConditions.Count);
            Assert.Single(WriteLog["save/condition.csv"]);
        }

        /// <summary>
        /// DeleteMyConditionのテスト(異常系・存在しないデータ)
        /// </summary>
        [Fact]
        public void DeleteMyCondition_notExist()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition cond = new();
            int count = masters.MyConditions.Count;

            // テスト
            dataManagement.DeleteMyCondition(cond);
            Assert.DoesNotContain(cond, masters.MyConditions);
            Assert.Equal(count, masters.MyConditions.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// DeleteMyConditionのテスト(異常系・null)
        /// </summary>
        [Fact]
        public void DeleteMyCondition_null()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            int count = masters.MyConditions.Count;

            // テスト
            dataManagement.DeleteMyCondition(null);
            Assert.DoesNotContain(null, masters.MyConditions);
            Assert.Equal(count, masters.MyConditions.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// ChangeNameOfMyConditionのテスト(正常系)
        /// </summary>
        [Fact]
        public void ChangeNameOfMyCondition_normal()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            var condition = masters.MyConditions[1];
            var oldName = condition.DispName;
            var name = "RenameTest";
            var count = masters.MyConditions.Count;

            // テスト
            dataManagement.ChangeNameOfMyCondition(name, condition);
            Assert.Single(masters.MyConditions, cond => cond.DispName == name);
            Assert.DoesNotContain(masters.MyConditions, cond => cond.DispName == oldName);
            Assert.Equal(count, masters.MyConditions.Count);
            Assert.Single(WriteLog["save/condition.csv"]);
        }

        /// <summary>
        /// ChangeNameOfMyConditionのテスト(異常系・名前がnull)
        /// </summary>
        [Fact]
        public void ChangeNameOfMyCondition_nullName()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            var condition = masters.MyConditions[1];
            var oldName = condition.DispName;
            var count = masters.MyConditions.Count;

            // テスト
            dataManagement.ChangeNameOfMyCondition(null, condition);
            Assert.Single(masters.MyConditions, cond => cond.DispName == oldName);
            Assert.Equal(count, masters.MyConditions.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// ChangeNameOfMyConditionのテスト(異常系・名前が空)
        /// </summary>
        [Fact]
        public void ChangeNameOfMyCondition_emptyName()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            var count = masters.MyConditions.Count;
            var name = "RenameTest";

            // テスト
            dataManagement.ChangeNameOfMyCondition(name, null);
            Assert.Equal(count, masters.MyConditions.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// ChangeNameOfMyConditionのテスト(異常系・Conditionがnull)
        /// </summary>
        [Fact]
        public void ChangeNameOfMyCondition_nullCondition()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            var condition = masters.MyConditions[1];
            var oldName = condition.DispName;
            var count = masters.MyConditions.Count;

            // テスト
            dataManagement.ChangeNameOfMyCondition(string.Empty, condition);
            Assert.Single(masters.MyConditions, cond => cond.DispName == oldName);
            Assert.Equal(count, masters.MyConditions.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// ChangeNameOfMyConditionのテスト(異常系・名前変更なし)
        /// </summary>
        [Fact]
        public void ChangeNameOfMyCondition_sameName()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            var condition = masters.MyConditions[1];
            var oldName = condition.DispName;
            var count = masters.MyConditions.Count;
            var name = oldName;

            // テスト
            dataManagement.ChangeNameOfMyCondition(name, condition);
            Assert.Single(masters.MyConditions, cond => cond.DispName == oldName);
            Assert.Equal(count, masters.MyConditions.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// ChangeNameOfMyConditionのテスト(異常系・存在しないCondition)
        /// </summary>
        [Fact]
        public void ChangeNameOfMyCondition_notExistCondition()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            var condition = new SearchCondition();
            var count = masters.MyConditions.Count;
            var name = "RenameTest";

            // テスト
            dataManagement.ChangeNameOfMyCondition(name, condition);
            Assert.Equal(count, masters.MyConditions.Count);
            Assert.Empty(WriteLog);
        }

        #endregion

        #region SaveDecoCount

        /// <summary>
        /// SaveDecoCountのテスト(正常系)
        /// </summary>
        [Fact]
        public void SaveDecoCount_normal()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            Deco deco = masters.Decos[1];
            int count = 8; 

            // テスト
            dataManagement.SaveDecoCount(deco, count);
            Assert.Equal(count, deco.DecoCount);
            Assert.Single(WriteLog["save/decocount.json"]);
        }

        /// <summary>
        /// SaveDecoCountのテスト(異常系・マイナス値)
        /// </summary>
        [Fact]
        public void SaveDecoCount_minus()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            Deco deco = masters.Decos[1];
            int oldCount = deco.DecoCount;
            int count = -4;

            // テスト
            dataManagement.SaveDecoCount(deco, count);
            Assert.Equal(oldCount, deco.DecoCount);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// SaveDecoCountのテスト(異常系・null)
        /// </summary>
        [Fact]
        public void SaveDecoCount_null()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();

            // テストデータ
            int count = 4;

            // テスト
            dataManagement.SaveDecoCount(null, count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// SaveDecoCountのテスト(異常系・存在しないデータ)
        /// </summary>
        [Fact]
        public void SaveDecoCount_notExist()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();

            // テストデータ
            Deco deco = new();
            int count = 4;

            // テスト
            dataManagement.SaveDecoCount(null, count);
            Assert.Empty(WriteLog);
        }

        #endregion

        #region AdditionalCharm関連

        /// <summary>
        /// AddCharmのテスト(正常系)
        /// </summary>
        [Fact]
        public void AddCharm_normal()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            Equipment charm = new(EquipKind.charm);
            int count = masters.AdditionalCharms.Count;

            // テスト
            dataManagement.AddCharm(charm);
            Assert.Contains(charm, masters.AdditionalCharms);
            Assert.Equal(count + 1, masters.AdditionalCharms.Count);
            Assert.Single(WriteLog["save/additionalCharm.csv"]);
            // 全装備にも反映
            Assert.Contains(charm, masters.AllEquipments);
            // 下位互換計算済み
            Assert.NotNull(charm.Upper);
        }

        /// <summary>
        /// AddCharmのテスト(異常系・null)
        /// </summary>
        [Fact]
        public void AddCharm_null()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            int count = masters.AdditionalCharms.Count;

            // テスト
            dataManagement.AddCharm(null);
            Assert.Equal(count, masters.AdditionalCharms.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// AddCharmのテスト(異常系・既にあるインスタンス)
        /// </summary>
        [Fact]
        public void AddCharm_exist()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            Equipment charm = masters.AdditionalCharms[0];
            int count = masters.AdditionalCharms.Count;

            // テスト
            dataManagement.AddCharm(charm);
            Assert.Equal(count, masters.AdditionalCharms.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// DeleteCharmのテスト(正常系)
        /// </summary>
        [Fact]
        public void DeleteCharm_normal()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            Equipment charm = masters.AdditionalCharms.Where(c => c.Name == "e0b2395f-5dff-419a-997b-d80218ef647a").First();
            int count = masters.AdditionalCharms.Count;
            // 除外固定削除の確認用
            masters.Cludes.Add(new() { Name = "e0b2395f-5dff-419a-997b-d80218ef647a", Kind = CludeKind.exclude });

            // テスト
            dataManagement.DeleteCharm(charm);
            Assert.DoesNotContain(charm, masters.AdditionalCharms);
            Assert.Equal(count - 1, masters.AdditionalCharms.Count);
            Assert.DoesNotContain(masters.MySets, set => set.Charm == charm);
            Assert.DoesNotContain(masters.Cludes, c => c.Name == charm.Name);
            Assert.Single(WriteLog["save/clude.csv"]);
            Assert.Single(WriteLog["save/myset.csv"]);
            Assert.Single(WriteLog["save/additionalCharm.csv"]);
            // 全装備にも反映
            Assert.DoesNotContain(charm, masters.AllEquipments);
        }

        /// <summary>
        /// DeleteCharmのテスト(正常系・下位互換護石の確認)
        /// </summary>
        [Fact]
        public void DeleteCharm_another()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            Equipment lower = masters.AdditionalCharms.Where(c => c.Upper != null).First();
            int count = masters.AdditionalCharms.Count;
            Equipment? charm = lower.Upper?.Item1;

            // テスト
            dataManagement.DeleteCharm(charm);
            Assert.DoesNotContain(charm, masters.AdditionalCharms);
            Assert.Equal(count - 1, masters.AdditionalCharms.Count);
            Assert.Single(WriteLog["save/additionalCharm.csv"]);
            // 下位互換護石の再計算の確認
            Assert.NotEqual(charm, lower.Upper?.Item1);
        }

        /// <summary>
        /// DeleteCharmのテスト(異常系・null)
        /// </summary>
        [Fact]
        public void DeleteCharm_null()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            int count = masters.AdditionalCharms.Count;

            // テスト
            dataManagement.DeleteCharm(null);
            Assert.Equal(count, masters.AdditionalCharms.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// DeleteCharmのテスト(異常系・存在しないデータ)
        /// </summary>
        [Fact]
        public void DeleteCharm_notExist()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            Equipment charm = new();
            int count = masters.AdditionalCharms.Count;

            // テスト
            dataManagement.DeleteCharm(charm);
            Assert.Equal(count, masters.AdditionalCharms.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// MoveCharmのテスト
        /// </summary>
        [Theory]
        [InlineData(true, 1, 2)]
        [InlineData(false, 1, 200)]
        [InlineData(false, 100, 2)]
        [InlineData(false, -1, 2)]
        [InlineData(false, 1, -2)]
        [InlineData(false, 1, 1)]
        public void MoveCharm_normal(bool isEffective, int dropIndex, int targetIndex)
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            Equipment? dropCharm = null;
            if (isEffective)
            {
                dropCharm = masters.AdditionalCharms[dropIndex];
            }
            int count = masters.AdditionalCharms.Count;
            var oldAll = masters.AllEquipments;

            // テスト
            dataManagement.MoveCharm(dropIndex, targetIndex);
            Assert.Equal(count, masters.AdditionalCharms.Count);
            if (isEffective)
            {
                Assert.Equal(dropCharm, masters.AdditionalCharms[targetIndex]);
                Assert.Single(WriteLog["save/additionalCharm.csv"]);
                // 中身は一緒だが作り直すため参照は変わる
                Assert.NotSame(oldAll, masters.AllEquipments);
            }
            else
            {
                Assert.Empty(WriteLog);
            }
        }

        #endregion

        #region Artian関連

        /// <summary>
        /// AddArtianのテスト(正常系)
        /// </summary>
        [Fact]
        public void AddArtian_normal()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            Weapon artian = new();
            artian.InitArtian();
            int count = masters.Artians.Count;

            // テスト
            dataManagement.AddArtian(artian);
            Assert.Contains(artian, masters.Artians);
            Assert.Equal(count + 1, masters.Artians.Count);
            Assert.Single(WriteLog["save/artian.csv"]);
            // 全装備にも反映
            Assert.Contains(artian, masters.AllEquipments);
        }

        /// <summary>
        /// AddArtianのテスト(異常系・null)
        /// </summary>
        [Fact]
        public void AddArtian_null()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            int count = masters.Artians.Count;

            // テスト
            dataManagement.AddArtian(null);
            Assert.Equal(count, masters.Artians.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// AddArtianのテスト(異常系・既にあるインスタンス)
        /// </summary>
        [Fact]
        public void AddArtian_exist()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            Weapon artian = masters.Artians[0];
            int count = masters.Artians.Count;

            // テスト
            dataManagement.AddArtian(artian);
            Assert.Equal(count, masters.Artians.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// DeleteArtianのテスト(正常系)
        /// </summary>
        [Fact]
        public void DeleteArtian_normal()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            Weapon artian = masters.Artians.Where(c => c.Name == "17d1d296-0c36-4b40-a954-87ef527bd2cd").First();
            int count = masters.Artians.Count;
            // 除外固定削除の確認用
            masters.Cludes.Add(new() { Name = "17d1d296-0c36-4b40-a954-87ef527bd2cd", Kind = CludeKind.exclude });

            // テスト
            dataManagement.DeleteArtian(artian);
            Assert.DoesNotContain(artian, masters.Artians);
            Assert.Equal(count - 1, masters.Artians.Count);
            Assert.DoesNotContain(masters.MySets, set => set.Weapon == artian);
            Assert.DoesNotContain(masters.Cludes, c => c.Name == artian.Name);
            Assert.Single(WriteLog["save/clude.csv"]);
            Assert.Single(WriteLog["save/myset.csv"]);
            Assert.Single(WriteLog["save/artian.csv"]);
            // 全装備にも反映
            Assert.DoesNotContain(artian, masters.AllEquipments);
        }

        /// <summary>
        /// DeleteArtianのテスト(異常系・null)
        /// </summary>
        [Fact]
        public void DeleteArtian_null()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            int count = masters.Artians.Count;

            // テスト
            dataManagement.DeleteArtian(null);
            Assert.Equal(count, masters.Artians.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// DeleteArtianのテスト(異常系・存在しないデータ)
        /// </summary>
        [Fact]
        public void DeleteArtian_notExist()
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            Weapon artian = new();
            int count = masters.Artians.Count;

            // テスト
            dataManagement.DeleteArtian(artian);
            Assert.Equal(count, masters.Artians.Count);
            Assert.Empty(WriteLog);
        }

        /// <summary>
        /// MoveArtianのテスト
        /// </summary>
        [Theory]
        [InlineData(true, 1, 0)]
        [InlineData(false, 1, 200)]
        [InlineData(false, 100, 2)]
        [InlineData(false, -1, 2)]
        [InlineData(false, 1, -2)]
        [InlineData(false, 1, 1)]
        public void MoveArtian_normal(bool isEffective, int dropIndex, int targetIndex)
        {
            // DIコンテナから取得
            var dataManagement = ServiceProvider.GetRequiredService<DataManagement>();
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            Equipment? dropCharm = null;
            if (isEffective)
            {
                dropCharm = masters.Artians[dropIndex];
            }
            int count = masters.Artians.Count;
            var oldAll = masters.AllEquipments;

            // テスト
            dataManagement.MoveArtian(dropIndex, targetIndex);
            Assert.Equal(count, masters.Artians.Count);
            if (isEffective)
            {
                Assert.Equal(dropCharm, masters.Artians[targetIndex]);
                Assert.Single(WriteLog["save/artian.csv"]);
                // 中身は一緒だが作り直すため参照は変わる
                Assert.NotSame(oldAll, masters.AllEquipments);
            }
            else
            {
                Assert.Empty(WriteLog);
            }
        }

        #endregion

    }
}
