using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Model
{
    public class CharmCombo
    {
        /// <summary>
        /// レア度
        /// </summary>
        public int Rare { get; set; }

        /// <summary>
        /// グループ1
        /// </summary>
        public int Group1 { get; set; }

        /// <summary>
        /// グループ2
        /// </summary>
        public int Group2 { get; set; }

        /// <summary>
        /// グループ3
        /// </summary>
        public int Group3 { get; set; }

        /// <summary>
        /// スロット1つ目
        /// </summary>
        public int Slot1 { get; set; }

        /// <summary>
        /// スロット2つ目
        /// </summary>
        public int Slot2 { get; set; }

        /// <summary>
        /// スロット3つ目
        /// </summary>
        public int Slot3 { get; set; }

        /// <summary>
        /// スロットタイプ(0:防御スキルのみ,1:攻撃スキルのみ,2:両方可)1つ目
        /// </summary>
        public int SlotType1 { get; set; } = 0;

        /// <summary>
        /// スロットタイプ(0:防御スキルのみ,1:攻撃スキルのみ,2:両方可)2つ目
        /// </summary>
        public int SlotType2 { get; set; } = 0;

        /// <summary>
        /// スロットタイプ(0:防御スキルのみ,1:攻撃スキルのみ,2:両方可)3つ目
        /// </summary>
        public int SlotType3 { get; set; } = 0;
    }
}
