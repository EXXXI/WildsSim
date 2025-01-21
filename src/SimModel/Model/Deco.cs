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

        /// <summary>
        /// カテゴリ
        /// </summary>
        public string DecoCateory { get; set; } = "未分類";
    }
}
