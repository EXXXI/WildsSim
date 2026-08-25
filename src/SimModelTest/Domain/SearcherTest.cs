using Microsoft.Extensions.DependencyInjection;
using SimModel.Domain;
using SimModel.Model;
using SimModel.Service;

namespace SimModelTest.Domain
{
    public class SearcherTest : TestDataSetUp
    {
        /// <summary>
        /// ExecSearchのテスト(正常系・武器指定)
        /// </summary>
        [Fact]
        public async Task ExecSearch_normal()
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 5));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 10;

            // テスト
            var result = await searcher.ExecSearch(limit);
            var set = Assert.Single(searcher.ResultSets);
            foreach (var skill in condition.Skills)
            {
                Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
            }
            Assert.Equal(condition.WeaponName, set.Weapon.Name);
        }

        /// <summary>
        /// ExecSearchのテスト(正常系・上限)
        /// </summary>
        [Theory]
        [InlineData(6, 10, true)] // つよ4よわ1の5パターン + つよ5の1パターン
        [InlineData(3, 3, false)]
        public async Task ExecSearch_limit(int expectedCount, int limit, bool allSearched)
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 4));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Equal(expectedCount, searcher.ResultSets.Count);
            Assert.Equal(allSearched, result);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
                }
            }
        }

        /// <summary>
        /// ExecSearchのテスト(正常系・武器種指定)
        /// </summary>
        [Fact]
        public async Task ExecSearch_weaponType()
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 5));
            condition.AddSkill(new Skill("武器スキル", 1));
            condition.IsSpecificWeapon = false;
            condition.WeaponType = WeaponType.大剣;
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 10;

            // テスト
            var result = await searcher.ExecSearch(limit);
            var set = Assert.Single(searcher.ResultSets);
            foreach (var skill in condition.Skills)
            {
                Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
            }
        }

        /// <summary>
        /// ExecSearchのテスト(正常系・防御)
        /// </summary>
        [Theory]
        [InlineData(1, 180, true)]
        [InlineData(1, 179, true)]
        [InlineData(7, 174, true)] // つよ4よわ1の5パターン + つよ5の1パターン + セット1パターン
        [InlineData(1, 140, false)]
        [InlineData(1, 139, false)]
        [InlineData(0, 141, false)]
        [InlineData(6, 132, false)] // つよ4よわ1の5パターン + つよ5の1パターン
        public async Task ExecSearch_def(int expectedCount, int def, bool isTrancending)
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            condition.IsTranscending = isTrancending;
            condition.Def = def;
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 10;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Equal(expectedCount, searcher.ResultSets.Count);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
                }
                Assert.True(condition.Def <= set.Maxdef);
            }
        }

        /// <summary>
        /// ExecSearchのテスト(正常系・攻撃力)
        /// </summary>
        [Theory]
        [InlineData(1, 0)] // 攻撃力0だと武器なしの結果が出た際にまとめられる
        [InlineData(3, 10)]
        [InlineData(2, 95)]
        public async Task ExecSearch_regist(int expectedCount, int attack)
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 5));
            condition.IsSpecificWeapon = false;
            condition.WeaponType = WeaponType.大剣;
            condition.MinAttack = attack;
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 10;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Equal(expectedCount, searcher.ResultSets.Count);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
                }
                Assert.True(condition.MinAttack <= set.Weapon.Attack);
            }
        }

        /// <summary>
        /// ExecSearchのテスト(正常系・通常スキル固定)
        /// </summary>
        [Fact]
        public async Task ExecSearch_fix()
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 4) { IsFixed = true });
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 10;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Equal(5, searcher.ResultSets.Count);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
                }
                Assert.Equal(4, set.Skills.Where(s => s.Name == "つよ防具スキル全").First().Level);
            }
        }

        /// <summary>
        /// ExecSearchのテスト(正常系・グループスキル固定)
        /// </summary>
        [Theory]
        [InlineData(17, 0)] // ワンセット防具が混じる
        [InlineData(16, 3)]
        public async Task ExecSearch_groupFix(int expectedCount, int fixLevel)
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよグループ", fixLevel) { IsFixed = true });
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 20;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Equal(expectedCount, searcher.ResultSets.Count);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True((set.Skills.Where(s => s.Name == skill.Name).FirstOrDefault()?.Level ?? 0) >= skill.Level);
                }
            }
        }

        /// <summary>
        /// ExecSearchのテスト(正常系・シリーズスキル固定)
        /// </summary>
        [Theory]
        [InlineData(7, 0)] // ワンセット防具が混じる
        [InlineData(20, 2)]
        [InlineData(6, 4)]
        public async Task ExecSearch_seriesFix(int expectedCount, int fixLevel)
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよの力", fixLevel) { IsFixed = true });
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 20;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Equal(expectedCount, searcher.ResultSets.Count);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True((set.Skills.Where(s => s.Name == skill.Name).FirstOrDefault()?.Level ?? 0) >= skill.Level);
                }
            }
        }

        /// <summary>
        /// ExecSearchのテスト(正常系・スロット限界突破)
        /// </summary>
        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        public async Task ExecSearch_trancending(int expectedCount, bool isTrancending)
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("スロ3防具スキル", 5));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            condition.IsTranscending = isTrancending;
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 10;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Equal(expectedCount, searcher.ResultSets.Count);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
                }
            }
        }

        /// <summary>
        /// ExecSearchのテスト(正常系・理想護石)
        /// </summary>
        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        public async Task ExecSearch_bestCharm(int expectedCount, bool isBestCharmSearch)
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("つよ防具スキル全", 6));
            condition.AddSkill(new Skill("スロ1武器スキル", 1));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            condition.IsBestCharmSearch = isBestCharmSearch;
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 10;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Equal(expectedCount, searcher.ResultSets.Count);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
                }
            }
        }

        /// <summary>
        /// ExecSearchのテスト(正常系・理想アーティア)
        /// </summary>
        [Theory]
        [InlineData(0, false)]
        [InlineData(10, true)]
        public async Task ExecSearch_bestArtian(int expectedCount, bool isBestArtianSearch)
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよグループ", 3));
            condition.AddSkill(new Skill("よわの力", 4));
            condition.IsSpecificWeapon = false;
            condition.WeaponType = WeaponType.大剣;
            condition.IsBestArtianSearch = isBestArtianSearch;
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 20;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Equal(expectedCount, searcher.ResultSets.Count);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
                }
            }
        }

        /// <summary>
        /// ExecSearchのテスト(正常系・防具除外)
        /// </summary>
        [Fact]
        public async Task ExecSearch_exclude()
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            masters.Cludes.Add(new() { Name = "つよ腕", Kind = CludeKind.exclude });
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 4));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 20;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Single(searcher.ResultSets);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
                }
                Assert.NotEqual("つよ腕", set.Arm.Name);
            }
        }

        /// <summary>
        /// ExecSearchのテスト(正常系・防具固定)
        /// </summary>
        [Fact]
        public async Task ExecSearch_include()
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            masters.Cludes.Add(new() { Name = "よわ腕", Kind = CludeKind.include });
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 4));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 20;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Single(searcher.ResultSets);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
                }
                Assert.Equal("よわ腕", set.Arm.Name);
            }
        }

        /// <summary>
        /// ExecSearchのテスト(正常系・装飾品所持数)
        /// </summary>
        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        public async Task ExecSearch_decoCount(int expectedCount, bool hasAllDecos)
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("スロ1不足スキル", 5));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            condition.HasAllDecos = hasAllDecos;
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 20;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Equal(expectedCount, searcher.ResultSets.Count);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
                }
            }
        }

        /// <summary>
        /// ExecSearchのテスト(正常系・装飾品)
        /// </summary>
        [Theory]
        [InlineData(2, "スロ3防具スキル")]
        [InlineData(2, "スロ3武器スキル")]
        [InlineData(3, "スロ3両対応スキル")]
        [InlineData(1, "スロ3両要求スキル")]
        public async Task ExecSearch_deco(int expectedCount, string decoSkillName)
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 5));
            condition.AddSkill(new Skill("つよ防具スキル全", 5));
            condition.AddSkill(new Skill(decoSkillName, 1));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 20;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Equal(expectedCount, searcher.ResultSets.Count);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
                }
            }
        }

        /// <summary>
        /// ExecSearchのテスト(正常系・キャンセル)
        /// </summary>
        [Fact]
        public async Task ExecSearch_cancel()
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new SearchCondition();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("スロ1武器スキル", 1));
            condition.IsSpecificWeapon = false;
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 1000;

            // テスト
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var task = Task.Run(() => searcher.ExecSearch(limit, token));
            tokenSource.Cancel();
            var result = await task;
            Assert.True(searcher.ResultSets.Count < 627);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
                }
            }
        }

        /// <summary>
        /// ExecSearchのテスト(異常系・null武器指定)
        /// 検索結果0件
        /// </summary>
        [Fact]
        public async Task ExecSearch_nullWeaponName()
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 5));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = null;
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 10;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Empty(searcher.ResultSets);
        }

        /// <summary>
        /// ExecSearchのテスト(異常系・空文字武器指定)
        /// 検索結果0件
        /// </summary>
        [Fact]
        public async Task ExecSearch_emptyWeaponName()
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 5));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "";
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 10;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Empty(searcher.ResultSets);
        }

        /// <summary>
        /// ExecSearchのテスト(異常系・存在しない武器指定)
        /// 検索結果0件
        /// </summary>
        [Fact]
        public async Task ExecSearch_notExistWeaponName()
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 5));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "dummy";
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 10;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Empty(searcher.ResultSets);
        }

        // TODO: 需要があれば調整して正常系として実装
        /// <summary>
        /// ExecSearchのテスト(異常系・武器種指定なし)
        /// スロットのみデータで検索
        /// </summary>
        [Fact]
        public async Task ExecSearch_noWeaponType()
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 5));
            condition.IsSpecificWeapon = false;
            condition.WeaponType = WeaponType.指定なし;
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 10;

            // テスト
            var result = await searcher.ExecSearch(limit);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
                }
            }
        }

        /// <summary>
        /// ExecSearchのテスト(異常系・異常防具除外)
        /// 無視する
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("dummy")]
        public async Task ExecSearch_invalidExclude(string? name)
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            masters.Cludes.Add(new() { Name = name, Kind = CludeKind.exclude });
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 5));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 20;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Single(searcher.ResultSets);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
                }
            }
        }

        /// <summary>
        /// ExecSearchのテスト(異常系・異常防具固定)
        /// 無視する
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("dummy")]
        public async Task ExecSearch_invalidInclude(string? name)
        {
            // DIコンテナから取得
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テストデータ
            masters.Cludes.Add(new() { Name = name, Kind = CludeKind.include });
            SearchCondition condition = new();
            condition.AddSkill(new Skill("防具スキル", 6));
            condition.AddSkill(new Skill("つよ防具スキル全", 5));
            condition.IsSpecificWeapon = true;
            condition.WeaponName = "スロットのみ_0-0-0";
            SearchRange range = new(condition, masters);
            Searcher searcher = new(condition, range);
            int limit = 20;

            // テスト
            var result = await searcher.ExecSearch(limit);
            Assert.Single(searcher.ResultSets);
            foreach (var set in searcher.ResultSets)
            {
                foreach (var skill in condition.Skills)
                {
                    Assert.True(set.Skills.Where(s => s.Name == skill.Name).First().Level >= skill.Level);
                }
            }
        }
    }
}
