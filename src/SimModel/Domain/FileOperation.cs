using Csv;
using Google.OrTools.ConstraintSolver;
using NLog;
using SimModel.Config;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SimModel.Domain
{
    /// <summary>
    /// CSV・Json操作クラス
    /// </summary>
    public class FileOperation
    {
        // 定数：ファイルパス
        private const string SkillCsv = "MHWilds_SKILL.csv";
        private const string HeadCsv = "MHWilds_EQUIP_HEAD.csv";
        private const string BodyCsv = "MHWilds_EQUIP_BODY.csv";
        private const string ArmCsv = "MHWilds_EQUIP_ARM.csv";
        private const string WaistCsv = "MHWilds_EQUIP_WST.csv";
        private const string LegCsv = "MHWilds_EQUIP_LEG.csv";
        private const string CharmCsv = "MHWilds_CHARM.csv";
        private const string DecoCsv = "MHWilds_DECO.csv";
        private const string WeaponCsv = "MHWilds_WEAPON.csv";
        private const string SaveFolder = "save";
        private const string DecoCountJson = SaveFolder + "/decocount.json";
        private const string CludeCsv = SaveFolder + "/clude.csv";
        private const string MySetCsv = SaveFolder + "/myset.csv";
        private const string RecentSkillCsv = SaveFolder + "/recentSkill.csv";
        private const string ConditionCsv = SaveFolder + "/condition.csv";
        private const string AdditionalCharmCsv = SaveFolder + "/additionalCharm.csv";
        private const string ArtianCsv = SaveFolder + "/artian.csv";
        private const string ShiningCharmComboCsv = "MHWilds_COMBO_SHININGCHARM.csv";
        private const string ShiningCharmGroupCsv = "MHWilds_GROUP_SHININGCHARM.csv";
        private const string DefUpgradeCsv = "MHWilds_DEF_UPGRADE.csv";

        private const string SkillMasterHeaderName = @"スキル系統";
        private const string SkillMasterHeaderRequiredPoints = @"必要ポイント";
        private const string SkillMasterHeaderCategory = @"カテゴリ";
        private const string SkillMasterHeaderCost = @"コスト";
        private const string SkillMasterHeaderSpecificName = @"発動スキル";

        static private Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// スキルマスタ読み込み
        /// </summary>
        internal List<Skill> LoadSkillCSV()
        {
            string csv = ReadAllText(SkillCsv);

            var skills = CsvReader.ReadFromText(csv)
                .Select(line => new
                {
                    Name = line[SkillMasterHeaderName],
                    Level = ParseUtil.Parse(line[SkillMasterHeaderRequiredPoints]),
                    Category = line[SkillMasterHeaderCategory],
                    CanWithArtian = ParseUtil.Parse(line[@"アーティア対応"]) == 1
                })
                // マスタのCSVにある同名スキルのうち、スキルレベルが最大のものだけを選ぶ
                .GroupBy(x => new { x.Name, x.Category })
                .Select(group => new Skill(group.Key.Name, group.Max(x => x.Level), group.Key.Category, canWithArtian:group.Last().CanWithArtian))
                .ToList();

            // 特殊な名称のデータを保持
            var hasSpecificNames = CsvReader.ReadFromText(csv)
                .Select(line => new
                {
                    Name = line[SkillMasterHeaderName],
                    Level = ParseUtil.Parse(line[SkillMasterHeaderRequiredPoints]),
                    Specific = line[SkillMasterHeaderSpecificName]
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Specific));
            foreach (var item in hasSpecificNames)
            {
                Skill skill = skills.First(s => s.Name == item.Name);
                skill.SpecificNames.Add(item.Level, item.Specific);
            }

            return skills;
        }

        /// <summary>
        /// 武器マスタ読み込み
        /// </summary>
        internal List<Weapon> LoadWeaponCSV()
        {
            List<Weapon> weapons = new();

            // 汎用スロット作成
            int maxSize = LogicConfig.Instance.MaxSlotSize;
            for (int i = 0; i <= maxSize; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    for (int k = 0; k <= j; k++)
                    {
                        Weapon weapon = new()
                        {
                            Name = $"スロットのみ_{i}-{j}-{k}",
                            Slot1 = i,
                            Slot2 = j,
                            Slot3 = k,
                            SlotType1 = 1,
                            SlotType2 = 1,
                            SlotType3 = 1
                        };
                        weapons.Add(weapon);
                    }
                }
            }

            // csv読み込み
            string csv = ReadAllText(WeaponCsv);
            var x = CsvReader.ReadFromText(csv);
            foreach (ICsvLine line in x)
            {
                // 入手不可データは読み飛ばす
                string period = line[@"入手時期"];
                if (period == "99" && !LogicConfig.Instance.AllowUnavailableEquipments)
                {
                    continue;
                }

                Weapon weapon = new Weapon();
                weapon.WeaponType = (WeaponType)Enum.Parse(typeof(WeaponType), line[@"武器種"]);
                weapon.Name = line[@"名前"];
                weapon.Rare = ParseUtil.Parse(line[@"レア度"]);
                weapon.Slot1 = ParseUtil.Parse(line[@"スロット1"]);
                weapon.Slot2 = ParseUtil.Parse(line[@"スロット2"]);
                weapon.Slot3 = ParseUtil.Parse(line[@"スロット3"]);
                weapon.SlotType1 = 1;
                weapon.SlotType2 = 1;
                weapon.SlotType3 = 1;
                weapon.Mindef = ParseUtil.Parse(line[@"防御ボーナス"]);
                weapon.Maxdef = weapon.Mindef; // 防御力の変動はない
                weapon.Attack = ParseUtil.Parse(line[@"表示攻撃力"]); 
                weapon.RowNo = ParseUtil.Parse(line[@"仮番号"], int.MaxValue);
                List<Skill> skills = new List<Skill>();
                for (int i = 1; i <= LogicConfig.Instance.MaxEquipSkillCount; i++)
                {
                    string skill = line[@"スキル系統" + i];
                    string level = line[@"スキル値" + i];
                    if (string.IsNullOrWhiteSpace(skill))
                    {
                        break;
                    }
                    skills.Add(new Skill(skill, ParseUtil.Parse(level)));
                }
                weapon.Skills = skills;

                weapons.Add(weapon);
            }

            return weapons;
        }

        /// <summary>
        /// 頭防具マスタ読み込み
        /// </summary>
        internal List<Equipment> LoadHeadCSV(Dictionary<int, DefUpgrade> defUpgrades)
        {
            return LoadEquipCSV(HeadCsv, EquipKind.head, defUpgrades);
        }

        /// <summary>
        /// 胴防具マスタ読み込み
        /// </summary>
        internal List<Equipment> LoadBodyCSV(Dictionary<int, DefUpgrade> defUpgrades)
        {
            return LoadEquipCSV(BodyCsv, EquipKind.body, defUpgrades);
        }

        /// <summary>
        /// 腕防具マスタ読み込み
        /// </summary>
        internal List<Equipment> LoadArmCSV(Dictionary<int, DefUpgrade> defUpgrades)
        {
            return LoadEquipCSV(ArmCsv, EquipKind.arm, defUpgrades);
        }

        /// <summary>
        /// 腰防具マスタ読み込み
        /// </summary>
        internal List<Equipment> LoadWaistCSV(Dictionary<int, DefUpgrade> defUpgrades)
        {
            return LoadEquipCSV(WaistCsv, EquipKind.waist, defUpgrades);
        }

        /// <summary>
        /// 足防具マスタ読み込み
        /// </summary>
        internal List<Equipment> LoadLegCSV(Dictionary<int, DefUpgrade> defUpgrades)
        {
            return LoadEquipCSV(LegCsv, EquipKind.leg, defUpgrades);
        }

        /// <summary>
        /// 護石マスタ読み込み
        /// </summary>
        internal List<Equipment> LoadCharmCSV()
        {
            return LoadEquipCSV(CharmCsv, EquipKind.charm);
        }

        /// <summary>
        /// 防具マスタ読み込み
        /// </summary>
        /// <param name="fileName">CSVファイル名</param>
        /// <param name="equipments">格納先</param>
        /// <param name="kind">部位</param>
        private List<Equipment> LoadEquipCSV(string fileName, EquipKind kind, Dictionary<int, DefUpgrade>? defUpgrades = null)
        {
            List<Equipment> equips = new();
            string csv = ReadAllText(fileName);
            var x = CsvReader.ReadFromText(csv);
            foreach (ICsvLine line in x)
            {
                // 入手不可データは読み飛ばす
                string period = line[@"入手時期"];
                if (period == "99" && !LogicConfig.Instance.AllowUnavailableEquipments)
                {
                    continue;
                }

                Equipment equip = new Equipment(kind);
                    equip.Name = line[@"名前"];
                equip.Rare = ParseUtil.Parse(line[@"レア度"]);
                if (kind != EquipKind.charm)
                {
                    equip.Slot1 = ParseUtil.Parse(line[@"スロット1"]);
                    equip.Slot2 = ParseUtil.Parse(line[@"スロット2"]);
                    equip.Slot3 = ParseUtil.Parse(line[@"スロット3"]);
                    equip.Mindef = ParseUtil.Parse(line[@"初期防御力"]);
                    int maxdef = CalcMaxdef(equip.Rare, equip.Mindef, equip.Kind, defUpgrades);
                    equip.Maxdef = ParseUtil.Parse(line[@"最終防御力"], maxdef); // 指定がある場合指定を優先
                    int transcendingDef = CalcTranscendingDef(equip.Rare, equip.Maxdef, equip.Kind, defUpgrades);
                    equip.TranscendingDef = transcendingDef;
                    equip.Fire = ParseUtil.Parse(line[@"火耐性"]);
                    equip.Water = ParseUtil.Parse(line[@"水耐性"]);
                    equip.Thunder = ParseUtil.Parse(line[@"雷耐性"]);
                    equip.Ice = ParseUtil.Parse(line[@"氷耐性"]);
                    equip.Dragon = ParseUtil.Parse(line[@"龍耐性"]);
                }
                // TODO: 次回作ではこうならないようにワンセットの機能をデフォで入れておく
                // 互換性のため、lineが"ワンセット"を要素に持っていることを確認
                equip.IsOneSet = line.HasColumn(@"ワンセット") && (line[@"ワンセット"] == "1");
                equip.RowNo = ParseUtil.Parse(line[@"仮番号"], int.MaxValue);
                List<Skill> skills = new List<Skill>();
                for (int i = 1; i <= LogicConfig.Instance.MaxEquipSkillCount; i++)
                {
                    string skill = line[@"スキル系統" + i];
                    string level = line[@"スキル値" + i];
                    if (string.IsNullOrWhiteSpace(skill))
                    {
                        break;
                    }
                    skills.Add(new Skill(skill, ParseUtil.Parse(level)));
                }
                equip.Skills = skills;
                //// 防具のスロットタイプ指定
                //if (line.HasColumn(@"スロット1タイプ"))// if (kind == EquipKind.charm)
                //{
                //    charm.SlotType1 = ParseUtil.Parse(line[@"スロット1タイプ"]);
                //    charm.SlotType2 = ParseUtil.Parse(line[@"スロット2タイプ"]);
                //    charm.SlotType3 = ParseUtil.Parse(line[@"スロット3タイプ"]);
                //}

                equips.Add(equip);
            }
            return equips;
        }

        /// <summary>
        /// 最大防御力をレア度から算出する
        /// </summary>
        /// <param name="rare">レア度</param>
        /// <param name="mindef">最低防御力</param>
        /// <param name="kind">防具種類</param>
        /// <returns>レア度から算出した最大防御力</returns>
        private int CalcMaxdef(int rare, int mindef, EquipKind kind, Dictionary<int, DefUpgrade>? defUpgrades = null)
        {
            if (kind == EquipKind.charm || defUpgrades == null)
            {
                return mindef;
            }
            bool getted = defUpgrades.TryGetValue(rare, out DefUpgrade? defUpgrade);
            if (getted && defUpgrade != null)
            {
                return mindef + defUpgrade.UpgradeDef;
            }
            return mindef;
        }

        /// <summary>
        /// 限界突破防御力をレア度から算出する
        /// </summary>
        /// <param name="rare">レア度</param>
        /// <param name="maxdef">最大防御力</param>
        /// <param name="kind">防具種類</param>
        /// <returns>レア度から算出した限界突破防御力</returns>
        private int CalcTranscendingDef(int rare, int maxdef, EquipKind kind, Dictionary<int, DefUpgrade>? defUpgrades = null)
        {
            if (kind == EquipKind.charm || defUpgrades == null)
            {
                return maxdef;
            }
            bool getted = defUpgrades.TryGetValue(rare, out DefUpgrade? defUpgrade);
            if (getted && defUpgrade != null)
            {
                return maxdef + defUpgrade.TranscendingDef;
            }
            return maxdef;
        }

        /// <summary>
        /// 装飾品マスタ読み込み
        /// </summary>
        internal List<Deco> LoadDecoCSV()
        {
            List<Deco> decos = new();

            string csv = ReadAllText(DecoCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                // 入手不可データは読み飛ばす
                string period = line[@"入手時期"];
                if (period == "99" && !LogicConfig.Instance.AllowUnavailableEquipments)
                {
                    continue;
                }

                Deco equip = new Deco();
                equip.Name = line[@"名前"];
                equip.Rare = ParseUtil.Parse(line[@"レア度"]);
                equip.Slot1 = ParseUtil.Parse(line[@"スロットサイズ"]);
                equip.Slot2 = 0;
                equip.Slot3 = 0;
                equip.SlotType1 = ParseUtil.Parse(line[@"スロットタイプ"]);
                equip.Mindef = 0;
                equip.Maxdef = 0;
                equip.Fire = 0;
                equip.Water = 0;
                equip.Thunder = 0;
                equip.Ice = 0;
                equip.Dragon = 0;
                List<Skill> skills = new List<Skill>();
                for (int i = 1; i <= LogicConfig.Instance.MaxDecoSkillCount; i++)
                {
                    string skill = line[@"スキル系統" + i];
                    string level = line[@"スキル値" + i];
                    if (string.IsNullOrWhiteSpace(skill))
                    {
                        break;
                    }
                    skills.Add(new Skill(skill, ParseUtil.Parse(level)));
                }
                equip.Skills = skills;

                // 所持数の初期値(泣シミュに準拠)
                if (equip.Slot1 == 4)
                {
                    equip.DecoCount = 0;
                }
                else
                {
                    equip.DecoCount = 7;
                }

                // カテゴリ
                if (skills.Count > 1)
                {
                    equip.DecoCateory = $"{skills[0].Name}複合";
                }
                else
                {
                    equip.DecoCateory = skills[0].Category;
                }

                decos.Add(equip);
            }
            
            decos = LoadDecoCountJson(decos);

            return decos;
        }

        /// <summary>
        /// 装飾品所持数読み込み
        /// </summary>
        private List<Deco> LoadDecoCountJson(List<Deco> decos)
        {
            string json = ReadAllText(DecoCountJson);
            if (string.IsNullOrWhiteSpace(json))
            {
                return decos;
            }

            JsonSerializerOptions options = new();
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All);
            Dictionary<string, int>? decoCounts = JsonSerializer.Deserialize<Dictionary<string, int>>(json, options);

            foreach (var deco in decos)
            {
                deco.DecoCount = decoCounts?.Where(dc => deco.Name == dc.Key).Select(dc => dc.Value).FirstOrDefault() ?? 0;
            }

            return decos;
        }

        /// <summary>
        /// 装飾品所持数書き込み
        /// </summary>
        public void SaveDecoCountJson(List<Deco> decos)
        {
            Dictionary<string, int> data = new();
            foreach (var deco in decos)
            {
                data.Add(deco.Name, deco.DecoCount);
            }
            JsonSerializerOptions options = new();
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All);
            string json = JsonSerializer.Serialize(data, options);

            File.WriteAllText(DecoCountJson, json);
        }

        /// <summary>
        /// 除外固定マスタ書き込み
        /// </summary>
        internal void SaveCludeCSV(List<Clude> cludes)
        {
            List<string[]> body = new List<string[]>();
            foreach (var clude in cludes)
            {
                string kind = "0";
                if (clude.Kind.Equals(CludeKind.include))
                {
                    kind = "1";
                }
                body.Add(new string[] { clude.Name, kind });
            }

            string export = CsvWriter.WriteToText(new string[] { "対象", "種別" }, body);
            File.WriteAllText(CludeCsv, export);

        }

        /// <summary>
        /// 除外固定マスタ読み込み
        /// </summary>
        internal List<Clude> LoadCludeCSV()
        {
            List<Clude> cludes = new();

            string csv = ReadAllText(CludeCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                Clude clude = new Clude
                {
                    Name = line[@"対象"],
                    Kind = (CludeKind)ParseUtil.Parse(line[@"種別"])
                };

                cludes.Add(clude);
            }
            return cludes;
        }

        /// <summary>
        /// マイセットマスタ書き込み
        /// </summary>
        internal void SaveMySetCSV(List<EquipSet> mySets)
        {
            List<string[]> body = new List<string[]>();
            foreach (var set in mySets)
            {
                body.Add(new string[] { set.Weapon.Name, set.Head.Name, set.Body.Name, set.Arm.Name, set.Waist.Name, set.Leg.Name, set.Charm.Name, set.DecoNameCSV, set.Name, set.IsTranscending ? "1" : "" });
            }
            string[] header = new string[] { "武器", "頭", "胴", "腕", "腰", "足", "護石", "装飾品", "名前", "限界突破有無" };
            string export = CsvWriter.WriteToText(header, body);
            File.WriteAllText(MySetCsv, export);

            // マイセット利用状況の反映のため護石、アーティアを再書き込みが必要だが、呼び出し側(DataManagement)で行うことにする
            //SaveAdditionalCharmCSV();
            //SaveArtianCSV();
        }

        /// <summary>
        /// マイセットマスタ読み込み
        /// </summary>
        internal List<EquipSet> LoadMySetCSV(IEnumerable<Equipment> equips)
        {
            List<EquipSet> mySets = new();

            string csv = ReadAllText(MySetCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                EquipSet set = new EquipSet();
                set.Weapon = GetEquipByName(line[@"武器"], equips) as Weapon ?? new();
                set.Head = GetEquipByName(line[@"頭"], equips);
                set.Body = GetEquipByName(line[@"胴"], equips);
                set.Arm = GetEquipByName(line[@"腕"], equips);
                set.Waist = GetEquipByName(line[@"腰"], equips);
                set.Leg = GetEquipByName(line[@"足"], equips);
                set.Charm = GetEquipByName(line[@"護石"], equips);
                set.Head.Kind = EquipKind.head;
                set.Body.Kind = EquipKind.body;
                set.Arm.Kind = EquipKind.arm;
                set.Waist.Kind = EquipKind.waist;
                set.Leg.Kind = EquipKind.leg;
                set.Charm.Kind = EquipKind.charm;
                set.Decos = DecosFromCsv(line[@"装飾品"], equips);
                set.SortDecos();
                set.Name = line[@"名前"];
                // 互換性のため、lineが"限界突破有無"を要素に持っていることを確認
                set.IsTranscending = line.HasColumn(@"限界突破有無") && (line[@"限界突破有無"] == "1");
                mySets.Add(set);
            }

            return mySets;
        }

        /// <summary>
        /// 装飾品CSVから装飾品リストを作成
        /// </summary>
        /// <param name="decoCsv">装飾品CSV</param>
        /// <param name="equips">装備一覧</param>
        /// <returns>装飾品リスト</returns>
        private List<Equipment> DecosFromCsv(string decoCsv, IEnumerable<Equipment> equips)
        {
            List<Equipment> decos = new();
            string[] splitted = decoCsv.Split(',');
            foreach (var decoName in splitted)
            {
                if (string.IsNullOrWhiteSpace(decoName))
                {
                    continue;
                }
                Equipment? deco = GetEquipByName(decoName, equips);
                if (deco != null)
                {
                    decos.Add(deco);
                }
            }
            return decos;
        }

        /// <summary>
        /// 装備名から武器を取得
        /// </summary>
        /// <param name="name">装備名</param>
        /// <param name="equips">装備一覧</param>
        /// <returns>該当装備</returns>
        private Equipment GetEquipByName(string name, IEnumerable<Equipment> equips)
        {
            Equipment? equip = equips.Where(equip => equip.Name == name.Trim()).FirstOrDefault();
            if (equip == null)
            {
                equip = new();
                equip.Name = name;
            }
            return equip;
        }



        /// <summary>
        /// 最近使ったスキル書き込み
        /// </summary>
        internal void SaveRecentSkillCSV(List<string> recentSkillNames)
        {
            List<string[]> body = new List<string[]>();
            foreach (var name in recentSkillNames)
            {
                body.Add(new string[] { name });
            }
            string[] header = new string[] { "スキル名" };
            string export = CsvWriter.WriteToText(header, body);
            try
            {
                File.WriteAllText(RecentSkillCsv, export);
            }
            catch (Exception e)
            {
                logger.Warn(e, "エラーが発生しました。");
            }
        }

        /// <summary>
        /// 最近使ったスキル読み込み
        /// </summary>
        internal List<string> LoadRecentSkillCSV()
        {
            List<string> recentSkillNames = new();

            string csv = ReadAllText(RecentSkillCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                recentSkillNames.Add(line[@"スキル名"]);
            }
            return recentSkillNames;
        }

        /// <summary>
        /// マイ検索条件書き込み
        /// </summary>
        internal void SaveMyConditionCSV(List<SearchCondition> myConditions)
        {
            List<string[]> body = new();
            foreach (var condition in myConditions)
            {
                List<string> bodyStrings = new();
                bodyStrings.Add(condition.ID);
                bodyStrings.Add(condition.DispName);
                bodyStrings.Add(condition.IsSpecificWeapon.ToString());
                bodyStrings.Add(condition.WeaponName ?? "null");
                bodyStrings.Add(condition.WeaponType.ToString());
                bodyStrings.Add(condition.MinAttack?.ToString() ?? "null");
                bodyStrings.Add(condition.Def?.ToString() ?? "null");
                bodyStrings.Add(condition.Fire?.ToString() ?? "null");
                bodyStrings.Add(condition.Water?.ToString() ?? "null");
                bodyStrings.Add(condition.Thunder?.ToString() ?? "null");
                bodyStrings.Add(condition.Ice?.ToString() ?? "null");
                bodyStrings.Add(condition.Dragon?.ToString() ?? "null");
                bodyStrings.Add(condition.SkillCSV);
                bodyStrings.Add(condition.IsTranscending ? "1" : string.Empty);
                body.Add(bodyStrings.ToArray());
            }

            string[] header = new string[] { "ID", "名前", "武器指定有無", "武器名", "武器種", "攻撃力", "防御力", "火耐性", "水耐性", "雷耐性", "氷耐性", "龍耐性", "スキル", "限界突破有無" };
            string export = CsvWriter.WriteToText(header, body);
            File.WriteAllText(ConditionCsv, export);
        }

        /// <summary>
        /// マイ検索条件読み込み
        /// </summary>
        internal List<SearchCondition> LoadMyConditionCSV()
        {
            List<SearchCondition> myConditions = new();

            string csv = ReadAllText(ConditionCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                SearchCondition condition = new();

                condition.ID = line[@"ID"];
                condition.DispName = line[@"名前"];
                condition.IsSpecificWeapon = Convert.ToBoolean(line[@"武器指定有無"]);
                condition.WeaponType = (WeaponType)Enum.Parse(typeof(WeaponType), line[@"武器種"]);
                if (condition.IsSpecificWeapon)
                {
                    condition.WeaponName = line[@"武器名"];
                }
                else
                {
                    condition.MinAttack = line[@"攻撃力"] == "null" ? null : ParseUtil.Parse(line[@"攻撃力"]);
                }
                condition.Def = line[@"防御力"] == "null" ? null : ParseUtil.Parse(line[@"防御力"]);
                condition.Fire = line[@"火耐性"] == "null" ? null : ParseUtil.Parse(line[@"火耐性"]);
                condition.Water = line[@"水耐性"] == "null" ? null : ParseUtil.Parse(line[@"水耐性"]);
                condition.Thunder = line[@"雷耐性"] == "null" ? null : ParseUtil.Parse(line[@"雷耐性"]);
                condition.Ice = line[@"氷耐性"] == "null" ? null : ParseUtil.Parse(line[@"氷耐性"]);
                condition.Dragon = line[@"龍耐性"] == "null" ? null : ParseUtil.Parse(line[@"龍耐性"]);
                condition.SkillCSV = line[@"スキル"];
                // 互換性のため、lineが"限界突破有無"を要素に持っていない場合、デフォルトで限界突破有りとする
                condition.IsTranscending = (!line.HasColumn(@"限界突破有無")) || (line[@"限界突破有無"] == "1");

                myConditions.Add(condition);
            }
            return myConditions;
        }

        /// <summary>
        /// 追加護石書き込み
        /// </summary>
        internal void SaveAdditionalCharmCSV(List<Equipment> additionalCharms, List<EquipSet> mySets)
        {
            List<string[]> body = new();
            foreach (var charm in additionalCharms)
            {
                List<string> bodyStrings = new List<string>();
                for (int i = 0; i < LogicConfig.Instance.MaxCharmSkillCount; i++)
                {
                    bodyStrings.Add(charm.Skills.Count > i ? charm.Skills[i].Name : string.Empty);
                    bodyStrings.Add(charm.Skills.Count > i ? charm.Skills[i].Level.ToString() : string.Empty);
                }
                // 泣きシミュフォーマット対応
                List<int> wSlot = new();
                List<int> aSlot = new();
                if (charm.SlotType1 == 1)
                {
                    wSlot.Add(charm.Slot1);
                }
                else if (charm.SlotType1 == 0)
                {
                    aSlot.Add(charm.Slot1);
                }
                if (charm.SlotType2 == 1)
                {
                    wSlot.Add(charm.Slot2);
                }
                else if (charm.SlotType2 == 0)
                {
                    aSlot.Add(charm.Slot2);
                }
                if (charm.SlotType3 == 1)
                {
                    wSlot.Add(charm.Slot3);
                }
                else if (charm.SlotType3 == 0)
                {
                    aSlot.Add(charm.Slot3);
                }
                while (wSlot.Count < 3)
                {
                    wSlot.Add(0);
                }
                while (aSlot.Count < 3)
                {
                    aSlot.Add(0);
                }
                foreach (int i in aSlot)
                {
                    bodyStrings.Add(i.ToString());
                }
                foreach (int i in wSlot)
                {
                    bodyStrings.Add(i.ToString());
                }

                bodyStrings.Add(charm.Slot1.ToString());
                bodyStrings.Add(charm.Slot2.ToString());
                bodyStrings.Add(charm.Slot3.ToString());
                bodyStrings.Add(charm.SlotType1.ToString());
                bodyStrings.Add(charm.SlotType2.ToString());
                bodyStrings.Add(charm.SlotType3.ToString());
                bodyStrings.Add(charm.Name);
                bodyStrings.Add(mySets.Where(set => charm.Name.Equals(set.Charm.Name)).Any() ? "マイセット登録中" : string.Empty);
                body.Add(bodyStrings.ToArray());
            }

            List<string> headStrings = new List<string>();
            for (int i = 1; i <= LogicConfig.Instance.MaxCharmSkillCount; i++)
            {
                headStrings.Add("スキル系統" + i);
                headStrings.Add("スキル値" + i);
            }
            headStrings.Add("(泣用防具スロ1)");
            headStrings.Add("(泣用防具スロ2)");
            headStrings.Add("(泣用防具スロ3)");
            headStrings.Add("(泣用武器スロ1)");
            headStrings.Add("(泣用武器スロ2)");
            headStrings.Add("(泣用武器スロ3)");
            headStrings.Add("スロット1");
            headStrings.Add("スロット2");
            headStrings.Add("スロット3");
            headStrings.Add("スロット1タイプ");
            headStrings.Add("スロット2タイプ");
            headStrings.Add("スロット3タイプ");
            headStrings.Add("内部管理ID");
            headStrings.Add("マイセット登録有無");
            string[] header = headStrings.ToArray();

            string export = CsvWriter.WriteToText(header, body);
            File.WriteAllText(AdditionalCharmCsv, export);
        }

        /// <summary>
        /// 追加護石読み込み
        /// </summary>
        internal List<Equipment> LoadAdditionalCharmCSV()
        {
            List<Equipment> additionalCharms = new();
            string csv = ReadAllText(AdditionalCharmCsv);
            var x = CsvReader.ReadFromText(csv);
            foreach (ICsvLine line in x)
            {
                Equipment charm = new Equipment(EquipKind.charm);
                try
                {
                    charm.Name = line[@"内部管理ID"];
                    if (string.IsNullOrWhiteSpace(charm.Name))
                    {
                        charm.Name = Guid.NewGuid().ToString();
                    }
                }
                catch (InvalidOperationException)
                {
                    charm.Name = Guid.NewGuid().ToString();
                }

                try
                {
                    charm.Slot1 = ParseUtil.Parse(line[@"スロット1"]);
                    charm.Slot2 = ParseUtil.Parse(line[@"スロット2"]);
                    charm.Slot3 = ParseUtil.Parse(line[@"スロット3"]);
                    charm.SlotType1 = ParseUtil.Parse(line[@"スロット1タイプ"]);
                    charm.SlotType2 = ParseUtil.Parse(line[@"スロット2タイプ"]);
                    charm.SlotType3 = ParseUtil.Parse(line[@"スロット3タイプ"]);
                }
                catch (InvalidOperationException)
                {
                    // 泣きシミュフォーマット対応
                    List<(int, int)> slotsData = new();
                    int w1 = ParseUtil.Parse(line[@"(泣用武器スロ1)"]);
                    if (w1 != 0)
                    {
                        slotsData.Add((w1, 1));
                    }
                    int w2 = ParseUtil.Parse(line[@"(泣用武器スロ2)"]);
                    if (w2 != 0)
                    {
                        slotsData.Add((w2, 1));
                    }
                    int w3 = ParseUtil.Parse(line[@"(泣用武器スロ3)"]);
                    if (w3 != 0)
                    {
                        slotsData.Add((w3, 1));
                    }
                    int a1 = ParseUtil.Parse(line[@"(泣用防具スロ1)"]);
                    if (a1 != 0)
                    {
                        slotsData.Add((a1, 0));
                    }
                    int a2 = ParseUtil.Parse(line[@"(泣用防具スロ2)"]);
                    if (a2 != 0)
                    {
                        slotsData.Add((a2, 0));
                    }
                    int a3 = ParseUtil.Parse(line[@"(泣用防具スロ3)"]);
                    if (a3 != 0)
                    {
                        slotsData.Add((a3, 0));
                    }
                    if (slotsData.Count >= 1)
                    {
                        (charm.Slot1, charm.SlotType1) = slotsData[0];
                    }
                    if (slotsData.Count >= 2)
                    {
                        (charm.Slot2, charm.SlotType2) = slotsData[1];
                    }
                    if (slotsData.Count >= 3)
                    {
                        (charm.Slot3, charm.SlotType3) = slotsData[2];
                    }
                }

                List<Skill> skills = new List<Skill>();
                for (int i = 1; i <= LogicConfig.Instance.MaxCharmSkillCount; i++)
                {
                    string skill = line[@"スキル系統" + i];
                    string level = line[@"スキル値" + i];
                    if (string.IsNullOrWhiteSpace(skill))
                    {
                        break;
                    }
                    skills.Add(new Skill(skill, ParseUtil.Parse(level)));
                }
                charm.Skills = skills;

                charm.SetCharmDispName();

                additionalCharms.Add(charm);
            }

            // GUIDの反映のためSaveが必要だが、マイセット読み込み後に実施するためここでは行わない
            // SaveAdditionalCharmCSV();

            return additionalCharms;
        }

        /// <summary>
        /// 護石検索用組み合わせ読み込み
        /// </summary>
        internal List<CharmCombo> LoadAdditionalCharmComboCSV()
        {
            List<CharmCombo> shiningCharmCombos = new();
            string csv = ReadAllText(ShiningCharmComboCsv);
            var x = CsvReader.ReadFromText(csv);
            foreach (ICsvLine line in x)
            {
                CharmCombo combo = new();
                combo.Rare = ParseUtil.Parse(line[@"レア度"]);
                combo.Group1 = ParseUtil.Parse(line[@"グループ1"]);
                combo.Group2 = ParseUtil.Parse(line[@"グループ2"]);
                combo.Group3 = ParseUtil.Parse(line[@"グループ3"]);
                combo.Slot1 = ParseUtil.Parse(line[@"スロット1"]);
                combo.Slot2 = ParseUtil.Parse(line[@"スロット2"]);
                combo.Slot3 = ParseUtil.Parse(line[@"スロット3"]);
                combo.SlotType1 = ParseUtil.Parse(line[@"スロット1タイプ"]);
                combo.SlotType2 = ParseUtil.Parse(line[@"スロット2タイプ"]);
                combo.SlotType3 = ParseUtil.Parse(line[@"スロット3タイプ"]);

                shiningCharmCombos.Add(combo);
            }

            return shiningCharmCombos;
        }

        /// <summary>
        /// 護石検索用グループ情報読み込み
        /// </summary>
        internal Dictionary<int, List<Skill>> LoadAdditionalCharmGroupCSV()
        {
            Dictionary<int, List<Skill>> shiningCharmGroups = new();
            shiningCharmGroups.Add(0, new());
            string csv = ReadAllText(ShiningCharmGroupCsv);
            var x = CsvReader.ReadFromText(csv);
            foreach (ICsvLine line in x)
            {
                int group = ParseUtil.Parse(line[@"グループ"]);
                if (!shiningCharmGroups.ContainsKey(group))
                {
                    shiningCharmGroups.Add(group, new());
                }
                List<Skill> groupSkills = shiningCharmGroups[group];
                Skill skill = new Skill(line[@"スキル名"], ParseUtil.Parse(line[@"レベル"]));
                groupSkills.Add(skill);
            }

            return shiningCharmGroups;
        }

        /// <summary>
        /// 防御力強化差分読み込み
        /// </summary>
        internal Dictionary<int, DefUpgrade> LoadDefUpgradeCSV()
        {
            Dictionary<int,DefUpgrade> defUpgrades = new();
            string csv = ReadAllText(DefUpgradeCsv);
            var x = CsvReader.ReadFromText(csv);
            foreach (ICsvLine line in x)
            {
                int rare = ParseUtil.Parse(line[@"レア度"]);
                int upgrade = ParseUtil.Parse(line[@"最大強化"]);
                int transcending = ParseUtil.Parse(line[@"限界突破強化"]);
                if (rare != 0)
                {
                    defUpgrades.Add(rare, new(upgrade, transcending));
                }
            }
            return defUpgrades;
        }

        /// <summary>
        /// アーティア書き込み
        /// </summary>
        internal void SaveArtianCSV(List<Weapon> artians, List<EquipSet> mySets)
        {
            List<string[]> body = new();
            foreach (var artian in artians)
            {
                List<string> bodyStrings = new List<string>();

                bodyStrings.Add(artian.WeaponType.ToString());
                bodyStrings.Add(artian.DispName.ToString());
                for (int i = 0; i < LogicConfig.Instance.ArtianSkillCount; i++)
                {
                    bodyStrings.Add(artian.Skills.Count > i ? artian.Skills[i].Name ?? string.Empty : string.Empty);
                    bodyStrings.Add(artian.Skills.Count > i ? artian.Skills[i].Level.ToString() : string.Empty);
                }
                bodyStrings.Add(artian.Name);
                bodyStrings.Add(mySets.Where(set => artian.Name.Equals(set.Charm.Name)).Any() ? "マイセット登録中" : string.Empty);

                body.Add(bodyStrings.ToArray());
            }

            List<string> headStrings = new List<string>();
            headStrings.Add("武器種");
            headStrings.Add("名前");
            for (int i = 1; i <= LogicConfig.Instance.ArtianSkillCount; i++)
            {
                headStrings.Add("スキル系統" + i);
                headStrings.Add("スキル値" + i);
            }
            headStrings.Add("内部管理ID");
            headStrings.Add("マイセット登録有無");
            string[] header = headStrings.ToArray();

            string export = CsvWriter.WriteToText(header, body);
            File.WriteAllText(ArtianCsv, export);
        }

        /// <summary>
        /// アーティア読み込み
        /// </summary>
        internal List<Weapon> LoadArtianCSV()
        {
            List<Weapon> artians = new();

            // csv読み込み
            string csv = ReadAllText(ArtianCsv);
            var x = CsvReader.ReadFromText(csv);
            foreach (ICsvLine line in x)
            {
                Weapon artian = new Weapon();
                artian.InitArtian();

                try
                {
                    artian.Name = line[@"内部管理ID"];
                    if (string.IsNullOrWhiteSpace(artian.Name))
                    {
                        artian.Name = Guid.NewGuid().ToString();
                    }
                }
                catch (InvalidOperationException)
                {
                    artian.Name = Guid.NewGuid().ToString();
                }
                artian.WeaponType = (WeaponType)Enum.Parse(typeof(WeaponType), line[@"武器種"]);
                artian.DispName = line[@"名前"];
                List<Skill> skills = new List<Skill>();
                for (int i = 1; i <= LogicConfig.Instance.ArtianSkillCount; i++)
                {
                    string skill = line[@"スキル系統" + i];
                    string level = line[@"スキル値" + i];
                    if (string.IsNullOrWhiteSpace(skill))
                    {
                        break;
                    }
                    skills.Add(new Skill(skill, ParseUtil.Parse(level)));
                }
                artian.Skills = skills;

                artians.Add(artian);

                // GUIDの反映のためSaveが必要だが、マイセット読み込み後に実施するためここでは行わない
                // SaveArtianCSV();
            }
            return artians;
        }

        /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <param name="fileName">CSVファイル名</param>
        /// <returns>CSVの内容</returns>
        private string ReadAllText(string fileName)
        {
            try
            {
                string csv = File.ReadAllText(fileName);

                // ライブラリの仕様に合わせてヘッダーを修正
                // ヘッダー行はコメントアウトしない
                if (csv.StartsWith('#'))
                {
                    csv = csv.Substring(1);
                }
                // 同名のヘッダーは利用不可なので小細工
                csv = csv.Replace("生産素材1,個数", "生産素材1,生産素材個数1");
                csv = csv.Replace("生産素材2,個数", "生産素材2,生産素材個数2");
                csv = csv.Replace("生産素材3,個数", "生産素材3,生産素材個数3");
                csv = csv.Replace("生産素材4,個数", "生産素材4,生産素材個数4");
                csv = csv.Replace("生産素材A1,個数", "生産素材1,生産素材個数1");
                csv = csv.Replace("生産素材A2,個数", "生産素材2,生産素材個数2");
                csv = csv.Replace("生産素材A3,個数", "生産素材3,生産素材個数3");
                csv = csv.Replace("生産素材A4,個数", "生産素材4,生産素材個数4");

                return csv;
            }
            catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// saveフォルダがなかったら作成する
        /// </summary>
        internal void MakeSaveFolder()
        {
            if (!System.IO.Directory.Exists(SaveFolder))
            {
                Directory.CreateDirectory(SaveFolder);
            }
        }


    }
}
