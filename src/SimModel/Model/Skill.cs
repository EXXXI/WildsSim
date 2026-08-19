using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SimModel.Model
{
    /// <summary>
    /// スキル
    /// </summary>
    public record Skill
    {
        // TODO: Skillをアプリ内で編集するような機能が必要になった場合再検討
        // その場合も通常起動時は問題ないと思われるが、テスト時にテスト同士で干渉する可能性がある
        /// <summary>
        /// スキルマスタ本体
        /// </summary>
        public static List<Skill> SkillMaster { get; set; } = new();

        /// <summary>
        /// 表示レベルを制限するカテゴリ名
        /// </summary>
        public static readonly List<string> DisplayRestrictCategories = new() { "グループスキル", "シリーズスキル" };

        /// <summary>
        /// スキル名
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// スキルレベル
        /// </summary>
        public int Level { get; set; } = 0;

        /// <summary>
        /// 固定検索フラグ
        /// </summary>
        public bool IsFixed { get; set; } = false;

        private string category = @"未分類";
        /// <summary>
        /// スキルのカテゴリ
        /// </summary>
        public string Category 
        {
            get
            {
                if (category == @"未分類")
                {
                    category = SkillMaster.Where(s => s.Name == Name).FirstOrDefault()?.Category ?? category;
                }
                return category;
            }
            set 
            {
                category = value;
            }
        }

        private bool? canWithArtian = null;
        /// <summary>
        /// アーティア武器に付与可能か否か
        /// </summary>
        public bool CanWithArtian
        {
            get
            {
                if (canWithArtian == null)
                {
                    canWithArtian = SkillMaster.Where(s => s.Name == Name).First().CanWithArtian;
                }
                return canWithArtian.Value;
            }
            set
            {
                canWithArtian = value;
            }
        }

        /// <summary>
        /// シリーズスキル等、レベルに特殊な名称がある場合ここに格納
        /// </summary>
        public Dictionary<int, string> SpecificNames { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">スキル名</param>
        /// <param name="level">レベル</param>
        /// <param name="isFixed">固定検索フラグ</param>
        public Skill(string name, int level, bool isFixed = false, bool? canWithArtian = null) 
            : this(name, level, SkillMaster.Where(s => s.Name == name).FirstOrDefault()?.Category, isFixed, canWithArtian) { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">スキル名</param>
        /// <param name="level">レベル</param>
        /// <param name="category">カテゴリ</param>
        /// <param name="isFixed">固定検索フラグ</param>
        public Skill(string name, int level, string? category, bool isFixed = false, bool? canWithArtian = null)
        {
            Name = name;
            Level = level;
            IsFixed = isFixed;
            if (canWithArtian != null)
            {
                CanWithArtian = canWithArtian.Value;
            }
            if (category != null)
            {
                Category = category;
            }
            SpecificNames = SkillMaster.Where(s => s.Name == name).Select(s => s.SpecificNames).FirstOrDefault() ?? new();
        }

        /// <summary>
        /// 最大レベル
        /// マスタに存在しないスキルの場合0
        /// </summary>
        public int MaxLevel {
            get 
            {
                foreach (var skill in SkillMaster)
                {
                    if (skill.Name == Name)
                    {
                        return skill.Level;
                    }
                }
                return 0;
            }
        }

        /// <summary>
        /// 表示用文字列
        /// </summary>
        public string Description
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name) || Level == 0)
                {
                    return string.Empty;
                }
                return SpecificNames.ContainsKey(Level) ? $"{SpecificNames[Level]}({Name}Lv{Level})" : $"{Name}Lv{Level}";
            }
        }

        /// <summary>
        /// 表示レベルを制限するか否か
        /// </summary>
        /// <param name="level">インスタンスと違うレベルを調べたい場合入力</param>
        /// <returns>制限する場合true</returns>
        public bool IsHideLevel(int? level = null)
        {
            return DisplayRestrictCategories.Contains(Category) && !SpecificNames.ContainsKey(level ?? Level);
        } 
    }
}
