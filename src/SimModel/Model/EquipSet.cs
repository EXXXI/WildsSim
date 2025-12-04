using SimModel.Config;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimModel.Model
{
    /// <summary>
    /// 装備セット
    /// </summary>
    public class EquipSet
    {
        /// <summary>
        /// スロット不正時文字列
        /// </summary>
        private const string InvalidSlot = "invalid";

        /// <summary>
        /// 武器装備
        /// </summary>
        public Weapon Weapon { get; set; } = new Weapon();

        /// <summary>
        /// 頭装備
        /// </summary>
        public Equipment Head { get; set; } = new Equipment(EquipKind.head);

        /// <summary>
        /// 胴装備
        /// </summary>
        public Equipment Body { get; set; } = new Equipment(EquipKind.body);

        /// <summary>
        /// 腕装備
        /// </summary>
        public Equipment Arm { get; set; } = new Equipment(EquipKind.arm);

        /// <summary>
        /// 腰装備
        /// </summary>
        public Equipment Waist { get; set; } = new Equipment(EquipKind.waist);

        /// <summary>
        /// 足装備
        /// </summary>
        public Equipment Leg { get; set; } = new Equipment(EquipKind.leg);

        /// <summary>
        /// 護石
        /// </summary>
        public Equipment Charm { get; set; } = new Equipment(EquipKind.charm);

        /// <summary>
        /// 装飾品(リスト)
        /// </summary>
        public List<Equipment> Decos { get; set; } = new();

        /// <summary>
        /// マイセット用名前
        /// </summary>
        public string Name { get; set; } = LogicConfig.Instance.DefaultMySetName;

        /// <summary>
        /// 限界突破有無
        /// </summary>
        public bool IsTranscending { get; set; } = false;

        /// <summary>
        /// 合計パラメータ計算用装備一覧
        /// </summary>
        private List<Equipment> Equipments
        {
            get
            {
                List<Equipment> ret = new List<Equipment>();
                ret.Add(Weapon);
                ret.Add(Head);
                ret.Add(Body);
                ret.Add(Arm);
                ret.Add(Waist);
                ret.Add(Leg);
                ret.Add(Charm);
                foreach (var deco in Decos)
                {
                    ret.Add(deco);
                }
                return ret;
            }
        }

        /// <summary>
        /// 初期防御力
        /// </summary>
        public int Mindef
        {
            get
            {
                int ret = 0;
                foreach (var equip in Equipments)
                {
                    ret += equip.Mindef;
                }
                return ret;
            }
        }

        /// <summary>
        /// 最大防御力
        /// </summary>
        public int Maxdef
        {
            get
            {
                int ret = 0;
                foreach (var equip in Equipments)
                {
                    if (IsTranscending)
                    {
                        ret += equip.TranscendingDef;
                    }
                    else
                    {
                        ret += equip.Maxdef;
                    }
                }
                return ret;
            }
        }

        /// <summary>
        /// 火耐性
        /// </summary>
        public int Fire
        {
            get
            {
                int ret = 0;
                foreach (var equip in Equipments)
                {
                    ret += equip.Fire;
                }
                return ret;
            }
        }

        /// <summary>
        /// 水耐性
        /// </summary>
        public int Water
        {
            get
            {
                int ret = 0;
                foreach (var equip in Equipments)
                {
                    ret += equip.Water;
                }
                return ret;
            }
        }

        /// <summary>
        /// 雷耐性
        /// </summary>
        public int Thunder
        {
            get
            {
                int ret = 0;
                foreach (var equip in Equipments)
                {
                    ret += equip.Thunder;
                }
                return ret;
            }
        }

        /// <summary>
        /// 氷耐性
        /// </summary>
        public int Ice
        {
            get
            {
                int ret = 0;
                foreach (var equip in Equipments)
                {
                    ret += equip.Ice;
                }
                return ret;
            }
        }

        /// <summary>
        /// 龍耐性
        /// </summary>
        public int Dragon
        {
            get
            {
                int ret = 0;
                foreach (var equip in Equipments)
                {
                    ret += equip.Dragon;
                }
                return ret;
            }
        }

        /// <summary>
        /// スキル(リスト)
        /// </summary>
        public List<Skill> Skills
        {
            get
            {
                List<Skill> ret = new List<Skill>();
                foreach (var equip in Equipments)
                {
                    JoinSkill(ret, equip.Skills);
                }

                // スキルレベル最大値調整
                foreach (var skill in ret)
                {
                    if (skill.Level > skill.MaxLevel)
                    {
                        skill.Level = skill.MaxLevel;
                    }
                }

                ret.Sort((a, b) => b.Level - a.Level);
                return ret;
            }
        }



        /// <summary>
        /// 制約式名称用の、装飾品を除いたCSV表記
        /// </summary>
        public string GlpkRowName
        {
            get
            {
                StringBuilder sb = new();
                sb.Append(Weapon.Name);
                sb.Append(',');
                sb.Append(Head.Name);
                sb.Append(',');
                sb.Append(Body.Name);
                sb.Append(',');
                sb.Append(Arm.Name);
                sb.Append(',');
                sb.Append(Waist.Name);
                sb.Append(',');
                sb.Append(Leg.Name);
                sb.Append(',');
                sb.Append(Charm.Name);

                return sb.ToString();
            }
        }

        /// <summary>
        /// 存在している装備を一覧で返す(GLPK用)
        /// </summary>
        /// <returns>リスト</returns>
        public List<Equipment> ExistingEquipsWithOutDecos()
        {
            List<Equipment> list = new();
            if (!string.IsNullOrWhiteSpace(Weapon.Name))
            {
                list.Add(Weapon);
            }
            if (!string.IsNullOrWhiteSpace(Head.Name))
            {
                list.Add(Head);
            }
            if (!string.IsNullOrWhiteSpace(Body.Name))
            {
                list.Add(Body);
            }
            if (!string.IsNullOrWhiteSpace(Arm.Name))
            {
                list.Add(Arm);
            }
            if (!string.IsNullOrWhiteSpace(Waist.Name))
            {
                list.Add(Waist);
            }
            if (!string.IsNullOrWhiteSpace(Leg.Name))
            {
                list.Add(Leg);
            }
            if (!string.IsNullOrWhiteSpace(Charm.Name))
            {
                list.Add(Charm);
            }
            return list;
        }


        /// <summary>
        /// 装飾品のCSV表記 Set可能
        /// </summary>
        public string DecoNameCSV
        {
            get
            {
                StringBuilder sb = new();
                bool isFirst = true;
                foreach (var deco in Decos)
                {
                    if (!isFirst)
                    {
                        sb.Append(',');
                    }
                    sb.Append(deco.Name);
                    isFirst = false;
                }
                return sb.ToString();
            }
            set
            {
                Decos = new List<Equipment>();
                string[] splitted = value.Split(',');
                foreach (var decoName in splitted)
                {
                    if (string.IsNullOrWhiteSpace(decoName))
                    {
                        continue;
                    }
                    Equipment? deco = Masters.GetEquipByName(decoName);
                    if (deco != null)
                    {
                        Decos.Add(deco);
                    }
                }
                SortDecos();
            }
        }

        /// <summary>
        /// 装飾品のCSV表記 3行
        /// </summary>
        public string DecoNameCSVMultiLine
        {
            get
            {
                StringBuilder sb = new();
                int secondLineIdx = Decos.Count / 3;
                int thirdLineIdx = Decos.Count * 2 / 3;
                for (int i = 0; i < Decos.Count; i++)
                {
                    if (i == 0)
                    {
                        // 処理なし
                    }
                    else if (i == secondLineIdx)
                    {
                        sb.Append(',');
                        sb.Append('\n');
                    }
                    else if (i == thirdLineIdx)
                    {
                        sb.Append(',');
                        sb.Append('\n');
                    }
                    else
                    {
                        sb.Append(',');
                    }
                    sb.Append(Decos[i].Name);
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// スキルのCSV形式
        /// </summary>
        public string SkillsDisp
        {
            get
            {
                List<Skill> existSkill = Skills.Where(s => s.Level > 0).ToList();

                StringBuilder sb = new();
                bool first = true;
                foreach (var skill in existSkill)
                {
                    if (skill.Level > 0)
                    {
                        if (!first)
                        {
                            sb.Append(", ");
                        }
                        sb.Append(skill.Description);
                        first = false;
                    }
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 装飾品のCSV表記 3行
        /// </summary>
        public string SkillsDispMultiLine
        {
            get
            {
                List<Skill> existSkill = Skills.Where(s => s.Level > 0).ToList();

                StringBuilder sb = new();
                int secondLineIdx = existSkill.Count / 3;
                int thirdLineIdx = existSkill.Count * 2 / 3;
                for (int i = 0; i < existSkill.Count; i++)
                {
                    if (i == 0)
                    {
                        // 処理なし
                    }
                    else if (i == secondLineIdx)
                    {
                        sb.Append(',');
                        sb.Append('\n');
                    }
                    else if (i == thirdLineIdx)
                    {
                        sb.Append(',');
                        sb.Append('\n');
                    }
                    else
                    {
                        sb.Append(',');
                    }
                    sb.Append(existSkill[i].Description);
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 装備の説明
        /// </summary>
        public string Description
        {
            get
            {
                StringBuilder sb = new();
                sb.Append("防御:");
                sb.Append(Mindef);
                sb.Append('→');
                sb.Append(Maxdef);
                if (IsTranscending)
                {
                    sb.Append("(限界突破)");
                }
                sb.Append(',');
                sb.Append("火:");
                sb.Append(Fire);
                sb.Append(',');
                sb.Append("水:");
                sb.Append(Water);
                sb.Append(',');
                sb.Append("雷:");
                sb.Append(Thunder);
                sb.Append(',');
                sb.Append("氷:");
                sb.Append(Ice);
                sb.Append(',');
                sb.Append("龍:");
                sb.Append(Dragon);
                sb.Append('\n');
                sb.Append(Weapon.SimpleDescription);
                sb.Append('\n');
                sb.Append(Head.SimpleDescription);
                sb.Append('\n');
                sb.Append(Body.SimpleDescription);
                sb.Append('\n');
                sb.Append(Arm.SimpleDescription);
                sb.Append('\n');
                sb.Append(Waist.SimpleDescription);
                sb.Append('\n');
                sb.Append(Leg.SimpleDescription);
                sb.Append('\n');
                sb.Append(Charm.SimpleDescription);
                sb.Append('\n');
                sb.Append(EquipKind.deco.StrWithColon());
                sb.Append(DecoNameCSV);
                sb.Append('\n'); 
                sb.Append("-----------");
                foreach (var skill in Skills)
                {
                    if (skill.Level > 0)
                    {
                        sb.Append('\n');
                        sb.Append(skill.Description);
                    }
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 装飾品のソート
        /// </summary>
        public void SortDecos()
        {
            List<Equipment> newDecos = new List<Equipment>();
            for (int i = 4; i > 0; i--)
            {
                foreach (var deco in Decos)
                {
                    if (deco.Slot1 == i)
                    {
                        newDecos.Add(deco);
                    }
                }
            }
            Decos = newDecos;
        }

        /// <summary>
        /// スキルの追加(同名スキルはスキルレベルを加算)
        /// 最大値のチェックはここではしていない
        /// </summary>
        /// <param name="baseSkills">スキル一覧</param>
        /// <param name="newSkills">追加するスキル</param>
        /// <returns>合わせたスキル一覧</returns>
        private List<Skill> JoinSkill(List<Skill> baseSkills, List<Skill> newSkills)
        {
            foreach (var newSkill in newSkills)
            {
                if (string.IsNullOrWhiteSpace(newSkill.Name))
                {
                    continue;
                }

                bool exist = false;
                foreach (var baseSkill in baseSkills)
                {
                    if (baseSkill.Name.Equals(newSkill.Name))
                    {
                        exist = true;
                        baseSkill.Level += newSkill.Level;
                    }
                }
                if (!exist)
                {
                    baseSkills.Add(new Skill(newSkill.Name, newSkill.Level));
                }
            }
            return baseSkills;
        }
    }
}
