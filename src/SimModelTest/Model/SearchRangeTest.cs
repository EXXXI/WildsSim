using Microsoft.Extensions.DependencyInjection;
using SimModel.Model;

namespace SimModelTest.Model
{
    /// <summary>
    /// SearchRangeのテスト
    /// </summary>
    public class SearchRangeTest : TestDataSetUp
    {
        /// <summary>
        /// コンストラクタのテスト(正常系・引数なし)
        /// </summary>
        [Fact]
        public void SearchRangeTest_noArgs()
        {
            SearchRange range = new();
            Assert.Empty(range.Weapons);
            Assert.Empty(range.Heads);
            Assert.Empty(range.Bodys);
            Assert.Empty(range.Arms);
            Assert.Empty(range.Waists);
            Assert.Empty(range.Legs);
            Assert.Empty(range.Charms);
            Assert.Empty(range.Decos);
            Assert.Empty(range.Cludes);
        }

        /// <summary>
        /// コンストラクタのテスト(正常系・武器固定)
        /// </summary>
        [Fact]
        public void SearchRangeTest_specificWeapon()
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            SearchCondition cond = new();
            cond.Skills.Add(new("つよの力", 2));
            cond.Skills.Add(new("防具スキル", 2));
            cond.IsSpecificWeapon = true;
            cond.WeaponName = "スロットのみ_0-0-0";

            // テスト
            SearchRange range = new(cond, masters);
            var weapon =Assert.Single(range.Weapons);
            Assert.Equal("スロットのみ_0-0-0", weapon.Name);
            Assert.NotEmpty(range.Heads);
            Assert.NotEmpty(range.Bodys);
            Assert.NotEmpty(range.Arms);
            Assert.NotEmpty(range.Waists);
            Assert.NotEmpty(range.Legs);
            Assert.NotEmpty(range.Charms);
            Assert.DoesNotContain(range.Charms, c => c.IsVirtual);
            Assert.NotEmpty(range.Decos);
            Assert.NotEmpty(range.Cludes);
        }

        /// <summary>
        /// コンストラクタのテスト(正常系・武器種のみ設定)
        /// </summary>
        [Fact]
        public void SearchRangeTest_weaponType()
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            SearchCondition cond = new();
            cond.Skills.Add(new("つよの力", 2));
            cond.Skills.Add(new("防具スキル", 2));
            cond.IsSpecificWeapon = false;
            cond.WeaponType = WeaponType.大剣;

            // テスト
            SearchRange range = new(cond, masters);
            Assert.NotEmpty(range.Weapons);
            Assert.DoesNotContain(range.Weapons, w => w.WeaponType != WeaponType.大剣);
            Assert.NotEmpty(range.Heads);
            Assert.NotEmpty(range.Bodys);
            Assert.NotEmpty(range.Arms);
            Assert.NotEmpty(range.Waists);
            Assert.NotEmpty(range.Legs);
            Assert.NotEmpty(range.Charms);
            Assert.DoesNotContain(range.Charms, c => c.IsVirtual);
            Assert.NotEmpty(range.Decos);
            Assert.NotEmpty(range.Cludes);
        }

        /// <summary>
        /// コンストラクタのテスト(正常系・理想検索2種)
        /// </summary>
        [Fact]
        public void SearchRangeTest_best()
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            SearchCondition cond = new();
            cond.Skills.Add(new("つよの力", 2));
            cond.Skills.Add(new("防具スキル", 2));
            cond.IsSpecificWeapon = false;
            cond.WeaponType = WeaponType.大剣;
            cond.IsBestCharmSearch = true;
            cond.IsBestArtianSearch = true;

            // テスト
            SearchRange range = new(cond, masters);
            Assert.NotEmpty(range.Weapons);
            Assert.Contains(range.Weapons, w => w.IsVirtual);
            Assert.NotEmpty(range.Heads);
            Assert.NotEmpty(range.Bodys);
            Assert.NotEmpty(range.Arms);
            Assert.NotEmpty(range.Waists);
            Assert.NotEmpty(range.Legs);
            Assert.NotEmpty(range.Charms);
            Assert.Contains(range.Charms, c => c.IsVirtual);
            Assert.NotEmpty(range.Decos);
            Assert.NotEmpty(range.Cludes);
        }

        /// <summary>
        /// コンストラクタのテスト(異常系・武器固定)
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("dummy")]
        [InlineData(null)]
        public void SearchRangeTest_invalidWeapon(string? weaponName)
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            SearchCondition cond = new();
            cond.Skills.Add(new("つよの力", 2));
            cond.Skills.Add(new("防具スキル", 2));
            cond.IsSpecificWeapon = true;
            cond.WeaponName = weaponName;

            // テスト
            SearchRange range = new(cond, masters);
            Assert.Empty(range.Weapons);
            Assert.NotEmpty(range.Heads);
            Assert.NotEmpty(range.Bodys);
            Assert.NotEmpty(range.Arms);
            Assert.NotEmpty(range.Waists);
            Assert.NotEmpty(range.Legs);
            Assert.NotEmpty(range.Charms);
            Assert.DoesNotContain(range.Charms, c => c.IsVirtual);
            Assert.NotEmpty(range.Decos);
            Assert.NotEmpty(range.Cludes);
        }

        /// <summary>
        /// コンストラクタのテスト(異常系・条件null)
        /// 想定していない呼び出し方なので例外で止まることだけ確認する
        /// </summary>
        [Fact]
        public void SearchRangeTest_nullCond()
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();

            // テスト
            var ex = Assert.ThrowsAny<Exception>(() => new SearchRange(null, masters));
        }

        /// <summary>
        /// コンストラクタのテスト(異常系・マスタnull)
        /// 想定していない呼び出し方なので例外で止まることだけ確認する
        /// </summary>
        [Fact]
        public void SearchRangeTest_nullMasters()
        {
            // テストデータ
            SearchCondition cond = new();
            cond.Skills.Add(new("つよの力", 2));
            cond.Skills.Add(new("防具スキル", 2));
            cond.IsSpecificWeapon = true;
            cond.WeaponName = "スロットのみ_0-0-0";

            // テスト
            var ex = Assert.ThrowsAny<Exception>(() => new SearchRange(cond, null));
        }
    }
}
