using Csv;
using SimModel.Config;
using SimModel.Domain;
using SimModel.ExceptionClass;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildsSim.Config
{
    /// <summary>
    /// View関連の設定
    /// </summary>
    internal class ViewConfig : ConfigBase
    {
        /// <summary>
        /// インスタンス
        /// </summary>
        static private ViewConfig instance;

        /// <summary>
        /// 画面設定ファイル
        /// </summary>
        private const string ConfCsv = "conf/viewConfig.csv";

        /// <summary>
        /// デフォルトの頑張り度
        /// </summary>
        public string DefaultLimit { get; set; } = "100";

        /// <summary>
        /// スキル未選択時の表示
        /// </summary>
        public string NoSkillName { get; set; } = "スキル選択";

        /// <summary>
        /// グリッドの列順保存有無
        /// </summary>
        public bool UseSavedColumnIndexes { get; set; }


        /// <summary>
        /// プライベートコンストラクタ
        /// </summary>
        private ViewConfig()
        {
            try
            {
                string csv = File.ReadAllText(ConfCsv);

                foreach (ICsvLine line in CsvReader.ReadFromText(csv))
                {
                    DefaultLimit = LoadConfigItem(line, @"デフォルトの頑張り度", DefaultLimit).ToString();
                    NoSkillName = LoadConfigItem(line, @"スキル未選択時の表示", NoSkillName);
                    UseSavedColumnIndexes = LoadConfigItem(line, @"グリッドの列順保存有無", @"有").Equals(@"有");
                }
            }
            catch (System.IO.IOException ex)
            {
                string message = $"設定ファイル {ConfCsv} の読み込みに失敗しました。";
                throw new SimulatorException(message, ex);
            }
        }

        /// <summary>
        /// インスタンス
        /// </summary>
        static public ViewConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ViewConfig();
                }
                return instance;
            }
        }
    }
}
