using System.Linq;

namespace SimModel.Model
{
    /// <summary>
    /// 装飾品
    /// 装備(Equipment)を継承
    /// </summary>
    public class Deco : Equipment
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="kind"></param>
        public Deco() : base(EquipKind.deco)
        {
        }

        /// <summary>
        /// 所持数
        /// </summary>
        public int DecoCount { get; set; } = 0;

        private string? decoCategory = null;
        /// <summary>
        /// カテゴリ
        /// </summary>
        public string DecoCategory 
        { 
            get
            {
                if (decoCategory != null)
                {
                    return decoCategory;
                }
                else if (Skills.Count > 0) {
                    return Skills[0].Category;
                }
                else
                {
                    return "未分類";
                }
            }
            set 
            { 
                decoCategory = value;
            } 
        }
    }
}
