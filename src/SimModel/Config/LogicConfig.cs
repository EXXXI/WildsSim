using Csv;
using SimModel.Domain;
using SimModel.ExceptionClass;
using System.IO.Abstractions;

namespace SimModel.Config
{
    /// <summary>
    /// SimModel側の設定
    /// </summary>
    public class LogicConfig : ConfigBase
    {
        /// <summary>
        /// ロジック設定ファイル
        /// </summary>
        private const string ConfCsv = "conf/logicConfig.csv";

        /// <summary>
        /// スロットの最大の大きさ
        /// </summary>
        public int MaxSlotSize { get; set; } = 4;

        /// <summary>
        /// 最近使ったスキルの記憶容量
        /// </summary>
        public int MaxRecentSkillCount { get; set; } = 20;

        /// <summary>
        /// 追加護石のスキル最大個数
        /// 泣シミュのフォーマットと合わせておく必要がある
        /// 基本的に変更を想定していない
        /// </summary>
        public int MaxCharmSkillCount { get; set; } = 3;

        /// <summary>
        /// 最大並列処理数
        /// 特殊な理由がない限り-1(自動でコア数に合わせる)であるべき
        /// 基本的に変更を想定していない
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; } = -1;

        /// <summary>
        /// 入手不可装備の利用有無
        /// </summary>
        public bool AllowUnavailableEquipments { get; set; } = false;

        /// <summary>
        /// 下位互換護石の検出有無
        /// </summary>
        public bool UseCalcUpperCharm { get; set; } = true;

        /// <summary>
        /// アーティア武器のスキル数
        /// これが変化する場合は、この数字だけで対応できない可能性が高い
        /// 基本的に変更を想定していない
        /// </summary>
        public int ArtianSkillCount { get; set; } = 2;

        /// <summary>
        /// マイセットのデフォルト名
        /// 基本的に変更を想定していない
        /// </summary>
        public string DefaultMySetName { get; set; } = "マイセット";

        /// <summary>
        /// ファイルシステムのインスタンス
        /// DIで注入される
        /// </summary>
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fileSystem"></param>
        public LogicConfig(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;

            try
            {
                string csv = _fileSystem.File.ReadAllText(ConfCsv);

                foreach (ICsvLine line in CsvReader.ReadFromText(csv))
                {
                    MaxSlotSize = LoadConfigItem(line, @"スロットの最大の大きさ", MaxSlotSize);
                    MaxRecentSkillCount = LoadConfigItem(line, @"最近使ったスキルの記憶容量", MaxRecentSkillCount);
                    MaxCharmSkillCount = LoadConfigItem(line, @"追加護石のスキル最大個数", MaxCharmSkillCount);
                    MaxDegreeOfParallelism = LoadConfigItem(line, @"最大並列処理数", MaxDegreeOfParallelism);
                    AllowUnavailableEquipments = LoadConfigItem(line, @"入手不可装備の利用有無", AllowUnavailableEquipments);
                    UseCalcUpperCharm = LoadConfigItem(line, @"下位互換護石の検出有無", UseCalcUpperCharm);
                    ArtianSkillCount = LoadConfigItem(line, @"アーティア武器のスキル数", ArtianSkillCount);
                    DefaultMySetName = LoadConfigItem(line, @"マイセットのデフォルト名", DefaultMySetName);
                }
            }
            catch (System.IO.IOException ex)
            {
                string message = $"設定ファイル {ConfCsv} の読み込みに失敗しました。";
                throw new SimulatorException(message, ex);
            }
            
        }

    }
}
