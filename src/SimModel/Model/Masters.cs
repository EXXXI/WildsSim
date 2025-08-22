using System;
using System.Collections.Generic;
using System.Linq;

namespace SimModel.Model
{
    /// <summary>
    /// 各種マスタ管理
    /// </summary>
    static public class Masters
    {
        /// <summary>
        /// スキルマスタ
        /// </summary>
        public static List<Skill> Skills { get; set; } = new();

        /// <summary>
        /// 武器マスタ
        /// </summary>
        public static List<Weapon> Weapons { get; set; } = new();

        /// <summary>
        /// 頭装備マスタ
        /// </summary>
        public static List<Equipment> Heads { get; set; } = new();

        /// <summary>
        /// 胴装備マスタ
        /// </summary>
        public static List<Equipment> Bodys { get; set; } = new();

        /// <summary>
        /// 腕装備マスタ
        /// </summary>
        public static List<Equipment> Arms { get; set; } = new();

        /// <summary>
        /// 腰装備マスタ
        /// </summary>
        public static List<Equipment> Waists { get; set; } = new();

        /// <summary>
        /// 足装備マスタ
        /// </summary>
        public static List<Equipment> Legs { get; set; } = new();

        /// <summary>
        /// 護石マスタ
        /// </summary>
        public static List<Equipment> Charms { get; set; } = new();

        /// <summary>
        /// 追加護石マスタ
        /// </summary>
        public static List<Equipment> AdditionalCharms { get; set; } = new();

        /// <summary>
        /// 追加護石組み合わせマスタ
        /// </summary>
        public static List<CharmCombo> AdditionalCharmCombos { get; set; } = new();

        /// <summary>
        /// 追加護石スキル情報マスタ
        /// </summary>
        public static Dictionary<int, List<Skill>> AdditionalCharmGroups { get; set; } = new();

        /// <summary>
        /// 装飾品マスタ
        /// </summary>
        public static List<Deco> Decos { get; set; } = new();

        /// <summary>
        /// 除外固定マスタ
        /// </summary>
        public static List<Clude> Cludes { get; set; } = new();

        /// <summary>
        /// マイセットマスタ
        /// </summary>
        public static List<EquipSet> MySets { get; set; } = new();

        /// <summary>
        /// 最近使ったスキルマスタ
        /// </summary>
        public static List<string> RecentSkillNames { get; set; } = new();

        /// <summary>
        /// マイ検索条件マスタ
        /// </summary>
        public static List<SearchCondition> MyConditions { get; set; } = new();

        /// <summary>
        /// 装備名から装備を取得
        /// </summary>
        /// <param name="equipName">装備名</param>
        /// <returns>装備</returns>
        public static Equipment GetEquipByName(string equipName)
        {
            string? name = equipName?.Trim();
            var equips = Weapons.Union(Heads).Union(Bodys).Union(Arms).Union(Waists).Union(Legs).Union(Charms).Union(AdditionalCharms).Union(Decos);
            return equips.Where(equip => equip.Name == name).FirstOrDefault() ?? new Equipment();
        }

        /// <summary>
        /// スキル名がマスタに存在するかチェック
        /// </summary>
        /// <param name="value">スキル名</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static bool IsSkillName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            string name = value.Trim();
            return Skills.Any(skill => skill.Name == name);
        }

        /// <summary>
        /// スキル名から最大レベルを算出
        /// マスタに存在しないスキルの場合0
        /// </summary>
        /// <param name="name">スキル名</param>
        /// <returns>最大レベル</returns>
        public static int SkillMaxLevel(string name)
        {
            foreach (var skill in Skills)
            {
                if (skill.Name == name)
                {
                    return skill.Level;
                }
            }
            return 0;
        }
    }
}
