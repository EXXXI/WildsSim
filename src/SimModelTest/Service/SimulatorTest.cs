using Microsoft.Extensions.DependencyInjection;
using Reactive.Bindings;
using SimModel.Model;
using SimModel.Service;

namespace SimModelTest.Service
{
    /// <summary>
    /// Simulatorのテスト
    /// </summary>
    public class SimulatorTest : TestDataSetUp
    {
        /// <summary>
        /// Searchのテスト(正常系)
        /// 細かい内部挙動のテストはSearcherのテストで行うためここでは導通と理想装備検索フラグのみ確認
        /// </summary>
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task SearchTest_normal(bool isBestCharm, bool isBestArtian)
        {
            // テストデータ
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            SearchCondition condition = new SearchCondition();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 4));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            condition.IsBestCharmSearch = isBestCharm;
            condition.IsBestArtianSearch = isBestArtian;

            //テスト
            var result = await simulator.Search(condition, 3);
            Assert.NotEmpty(result);
            Assert.Single(WriteLog["save/recentSkill.csv"]);
            Assert.Equal(isBestCharm, simulator.IsBestCharmSearch);
            Assert.Equal(isBestArtian, simulator.IsBestArtianSearch);
        }

        /// <summary>
        /// SearchMoreのテスト(正常系)
        /// </summary>
        [Fact]
        public async Task SearchMoreTest_normal()
        {
            // テストデータ
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            SearchCondition condition = new SearchCondition();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 4));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";

            //テスト
            var result1 = await simulator.Search(condition, 3);
            Assert.Equal(3, result1.Count);
            var result2 = await simulator.SearchMore(2);
            Assert.Equal(5, result1.Count);
            var result3 = await simulator.SearchMore(2);
            Assert.Equal(6, result1.Count);
            Assert.Single(WriteLog["save/recentSkill.csv"]);
        }

        /// <summary>
        /// SearchMoreのテスト(異常系・未検索)
        /// </summary>
        [Fact]
        public async Task SearchMoreTest_noSearcher()
        {
            // テストデータ
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            SearchCondition condition = new SearchCondition();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 4));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";

            //テスト
            var result = await simulator.SearchMore(2);
            Assert.Empty(result);
        }

        /// <summary>
        /// SearchExtraSkillのテスト(正常系)
        /// </summary>
        [Fact]
        public async Task SearchExtraSkillTest_normal()
        {
            // テストデータ
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            SearchCondition condition = new SearchCondition();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("よわグループ", 3));
            condition.AddSkill(new Skill("よわの力", 2));
            condition.AddSkill(new Skill("つよの力", 2));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";

            //テスト
            var result = await simulator.SearchExtraSkill(condition);
            Assert.Equal(10, result.Count);
            Assert.Contains(result, s => s.Name == "つよ防具スキル全" && s.Level == 1);
            Assert.Contains(result, s => s.Name == "つよ防具スキル全" && s.Level == 2);
            Assert.Contains(result, s => s.Name == "つよ防具スキル頭" && s.Level == 1);
            Assert.Contains(result, s => s.Name == "つよ防具スキル胴" && s.Level == 1);
            Assert.Contains(result, s => s.Name == "つよ防具スキル腕" && s.Level == 1);
            Assert.Contains(result, s => s.Name == "つよ防具スキル腰" && s.Level == 1);
            Assert.Contains(result, s => s.Name == "つよ防具スキル足" && s.Level == 1);
            // 表示しないレベルもこの段階では残しておく
            Assert.Contains(result, s => s.Name == "よわの力" && s.Level == 3);
            Assert.Contains(result, s => s.Name == "つよグループ" && s.Level == 1);
            Assert.Contains(result, s => s.Name == "つよグループ" && s.Level == 2);
        }

        /// <summary>
        /// SearchExtraSkillのテスト(正常系・progressあり)
        /// </summary>
        [Fact]
        public async Task SearchExtraSkillTest_progress()
        {
            // テストデータ
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            SearchCondition condition = new SearchCondition();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("よわグループ", 3));
            condition.AddSkill(new Skill("よわの力", 2));
            condition.AddSkill(new Skill("つよの力", 2));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            ReactivePropertySlim<double> progress = new();

            //テスト
            var result = await simulator.SearchExtraSkill(condition, progress: progress);
            Assert.Equal(1, progress.Value, 0.001);
        }

        /// <summary>
        /// SearchCharmのテスト(正常系)
        /// </summary>
        [Fact]
        public async Task SearchCharm_normal()
        {
            // テストデータ
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            SearchCondition condition = new SearchCondition();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 1));
            condition.AddSkill(new Skill("スロ1防具スキル", 1));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";

            // 護石の除外固定が機能しないことを確認する
            var masters = ServiceProvider.GetRequiredService<Masters>();
            masters.Cludes.Add(new() { Name = "テスト護石Ⅰ", Kind = CludeKind.include });

            //テスト
            var result1 = await simulator.Search(condition, 10);
            Assert.Empty(result1);
            var result2 = await simulator.SearchCharm();
            var set = Assert.Single(result2);
            Assert.True(set.Charm.IsVirtual);
        }

        /// <summary>
        /// SearchCharmのテスト(正常系・progressあり)
        /// </summary>
        [Fact]
        public async Task SearchCharm_progress()
        {
            // テストデータ
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            SearchCondition condition = new SearchCondition();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 1));
            condition.AddSkill(new Skill("スロ1防具スキル", 1));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            ReactivePropertySlim<double> progress = new();

            // 護石の除外固定が機能しないことを確認する
            var masters = ServiceProvider.GetRequiredService<Masters>();
            masters.Cludes.Add(new() { Name = "テスト護石Ⅰ", Kind = CludeKind.include });

            //テスト
            var result1 = await simulator.Search(condition, 10);
            Assert.Empty(result1);
            var result2 = await simulator.SearchCharm(progress: progress);
            Assert.Equal(1, progress.Value, 0.001);
        }

        /// <summary>
        /// SearchCharmのテスト(異常系・未検索)
        /// </summary>
        [Fact]
        public async Task SearchCharm_noSearcher()
        {
            // テストデータ
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            SearchCondition condition = new SearchCondition();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 1));
            condition.AddSkill(new Skill("スロ1防具スキル", 1));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";

            //テスト
            var result2 = await simulator.SearchCharm();
            Assert.Empty(result2);
        }

        /// <summary>
        /// AddExcludeのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void AddExclude()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            simulator.AddExclude("つよ頭");
            Assert.Single(WriteLog["save/clude.csv"]);
        }

        /// <summary>
        /// AddIncludeのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void AddInclude()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            simulator.AddInclude("つよ頭");
            Assert.Single(WriteLog["save/clude.csv"]);
        }

        /// <summary>
        /// DeleteCludeのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void DeleteClude()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            simulator.AddExclude("つよ頭");
            ClearAllWriteLog();

            simulator.DeleteClude("つよ頭");
            Assert.Single(WriteLog["save/clude.csv"]);
        }

        /// <summary>
        /// DeleteAllCludeのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void DeleteAllClude()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            simulator.DeleteAllClude();
            Assert.Single(WriteLog["save/clude.csv"]);
        }

        /// <summary>
        /// DeleteAllArmorCludeのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void DeleteAllArmorClude()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            simulator.DeleteAllArmorClude();
            Assert.Single(WriteLog["save/clude.csv"]);
        }

        /// <summary>
        /// DeleteAllWeaponCludeのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void DeleteAllWeaponClude()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            simulator.DeleteAllWeaponClude();
            Assert.Single(WriteLog["save/clude.csv"]);
        }

        /// <summary>
        /// AddMySetのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void AddMySet()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            simulator.AddMySet(new());
            Assert.Single(WriteLog["save/myset.csv"]);
        }

        /// <summary>
        /// DeleteMySetのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void DeleteMySet()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            var masters = ServiceProvider.GetRequiredService<Masters>();
            simulator.DeleteMySet(masters.MySets.First());
            Assert.Single(WriteLog["save/myset.csv"]);
        }

        /// <summary>
        /// ChangeNameOfMySetのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void ChangeNameOfMySet()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            var masters = ServiceProvider.GetRequiredService<Masters>();
            simulator.ChangeNameOfMySet("nameChange", masters.MySets.First());
            Assert.Single(WriteLog["save/myset.csv"]);
        }

        /// <summary>
        /// MoveMySetのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void MoveMySet()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            simulator.MoveMySet(0, 1);
            Assert.Single(WriteLog["save/myset.csv"]);
        }

        /// <summary>
        /// AddMyConditionのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void AddMyCondition()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            simulator.AddMyCondition(new());
            Assert.Single(WriteLog["save/condition.csv"]);
        }

        /// <summary>
        /// DeleteMyConditionのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void DeleteMyCondition()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            var masters = ServiceProvider.GetRequiredService<Masters>();
            simulator.DeleteMyCondition(masters.MyConditions.First());
            Assert.Single(WriteLog["save/condition.csv"]);
        }

        /// <summary>
        /// ChangeNameOfMyConditionのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void ChangeNameOfMyCondition()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            var masters = ServiceProvider.GetRequiredService<Masters>();
            simulator.ChangeNameOfMyCondition("nameChange", masters.MyConditions.First());
            Assert.Single(WriteLog["save/condition.csv"]);
        }

        /// <summary>
        /// SaveDecoCountのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void SaveDecoCount()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            var masters = ServiceProvider.GetRequiredService<Masters>();
            simulator.SaveDecoCount(masters.Decos.First(), 7);
            Assert.Single(WriteLog["save/decocount.json"]);
        }

        /// <summary>
        /// AddCharmのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void AddCharm()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            simulator.AddCharm(new());
            Assert.Single(WriteLog["save/additionalCharm.csv"]);
        }

        /// <summary>
        /// DeleteCharmのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void DeleteCharm()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            var masters = ServiceProvider.GetRequiredService<Masters>();
            simulator.DeleteCharm(masters.AdditionalCharms.First());
            Assert.Single(WriteLog["save/additionalCharm.csv"]);
        }

        /// <summary>
        /// MoveCharmのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void MoveCharm()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            simulator.MoveCharm(0, 1);
            Assert.Single(WriteLog["save/additionalCharm.csv"]);
        }

        /// <summary>
        /// AddArtianのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void AddArtian()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            simulator.AddArtian(new());
            Assert.Single(WriteLog["save/artian.csv"]);
        }

        /// <summary>
        /// DeleteArtianのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void DeleteArtian()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            var masters = ServiceProvider.GetRequiredService<Masters>();
            simulator.DeleteArtian(masters.Artians.First());
            Assert.Single(WriteLog["save/artian.csv"]);
        }

        /// <summary>
        /// MoveArtianのテスト
        /// 細かい内部挙動のテストはDataManagementのテストで行うためここでは導通のみ確認
        /// </summary>
        [Fact]
        public void MoveArtian()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            simulator.MoveArtian(0, 1);
            Assert.Single(WriteLog["save/artian.csv"]);
        }

        /// <summary>
        /// Searchのテスト(正常系・キャンセル)
        /// </summary>
        [Fact]
        public async Task SearchTest_cancel()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            SearchCondition condition = new SearchCondition();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("スロ1武器スキル", 1));
            condition.IsSpecificWeapon = false;

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var task = Task.Run(() => simulator.Search(condition, 1000, token));
            tokenSource.Cancel();
            var result = await task;

            Assert.True(result.Count < 627); // 627: 本来の検索結果数
            Assert.True(simulator.IsCanceling);
        }

        /// <summary>
        /// SearchMoreのテスト(正常系・キャンセル)
        /// </summary>
        [Fact]
        public async Task SearchMoreTest_cancel()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            SearchCondition condition = new SearchCondition();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("スロ1武器スキル", 1));
            condition.IsSpecificWeapon = false;
            await simulator.Search(condition, 1);

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var task = Task.Run(() => simulator.SearchMore(1000, token));
            tokenSource.Cancel();
            var result = await task;

            Assert.True(result.Count < 627); // 627: 本来の検索結果数
            Assert.True(simulator.IsCanceling);
        }

        /// <summary>
        /// SearchExtraSkillのテスト(正常系・キャンセル)
        /// </summary>
        [Fact]
        public async Task SearchExtraSkillTest_cancel()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            SearchCondition condition = new SearchCondition();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("スロ1武器スキル", 1));
            condition.IsSpecificWeapon = false;
            var max = await simulator.SearchExtraSkill(condition);

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var task = Task.Run(() => simulator.SearchExtraSkill(condition, token: token));
            tokenSource.Cancel();
            var result = await task;

            Assert.True(result.Count < max.Count);
            Assert.True(simulator.IsCanceling);
        }

        /// <summary>
        /// SearchCharmのテスト(正常系・キャンセル)
        /// </summary>
        [Fact]
        public async Task SearchCharmTest_cancel()
        {
            var simulator = ServiceProvider.GetRequiredService<Simulator>();
            SearchCondition condition = new SearchCondition();
            condition.AddSkill(new Skill("強欲の力", 2));
            condition.AddSkill(new Skill("つよ防具スキル全", 1));
            condition.AddSkill(new Skill("防具スキル", 5));
            condition.IsSpecificWeapon = false;
            await simulator.Search(condition, 10);
            var max = await simulator.SearchCharm();

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var task = Task.Run(() => simulator.SearchCharm(token: token));
            tokenSource.Cancel();
            var result = await task;

            Assert.True(result.Count < max.Count);
            Assert.True(simulator.IsCanceling);
        }
    }
}
