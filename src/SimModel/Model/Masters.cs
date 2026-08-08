using System;
using System.Collections.Generic;
using System.Linq;

namespace SimModel.Model
{
    /// <summary>
    /// 各種マスタ管理
    /// </summary>
    public class Masters
    {
        // Loadしたきり変更されないマスタはstaticで保持する
        // Saveする必要があるマスタはstaticで保持しない(テスト時にテスト同士で干渉しないように)

        /// <summary>
        /// スキルマスタ 本体はSkill.SkillMasterに保持される
        /// </summary>
        public static List<Skill> Skills {
            get
            {
                return Skill.SkillMaster;
            }
            set 
            {
                Skill.SkillMaster = value;
            }
        }

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
        public List<Equipment> AdditionalCharms { get; set; } = new();

        /// <summary>
        /// 追加護石組み合わせマスタ
        /// </summary>
        public static List<CharmCombo> ShiningCharmCombos { get; set; } = new();

        /// <summary>
        /// 追加護石スキル情報マスタ
        /// </summary>
        public static Dictionary<int, List<Skill>> ShiningCharmGroups { get; set; } = new();

        /// <summary>
        /// アーティアマスタ
        /// </summary>
        public List<Weapon> Artians { get; set; } = new();

        /// <summary>
        /// 装飾品マスタ
        /// </summary>
        public static List<Deco> Decos { get; set; } = new();

        /// <summary>
        /// 除外固定マスタ
        /// </summary>
        public List<Clude> Cludes { get; set; } = new();

        /// <summary>
        /// マイセットマスタ
        /// </summary>
        public List<EquipSet> MySets { get; set; } = new();

        /// <summary>
        /// 最近使ったスキルマスタ
        /// </summary>
        public List<string> RecentSkillNames { get; set; } = new();

        /// <summary>
        /// マイ検索条件マスタ
        /// </summary>
        public List<SearchCondition> MyConditions { get; set; } = new();

        /// <summary>
        /// 防御力差分マスタ
        /// </summary>
        public static Dictionary<int, DefUpgrade> DefUpgrades { get; set; } = new();

        /// <summary>
        /// 全装備キャッシュ
        /// </summary>
        private IEnumerable<Equipment>? _allEquipments = null;
        /// <summary>
        /// 全装備
        /// </summary>
        public IEnumerable<Equipment> AllEquipments { 
            get 
            {
                if (_allEquipments == null)
                {
                    _allEquipments = Weapons.Union(Artians).Union(Heads).Union(Bodys).Union(Arms).Union(Waists).Union(Legs).Union(Charms).Union(AdditionalCharms).Union(Decos);
                }
                return _allEquipments;
            }
        }

        /// <summary>
        /// 全装備キャッシュをクリア
        /// TODO: Artians、AdditionalCharmsの更新時に自動で呼び出される仕組みが望ましい
        /// </summary>
        public void ClearAllEquipmentsCache()
        {
            _allEquipments = null;
        }


        /// <summary>
        /// 装備名から装備を取得
        /// </summary>
        /// <param name="equipName">装備名</param>
        /// <returns>装備</returns>
        public Equipment GetEquipByName(string equipName)
        {
            string? name = equipName?.Trim();
            return AllEquipments.Where(equip => equip.Name == name).FirstOrDefault() ?? new Equipment();
        }

        // TODO: Skillに移管すべき？
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

        // TODO: Skillに移管すべき？
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
