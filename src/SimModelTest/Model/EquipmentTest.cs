using Microsoft.Extensions.DependencyInjection;
using SimModel.Model;

namespace SimModelTest.Model
{
    /// <summary>
    /// Equipmentのテスト
    /// </summary>
    public class EquipmentTest : TestDataSetUp
    {
        [Theory]
        [InlineData(3, 2, 1, 2, 1, 0, 5, EquipKind.head)]
        [InlineData(3, 2, 1, 3, 1, 0, 5, EquipKind.body)]
        [InlineData(3, 2, 0, 2, 1, 0, 6, EquipKind.arm)]
        [InlineData(3, 2, 0, 3, 1, 0, 6, EquipKind.waist)]
        [InlineData(2, 1, 0, 2, 1, 0, 7, EquipKind.leg)]
        [InlineData(2, 1, 0, 2, 1, 0, 5, EquipKind.charm)]
        [InlineData(2, 1, 0, 2, 1, 0, 5, EquipKind.deco)]
        [InlineData(2, 1, 0, 2, 1, 0, 5, EquipKind.error)]
        public void TranscendingSlotTest_normal(
            int expectedTranscendingSlot1, int expectedTranscendingSlot2, int expectedTranscendingSlot3,
            int slot1, int slot2, int slot3, int rare, EquipKind equipKind)
        {
            // テストデータ
            Equipment equip = new()
            {
                Slot1 = slot1,
                Slot2 = slot2,
                Slot3 = slot3,
                Rare = rare,
                Kind = equipKind
            };

            // テスト
            Assert.Equal(expectedTranscendingSlot1, equip.TranscendingSlot1);
            Assert.Equal(expectedTranscendingSlot2, equip.TranscendingSlot2);
            Assert.Equal(expectedTranscendingSlot3, equip.TranscendingSlot3);
        }

        /// <summary>
        /// TranscendingDefのテスト（正常系・指定なし）
        /// </summary>
        [Fact]
        public void TranscendingDefTest_init()
        {
            // テストデータ
            Equipment equip = new()
            {
                Maxdef = 10,
            };

            // テスト
            Assert.Equal(10, equip.TranscendingDef);
        }

        /// <summary>
        /// TranscendingDefのテスト（正常系・指定あり）
        /// </summary>
        [Fact]
        public void TranscendingDefTest_setted()
        {
            // テストデータ
            Equipment equip = new()
            {
                Maxdef = 10,
                TranscendingDef = 20
            };

            // テスト
            Assert.Equal(20, equip.TranscendingDef);
        }

        /// <summary>
        /// DispNameのテスト（正常系・指定なし）
        /// 現状はDetailDispNameも確認する
        /// </summary>
        [Fact]
        public void DispNameTest_init()
        {
            // テストデータ
            Equipment equip = new()
            {
                Name = "testname",
            };

            // テスト
            Assert.Equal("testname", equip.DispName);
            Assert.Equal("testname", equip.DetailDispName);
        }

        /// <summary>
        /// DispNameのテスト（正常系・指定あり）
        /// 現状はDetailDispNameも確認する
        /// </summary>
        [Fact]
        public void DispNameTest_setted()
        {
            // テストデータ
            Equipment equip = new()
            {
                Name = "testname",
                DispName = "dispname"
            };

            // テスト
            Assert.Equal("dispname", equip.DispName);
            Assert.Equal("dispname", equip.DetailDispName);
        }

        /// <summary>
        /// Descriptionのテスト
        /// </summary>
        [Theory]
        [InlineData("つよ腕,0-0-0\n防御:20→28→36,火:-10,水:-10,雷:5,氷:-10,龍:-10\nつよグループLv1\nつよの力Lv1\n防具スキルLv1\nつよ防具スキル全Lv1\nつよ防具スキル腕Lv1", "つよ腕")]
        [InlineData("セット腕,2-1-0→3-2-1\n防御:15→25→35,火:0,水:0,雷:0,氷:0,龍:0\n強欲グループLv1\n強欲の力Lv1\n防具スキルLv1\nつよ防具スキル腕Lv1", "セット腕")]
        [InlineData("武一珠【１】\nスロ1武器スキルLv1", "武一珠【１】")]
        [InlineData("", "dummy")]
        public void DescriptionTest_normal(string expected, string equipName)
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            var equip = masters.AllEquipments.Where(e => e.Name == equipName).FirstOrDefault() ?? new();
            
            // テスト
            Assert.Equal(expected, equip.Description);
        }

        /// <summary>
        /// SimpleDescriptionのテスト
        /// </summary>
        [Theory]
        [InlineData("腕：つよ腕,0-0-0", "つよ腕")]
        [InlineData("腕：セット腕,2-1-0→3-2-1", "セット腕")]
        [InlineData("装飾品：武一珠【１】", "武一珠【１】")]
        [InlineData("頭：", "dummy")]
        public void SimpleDescriptionTest_normal(string expected, string equipName)
        {
            // テストデータ
            var masters = ServiceProvider.GetRequiredService<Masters>();
            var equip = masters.AllEquipments.Where(e => e.Name == equipName).FirstOrDefault() ?? new();

            // テスト
            Assert.Equal(expected, equip.SimpleDescription);
        }


    }
}
