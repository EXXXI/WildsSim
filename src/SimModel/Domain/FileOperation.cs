using Csv;
using Google.OrTools.ConstraintSolver;
using Google.Protobuf.Collections;
using NLog;
using SimModel.Config;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using static Google.Protobuf.WellKnownTypes.Field.Types;
using static System.Reflection.Metadata.BlobBuilder;

namespace SimModel.Domain
{
    /// <summary>
    /// CSV・Json操作クラス
    /// </summary>
    static internal class FileOperation
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

        static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// スキルマスタ読み込み
        /// </summary>
        static internal void LoadSkillCSV()
        {
            string csv = ReadAllText(SkillCsv);

            Masters.Skills = CsvReader.ReadFromText(csv)
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
                Skill skill = Masters.Skills.First(s => s.Name == item.Name);
                skill.SpecificNames.Add(item.Level, item.Specific);
            }

            // どの防具・護石・武器にも存在しないスキルの場合除外
            // TODO: AdditionalCharms実装後、要見直し
            var equips = Masters.Weapons.Union(Masters.Heads).Union(Masters.Bodys).Union(Masters.Arms)
                .Union(Masters.Waists).Union(Masters.Legs).Union(Masters.Charms).Union(Masters.Decos);
            Masters.Skills = Masters.Skills.Where(skill =>
                equips.Any(e => e.Skills.Any(s => s.Name == skill.Name)))
                .ToList();
        }

        /// <summary>
        /// 武器マスタ読み込み
        /// </summary>
        static internal void LoadWeaponCSV()
        {
            Masters.Weapons = new();

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
                        Masters.Weapons.Add(weapon);
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

                Masters.Weapons.Add(weapon);
            }
        }

        /// <summary>
        /// 頭防具マスタ読み込み
        /// </summary>
        static internal void LoadHeadCSV()
        {
            Masters.Heads = new();
            LoadEquipCSV(HeadCsv, Masters.Heads, EquipKind.head);
        }

        /// <summary>
        /// 胴防具マスタ読み込み
        /// </summary>
        static internal void LoadBodyCSV()
        {
            Masters.Bodys = new();
            LoadEquipCSV(BodyCsv, Masters.Bodys, EquipKind.body);
        }

        /// <summary>
        /// 腕防具マスタ読み込み
        /// </summary>
        static internal void LoadArmCSV()
        {
            Masters.Arms = new();
            LoadEquipCSV(ArmCsv, Masters.Arms, EquipKind.arm);
        }

        /// <summary>
        /// 腰防具マスタ読み込み
        /// </summary>
        static internal void LoadWaistCSV()
        {
            Masters.Waists = new();
            LoadEquipCSV(WaistCsv, Masters.Waists, EquipKind.waist);
        }

        /// <summary>
        /// 足防具マスタ読み込み
        /// </summary>
        static internal void LoadLegCSV()
        {
            Masters.Legs = new();
            LoadEquipCSV(LegCsv, Masters.Legs, EquipKind.leg);
        }

        /// <summary>
        /// 護石マスタ読み込み
        /// </summary>
        static internal void LoadCharmCSV()
        {
            Masters.Charms = new();
            LoadEquipCSV(CharmCsv, Masters.Charms, EquipKind.charm);
        }

        /// <summary>
        /// 防具マスタ読み込み
        /// </summary>
        /// <param name="fileName">CSVファイル名</param>
        /// <param name="equipments">格納先</param>
        /// <param name="kind">部位</param>
        static private void LoadEquipCSV(string fileName, List<Equipment> equipments, EquipKind kind)
        {
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
                    int maxdef = CalcMaxdef(equip.Rare, equip.Mindef, equip.Kind);
                    equip.Maxdef = ParseUtil.Parse(line[@"最終防御力"], maxdef); // 指定がある場合指定を優先
                    int transcendingDef = CalcTranscendingDef(equip.Rare, equip.Maxdef, equip.Kind);
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

                equipments.Add(equip);
            }
        }

        /// <summary>
        /// 最大防御力をレア度から算出する
        /// </summary>
        /// <param name="rare">レア度</param>
        /// <param name="mindef">最低防御力</param>
        /// <param name="kind">防具種類</param>
        /// <returns>レア度から算出した最大防御力</returns>
        private static int CalcMaxdef(int rare, int mindef, EquipKind kind)
        {
            if (kind == EquipKind.charm)
            {
                return mindef;
            }
            bool getted = Masters.DefUpgrades.TryGetValue(rare, out DefUpgrade? defUpgrade);
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
        private static int CalcTranscendingDef(int rare, int maxdef, EquipKind kind)
        {
            if (kind == EquipKind.charm)
            {
                return maxdef;
            }
            bool getted = Masters.DefUpgrades.TryGetValue(rare, out DefUpgrade? defUpgrade);
            if (getted && defUpgrade != null)
            {
                return maxdef + defUpgrade.TranscendingDef;
            }
            return maxdef;
        }

        /// <summary>
        /// 装飾品マスタ読み込み
        /// </summary>
        static internal void LoadDecoCSV()
        {
            Masters.Decos = new();

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

                Masters.Decos.Add(equip);
            }
            
            LoadDecoCountJson();
        }

        /// <summary>
        /// 装飾品所持数読み込み
        /// </summary>
        private static void LoadDecoCountJson()
        {
            string json = ReadAllText(DecoCountJson);
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            JsonSerializerOptions options = new();
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All);
            Dictionary<string, int>? decoCounts = JsonSerializer.Deserialize<Dictionary<string, int>>(json, options);

            foreach (var deco in Masters.Decos)
            {
                deco.DecoCount = decoCounts?.Where(dc => deco.Name == dc.Key).Select(dc => dc.Value).FirstOrDefault() ?? 0;
            }
        }

        /// <summary>
        /// 装飾品所持数書き込み
        /// </summary>
        public static void SaveDecoCountJson()
        {
            Dictionary<string, int> data = new();
            foreach (var deco in Masters.Decos)
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
        static internal void SaveCludeCSV()
        {
            List<string[]> body = new List<string[]>();
            foreach (var clude in Masters.Cludes)
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
        static internal void LoadCludeCSV()
        {
            Masters.Cludes = new();

            string csv = ReadAllText(CludeCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                Clude clude = new Clude
                {
                    Name = line[@"対象"],
                    Kind = (CludeKind)ParseUtil.Parse(line[@"種別"])
                };

                Masters.Cludes.Add(clude);
            }
        }

        /// <summary>
        /// マイセットマスタ書き込み
        /// </summary>
        static internal void SaveMySetCSV()
        {
            List<string[]> body = new List<string[]>();
            foreach (var set in Masters.MySets)
            {
                body.Add(new string[] { set.Weapon.Name, set.Head.Name, set.Body.Name, set.Arm.Name, set.Waist.Name, set.Leg.Name, set.Charm.Name, set.DecoNameCSV, set.Name });
            }
            string[] header = new string[] { "武器", "頭", "胴", "腕", "腰", "足", "護石", "装飾品", "名前" };
            string export = CsvWriter.WriteToText(header, body);
            File.WriteAllText(MySetCsv, export);

            // マイセット利用状況の反映のため護石、アーティアを再書き込み
            SaveAdditionalCharmCSV();
            SaveArtianCSV();
        }

        /// <summary>
        /// マイセットマスタ読み込み
        /// </summary>
        static internal void LoadMySetCSV()
        {
            Masters.MySets = new();

            string csv = ReadAllText(MySetCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                EquipSet set = new EquipSet();
                set.Weapon = Masters.GetEquipByName(line[@"武器"]) as Weapon ?? new();
                set.Head = Masters.GetEquipByName(line[@"頭"]);
                set.Body = Masters.GetEquipByName(line[@"胴"]);
                set.Arm = Masters.GetEquipByName(line[@"腕"]);
                set.Waist = Masters.GetEquipByName(line[@"腰"]);
                set.Leg = Masters.GetEquipByName(line[@"足"]);
                set.Charm = Masters.GetEquipByName(line[@"護石"]);
                set.Head.Kind = EquipKind.head;
                set.Body.Kind = EquipKind.body;
                set.Arm.Kind = EquipKind.arm;
                set.Waist.Kind = EquipKind.waist;
                set.Leg.Kind = EquipKind.leg;
                set.Charm.Kind = EquipKind.charm;
                set.DecoNameCSV = line[@"装飾品"];
                set.Name = line[@"名前"];
                Masters.MySets.Add(set);
            }

            // マイセット利用状況の反映のため護石を再書き込み
            SaveAdditionalCharmCSV();
            SaveArtianCSV();
        }

        /// <summary>
        /// 最近使ったスキル書き込み
        /// </summary>
        internal static void SaveRecentSkillCSV()
        {
            List<string[]> body = new List<string[]>();
            foreach (var name in Masters.RecentSkillNames)
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
        internal static void LoadRecentSkillCSV()
        {
            Masters.RecentSkillNames = new();

            string csv = ReadAllText(RecentSkillCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                Masters.RecentSkillNames.Add(line[@"スキル名"]);
            }
        }

        /// <summary>
        /// マイ検索条件書き込み
        /// </summary>
        internal static void SaveMyConditionCSV()
        {
            List<string[]> body = new();
            foreach (var condition in Masters.MyConditions)
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
                body.Add(bodyStrings.ToArray());
            }

            string[] header = new string[] { "ID", "名前", "武器指定有無", "武器名", "武器種", "攻撃力", "防御力", "火耐性", "水耐性", "雷耐性", "氷耐性", "龍耐性", "スキル"};
            string export = CsvWriter.WriteToText(header, body);
            File.WriteAllText(ConditionCsv, export);
        }

        /// <summary>
        /// マイ検索条件読み込み
        /// </summary>
        internal static void LoadMyConditionCSV()
        {
            Masters.MyConditions = new();

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

                Masters.MyConditions.Add(condition);
            }
        }

        /// <summary>
        /// 追加護石書き込み
        /// </summary>
        internal static void SaveAdditionalCharmCSV()
        {
            List<string[]> body = new();
            foreach (var charm in Masters.AdditionalCharms)
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
                bodyStrings.Add(Masters.MySets.Where(set => charm.Name.Equals(set.Charm.Name)).Any() ? "マイセット登録中" : string.Empty);
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
        internal static void LoadAdditionalCharmCSV()
        {
            Masters.AdditionalCharms = new();
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

                Masters.AdditionalCharms.Add(charm);
            }

            // GUIDの反映のためSaveが必要だが、マイセット読み込み後に実施するためここでは行わない
            // SaveAdditionalCharmCSV();
        }

        /// <summary>
        /// 護石検索用組み合わせ読み込み
        /// </summary>
        internal static void LoadAdditionalCharmComboCSV()
        {
            Masters.ShiningCharmCombos = new();
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

                Masters.ShiningCharmCombos.Add(combo);
            }
        }

        /// <summary>
        /// 護石検索用グループ情報読み込み
        /// </summary>
        internal static void LoadAdditionalCharmGroupCSV()
        {
            Masters.ShiningCharmGroups = new();
            Masters.ShiningCharmGroups.Add(0, new());
            string csv = ReadAllText(ShiningCharmGroupCsv);
            var x = CsvReader.ReadFromText(csv);
            foreach (ICsvLine line in x)
            {
                int group = ParseUtil.Parse(line[@"グループ"]);
                if (!Masters.ShiningCharmGroups.ContainsKey(group))
                {
                    Masters.ShiningCharmGroups.Add(group, new());
                }
                List<Skill> groupSkills = Masters.ShiningCharmGroups[group];
                Skill skill = new Skill(line[@"スキル名"], ParseUtil.Parse(line[@"レベル"]));
                groupSkills.Add(skill);
            }
        }

        /// <summary>
        /// 防御力強化差分読み込み
        /// </summary>
        internal static void LoadDefUpgradeCSV()
        {
            Masters.DefUpgrades = new();
            string csv = ReadAllText(DefUpgradeCsv);
            var x = CsvReader.ReadFromText(csv);
            foreach (ICsvLine line in x)
            {
                int rare = ParseUtil.Parse(line[@"レア度"]);
                int upgrade = ParseUtil.Parse(line[@"最大強化"]);
                int transcending = ParseUtil.Parse(line[@"限界突破強化"]);
                if (rare != 0)
                {
                    Masters.DefUpgrades.Add(rare, new(upgrade, transcending));
                }
            }
        }

        /// <summary>
        /// アーティア書き込み
        /// </summary>
        internal static void SaveArtianCSV()
        {
            List<string[]> body = new();
            foreach (var artian in Masters.Artians)
            {
                List<string> bodyStrings = new List<string>();

                bodyStrings.Add(artian.WeaponType.ToString());
                bodyStrings.Add(artian.DispName.ToString());
                for (int i = 0; i < LogicConfig.Instance.ArtianSkillCount; i++)
                {
                    bodyStrings.Add(artian.Skills.Count > i ? artian.Skills[i].Name : string.Empty);
                    bodyStrings.Add(artian.Skills.Count > i ? artian.Skills[i].Level.ToString() : string.Empty);
                }
                bodyStrings.Add(artian.Name);
                bodyStrings.Add(Masters.MySets.Where(set => artian.Name.Equals(set.Charm.Name)).Any() ? "マイセット登録中" : string.Empty);

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
        static internal void LoadArtianCSV()
        {
            Masters.Artians = new();

            // csv読み込み
            string csv = ReadAllText(ArtianCsv);
            var x = CsvReader.ReadFromText(csv);
            foreach (ICsvLine line in x)
            {
                Weapon artian = new Weapon();

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
                artian.Rare = 8;
                artian.Slot1 = 3;
                artian.Slot2 = 3;
                artian.Slot3 = 3;
                artian.SlotType1 = 1;
                artian.SlotType2 = 1;
                artian.SlotType3 = 1;
                artian.Mindef = 0;
                artian.Maxdef = artian.Mindef; // 防御力の変動はない
                artian.Attack = 190;
                artian.RowNo = int.MaxValue;
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

                Masters.Artians.Add(artian);

                // GUIDの反映のためSaveが必要だが、マイセット読み込み後に実施するためここでは行わない
                // SaveArtianCSV();
            }
        }

        /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <param name="fileName">CSVファイル名</param>
        /// <returns>CSVの内容</returns>
        static private string ReadAllText(string fileName)
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
        internal static void MakeSaveFolder()
        {
            if (!System.IO.Directory.Exists(SaveFolder))
            {
                Directory.CreateDirectory(SaveFolder);
            }
        }
    }
}
