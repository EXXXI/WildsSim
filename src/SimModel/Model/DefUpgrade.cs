using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Model
{
    /// <summary>
    /// 防御力強化情報
    /// </summary>
    public class DefUpgrade
    {
        /// <summary>
        /// 最大強化時の防御力差分
        /// </summary>
        public int UpgradeDef { get; set; } = 0;

        /// <summary>
        /// 限界突破強化時の防御力差分
        /// </summary>
        public int TranscendingDef { get; set; } = 0;

        public DefUpgrade(int upgrade, int transcending)
        {
            this.UpgradeDef = upgrade;
            this.TranscendingDef = transcending;
        }
    }
}
