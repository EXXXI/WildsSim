using SimModel.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimModel.Model
{
    /// <summary>
    /// 検索条件
    /// </summary>
    public class SearchCondition
    {
        /// <summary>
        /// スキルリスト
        /// </summary>
        public List<Skill> Skills { get; set; } = new();

        /// <summary>
        /// 武器が指定されているか否か
        /// </summary>
        public bool IsSpecificWeapon { get; set; }

        /// <summary>
        /// 武器名(武器指定時のみ有効)
        /// </summary>
        public string WeaponName { get; set; }

        /// <summary>
        /// 武器種(武器非指定時のみ有効)
        /// </summary>
        public WeaponType WeaponType { get; set; }

        /// <summary>
        /// 武器種(武器非指定時のみ有効)
        /// </summary>
        public int? MinAttack { get; set; }

        /// <summary>
        /// 防御力
        /// </summary>
        public int? Def { get; set; }

        /// <summary>
        /// 火耐性
        /// </summary>
        public int? Fire { get; set; }

        /// <summary>
        /// 水耐性
        /// </summary>
        public int? Water { get; set; }

        /// <summary>
        /// 雷耐性
        /// </summary>
        public int? Thunder { get; set; }

        /// <summary>
        /// 氷耐性
        /// </summary>
        public int? Ice { get; set; }

        /// <summary>
        /// 龍耐性
        /// </summary>
        public int? Dragon { get; set; }

        /// <summary>
        /// マイ検索条件保存用ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// マイ検索条件保存用名前
        /// </summary>
        public string DispName { get; set; }

        /// <summary>
        /// 護石検索用固定護石
        /// </summary>
        public Equipment? FixCharm { get; set; } = null;

        /// <summary>
        /// 理論値護石検索フラグ
        /// </summary>
        public bool IsBestCharmSearch { get; set; } = false;

        /// <summary>
        /// CSV用スキル形式
        /// </summary>
        public string SkillCSV
        {
            get
            {
                StringBuilder sb = new();
                bool isFirst = true;
                foreach (var skill in Skills)
                {
                    if (!isFirst)
                    {
                        sb.Append(',');
                    }
                    sb.Append(skill.Name);
                    sb.Append(',');
                    sb.Append(skill.Level);
                    if (skill.IsFixed)
                    {
                        sb.Append("固定");
                    }
                    isFirst = false;
                }
                return sb.ToString();
            }
            set
            {
                Skills = new List<Skill>();
                string[] splitted = value.Split(',');
                for (int i = 0; i < splitted.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(splitted[i]))
                    {
                        return;
                    }
                    string name = splitted[i];
                    string levelStr = splitted[++i];
                    bool isFixed = levelStr.EndsWith("固定");
                    levelStr = levelStr.Replace("固定", string.Empty);
                    Skill skill = new(name, ParseUtil.Parse(levelStr));
                    skill.IsFixed = isFixed;
                    Skills.Add(skill);
                }
            }
        }

        /// <summary>
        /// 表示用説明
        /// </summary>
        public string Description
        {
            get
            {
                string none = "なし";
                StringBuilder sb = new StringBuilder();
                if (IsSpecificWeapon)
                {
                    sb.AppendLine($"武器:{WeaponName}");
                }
                else
                {
                    sb.Append($"武器種:{WeaponType}, ");
                    sb.AppendLine($"最低攻撃力:{MinAttack}");
                }
                sb.AppendLine($"防御力:{Def?.ToString() ?? none}");
                sb.Append($"火:{Fire?.ToString() ?? none},");
                sb.Append($"水:{Water?.ToString() ?? none},");
                sb.Append($"雷:{Thunder?.ToString() ?? none},");
                sb.Append($"氷:{Ice?.ToString() ?? none},");
                sb.Append($"龍:{Dragon?.ToString() ?? none}");
                foreach (var skill in Skills)
                {
                    sb.AppendLine();
                    sb.Append($"{skill.Description}{(skill.IsFixed ? "(固定)" : string.Empty)}");
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public SearchCondition()
        {
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="condition"></param>
        public SearchCondition(SearchCondition condition)
        {
            Skills = new List<Skill>();
            foreach (var skill in condition.Skills)
            {
                Skill newSkill = new Skill(skill.Name, skill.Level, skill.IsFixed);
                Skills.Add(newSkill);
            }
            IsSpecificWeapon = condition.IsSpecificWeapon;
            WeaponName = condition.WeaponName;
            WeaponType = condition.WeaponType;
            MinAttack = condition.MinAttack; 
            Def = condition.Def;
            Fire = condition.Fire;
            Water = condition.Water;
            Thunder = condition.Thunder;
            Ice = condition.Ice;
            Dragon = condition.Dragon;
            IsBestCharmSearch = condition.IsBestCharmSearch;
        }

        /// <summary>
        /// スキル追加(同名スキルはレベルが高い方のみを採用、固定がある場合は固定が優先)
        /// </summary>
        /// <param name="additionalSkill">追加スキル</param>
        /// <returns>追加したスキルが有効だった場合true</returns>
        // 
        public bool AddSkill(Skill additionalSkill)
        {
            foreach (var skill in Skills)
            {
                if(skill.Name == additionalSkill.Name)
                {
                    // 固定フラグに差がある場合それを優先
                    if (!skill.IsFixed && additionalSkill.IsFixed)
                    {
                        skill.Level = additionalSkill.Level;
                        skill.IsFixed = additionalSkill.IsFixed; // true
                        return true;
                    }
                    if (skill.IsFixed && !additionalSkill.IsFixed)
                    {
                        return false;
                    }

                    // 固定フラグに差がない場合は高いほうを優先
                    if (skill.Level < additionalSkill.Level)
                    {
                        skill.Level = additionalSkill.Level;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            Skills.Add(additionalSkill);
            return true;
        }

        /// <summary>
        /// この検索条件に関連する理論値護石リストを生成
        /// </summary>
        /// <returns></returns>
        internal List<Equipment> MakeRelatedCharms()
        {
            List<Equipment> targetCharms = new();
            var condSkills = Skills.Where(skill => skill.Level != 0);
            foreach (var combo in Masters.ShiningCharmCombos)
            {
                var skill1List = Masters.ShiningCharmGroups[combo.Group1].Where(skill => condSkills.Any(s => s.Name == skill.Name)).Append(null);
                var skill2List = Masters.ShiningCharmGroups[combo.Group2].Where(skill => condSkills.Any(s => s.Name == skill.Name)).Append(null);
                var skill3List = Masters.ShiningCharmGroups[combo.Group3].Where(skill => condSkills.Any(s => s.Name == skill.Name)).Append(null);
                foreach (var skill1 in skill1List)
                {
                    foreach (var skill2 in skill2List)
                    {
                        foreach (var skill3 in skill3List)
                        {
                            Equipment charm = new()
                            {
                                Name = Guid.NewGuid().ToString(), // 一時的な名前
                                Kind = EquipKind.charm,
                                Rare = combo.Rare,
                                Slot1 = combo.Slot1,
                                Slot2 = combo.Slot2,
                                Slot3 = combo.Slot3,
                                SlotType1 = combo.SlotType1,
                                SlotType2 = combo.SlotType2,
                                SlotType3 = combo.SlotType3,
                                IsVirtual = true
                            };
                            if (skill1 != null)
                            {
                                charm.Skills.Add(new Skill(skill1.Name, skill1.Level));
                            }
                            if (skill2 != null && !charm.Skills.Any(s => s.Name == skill2.Name))
                            {
                                charm.Skills.Add(new Skill(skill2.Name, skill2.Level));
                            }
                            if (skill3 != null && !charm.Skills.Any(s => s.Name == skill3.Name))
                            {
                                charm.Skills.Add(new Skill(skill3.Name, skill3.Level));
                            }
                            charm.SetCharmDispName();
                            // 同じ性能の護石が既に登録されていないかチェック
                            bool isExist = false;
                            foreach (var exist in targetCharms)
                            {
                                if (IsSameCharm(charm, exist))
                                {
                                    isExist = true;
                                    break;
                                }
                            }
                            if (isExist)
                            {
                                continue;
                            }
                            targetCharms.Add(charm);
                        }
                    }
                }
            }

            return targetCharms;
        }

        /// <summary>
        /// 同じ性能の護石かチェック
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private bool IsSameCharm(Equipment left, Equipment right)
        {
            if (left.Skills.Count != right.Skills.Count) { return false; }
            if (left.Slot1 != right.Slot1) { return false; }
            if (left.Slot2 != right.Slot2) { return false; }
            if (left.Slot3 != right.Slot3) { return false; }
            if (left.SlotType1 != right.SlotType1) { return false; }
            if (left.SlotType2 != right.SlotType2) { return false; }
            if (left.SlotType3 != right.SlotType3) { return false; }
            foreach (var skill in left.Skills)
            {
                if (!right.Skills.Any(s => s.Name == skill.Name && s.Level == skill.Level))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
