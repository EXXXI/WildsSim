using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Model
{
    public class Weapon : Equipment
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="kind"></param>
        public Weapon() : base(EquipKind.weapon)
        {
        }

        /// <summary>
        /// 攻撃力
        /// </summary>
        public int Attack { get; set; } = 0;

        /// <summary>
        /// 武器種
        /// </summary>
        public WeaponType WeaponType { get; set; } = WeaponType.指定なし;

        /// <summary>
        /// アーティア武器として各種値を設定
        /// </summary>
        public void InitArtian()
        {
            Rare = 8;
            Slot1 = 3;
            Slot2 = 3;
            Slot3 = 3;
            SlotType1 = 1;
            SlotType2 = 1;
            SlotType3 = 1;
            Mindef = 0;
            Maxdef = Mindef; // 防御力の変動はない
            Attack = 190;
            RowNo = int.MaxValue;
        }
    }
}
