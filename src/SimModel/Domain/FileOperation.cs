using Csv;
using NLog;
using SimModel.Config;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
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
                    Category = line[SkillMasterHeaderCategory]
                })
                // マスタのCSVにある同名スキルのうち、スキルレベルが最大のものだけを選ぶ
                .GroupBy(x => new { x.Name, x.Category })
                .Select(group => new Skill(group.Key.Name, group.Max(x => x.Level), group.Key.Category))
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
                Equipment equip = new Equipment(kind);
                equip.Name = line[@"名前"];
                equip.Rare = ParseUtil.Parse(line[@"レア度"]);
                equip.Slot1 = ParseUtil.Parse(line[@"スロット1"]);
                equip.Slot2 = ParseUtil.Parse(line[@"スロット2"]);
                equip.Slot3 = ParseUtil.Parse(line[@"スロット3"]);
                if (kind == EquipKind.charm)
                {
                    equip.SlotType1 = ParseUtil.Parse(line[@"スロット1タイプ"]);
                    equip.SlotType2 = ParseUtil.Parse(line[@"スロット2タイプ"]);
                    equip.SlotType3 = ParseUtil.Parse(line[@"スロット3タイプ"]);
                }
                equip.Mindef = ParseUtil.Parse(line[@"初期防御力"]);
                equip.Maxdef = ParseUtil.Parse(line[@"最終防御力"], equip.Mindef); // 読み込みに失敗した場合は初期防御力と同値とみなす
                equip.Fire = ParseUtil.Parse(line[@"火耐性"]);
                equip.Water = ParseUtil.Parse(line[@"水耐性"]);
                equip.Thunder = ParseUtil.Parse(line[@"雷耐性"]);
                equip.Ice = ParseUtil.Parse(line[@"氷耐性"]);
                equip.Dragon = ParseUtil.Parse(line[@"龍耐性"]);
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

                equipments.Add(equip);
            }
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
                if (equip.Slot1 == 4)
                {
                    if (skills.Count < 2)
                    {
                        equip.DecoCateory = "4スロ単一スキル";
                    }
                    else
                    {
                        equip.DecoCateory = $"4スロ{skills[1].Name}複合";
                    }
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
                // TODO: ★後で検討
                //bodyStrings.Add(condition.WeaponSlot1.ToString());
                //bodyStrings.Add(condition.WeaponSlot2.ToString());
                //bodyStrings.Add(condition.WeaponSlot3.ToString());
                bodyStrings.Add(condition.Def?.ToString() ?? "null");
                bodyStrings.Add(condition.Fire?.ToString() ?? "null");
                bodyStrings.Add(condition.Water?.ToString() ?? "null");
                bodyStrings.Add(condition.Thunder?.ToString() ?? "null");
                bodyStrings.Add(condition.Ice?.ToString() ?? "null");
                bodyStrings.Add(condition.Dragon?.ToString() ?? "null");
                bodyStrings.Add(condition.SkillCSV);
                body.Add(bodyStrings.ToArray());
            }

            string[] header = new string[] { "ID", "名前", "武器スロ1", "武器スロ2", "武器スロ3", "防御力", "火耐性", "水耐性", "雷耐性", "氷耐性", "龍耐性", "スキル"};
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
                // TODO: ★後で検討
                //condition.WeaponSlot1 = ParseUtil.Parse(line[@"武器スロ1"]);
                //condition.WeaponSlot2 = ParseUtil.Parse(line[@"武器スロ2"]);
                //condition.WeaponSlot3 = ParseUtil.Parse(line[@"武器スロ3"]);
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
