using System.Collections.Generic;
using System.Linq;

namespace SimModel.Model
{
    /// <summary>
    /// スキル
    /// </summary>
    public record Skill
    {

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

        /// <summary>
        /// スキルのカテゴリ
        /// </summary>
        public string Category { get; init; }

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
        public Skill(string name, int level, bool isFixed = false) 
            : this(name, level, Masters.Skills.Where(s => s.Name == name).FirstOrDefault()?.Category, isFixed) { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">スキル名</param>
        /// <param name="level">レベル</param>
        /// <param name="category">カテゴリ</param>
        /// <param name="isFixed">固定検索フラグ</param>
        public Skill(string name, int level, string? category, bool isFixed = false)
        {
            Name = name;
            Level = level;
            IsFixed = isFixed;
            Category = string.IsNullOrEmpty(category) ? @"未分類" : category;
            SpecificNames = Masters.Skills.Where(s => s.Name == name).Select(s => s.SpecificNames).FirstOrDefault() ?? new();
        }

        /// <summary>
        /// 最大レベル
        /// マスタに存在しないスキルの場合0
        /// </summary>
        public int MaxLevel {
            get 
            {
                return Masters.SkillMaxLevel(Name);
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
                return SpecificNames.ContainsKey(Level) ? SpecificNames[Level] : $"{Name}Lv{Level}";
            }
        }
    }
}
