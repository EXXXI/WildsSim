using Csv;
using SimModel.Domain;
using System.IO;

namespace SimModel.Config
{
    /// <summary>
    /// SimModel側の設定
    /// </summary>
    public class LogicConfig
    {
        /// <summary>
        /// インスタンス
        /// </summary>
        static private LogicConfig instance;

        /// <summary>
        /// ロジック設定ファイル
        /// </summary>
        private const string ConfCsv = "conf/logicConfig.csv";

        /// <summary>
        /// スロットの最大の大きさ
        /// </summary>
        public int MaxSlotSize { get; set; }

        /// <summary>
        /// 最近使ったスキルの記憶容量
        /// </summary>
        public int MaxRecentSkillCount { get; set; }

        /// <summary>
        /// 防具のスキル最大個数
        /// </summary>
        public int MaxEquipSkillCount { get; set; }

        /// <summary>
        /// 装飾品のスキル最大個数
        /// </summary>
        public int MaxDecoSkillCount { get; set; }

        /// <summary>
        /// 追加護石のスキル最大個数
        /// </summary>
        public int MaxCharmSkillCount { get; set; }

        /// <summary>
        /// 最大並列処理数
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; }

        /// <summary>
        /// 入手不可装備の利用有無
        /// </summary>
        public bool AllowUnavailableEquipments { get; set; }

        /// <summary>
        /// 下位互換護石の検出有無
        /// </summary>
        public bool UseCalcUpperCharm { get; set; }

        /// <summary>
        /// アーティアのスキル数
        /// (現状2固定としてファイルへの外出しはしない)
        /// </summary>
        public int ArtianSkillCount { get; set; } = 2;

        /// <summary>
        /// マイセットのデフォルト名
        /// </summary>
        public string DefaultMySetName { get; } = "マイセット";

        /// <summary>
        /// プライベートコンストラクタ
        /// </summary>
        private LogicConfig()
        {
            string csv = File.ReadAllText(ConfCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                MaxSlotSize = ParseUtil.LoadConfigItem(line, @"スロットの最大の大きさ", 4);
                MaxRecentSkillCount = ParseUtil.LoadConfigItem(line, @"最近使ったスキルの記憶容量", 20);
                MaxEquipSkillCount = ParseUtil.LoadConfigItem(line, @"防具のスキル最大個数", 5);
                MaxDecoSkillCount = ParseUtil.LoadConfigItem(line, @"装飾品のスキル最大個数", 2);
                MaxCharmSkillCount = ParseUtil.LoadConfigItem(line, @"追加護石のスキル最大個数", 3);
                MaxDegreeOfParallelism = ParseUtil.LoadConfigItem(line, @"最大並列処理数", 4);
                AllowUnavailableEquipments = ParseUtil.LoadConfigItem(line, @"入手不可装備の利用有無", false);
                UseCalcUpperCharm = ParseUtil.LoadConfigItem(line, @"下位互換護石の検出有無", true); 
            }
        }

        /// <summary>
        /// インスタンス
        /// </summary>
        static public LogicConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LogicConfig();
                }
                return instance;
            }
        }
    }
}
