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
    }
}
