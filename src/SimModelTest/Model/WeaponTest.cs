using SimModel.Model;

namespace SimModelTest.Model
{
    /// <summary>
    /// Weaponのテスト
    /// </summary>
    public class WeaponTest
    {
        /// <summary>
        /// InitArtianのテスト
        /// </summary>
        [Fact]
        public void InitArtianTest()
        {
            // テストデータ
            Weapon weapon = new();
            weapon.InitArtian();

            // テスト
            Assert.Equal(8, weapon.Rare);
            Assert.Equal(3, weapon.Slot1);
            Assert.Equal(3, weapon.Slot2);
            Assert.Equal(3, weapon.Slot3);
            Assert.Equal(1, weapon.SlotType1);
            Assert.Equal(1, weapon.SlotType2);
            Assert.Equal(1, weapon.SlotType3);
            Assert.Equal(0, weapon.Mindef);
            Assert.Equal(0, weapon.Maxdef);
            Assert.Equal(190, weapon.Attack);
            Assert.Equal(int.MaxValue, weapon.RowNo);
        }
    }
}
