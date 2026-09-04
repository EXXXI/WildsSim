using Csv;
using SimModel.Domain;
using System;

namespace SimModel.Config
{
    /// <summary>
    /// 設定ファイルの読み込み機能を持つAbstractクラス
    /// 実際の設定ファイル読み込みクラスが継承する
    /// </summary>
    public abstract class ConfigBase
    {
        /// <summary>
        /// Configの項目を読み込み
        /// </summary>
        /// <param name="line">CsvLine</param>
        /// <param name="columnName">項目名</param>
        /// <param name="def">読み込み失敗時の値(int)</param>
        /// <returns>CSVから読み込んだ値、ただし、読み込み失敗時はdefの値を利用</returns>
        static protected int LoadConfigItem(ICsvLine line, string columnName, int def)
        {
            try
            {
                return ParseUtil.Parse(line[columnName], def);
            }
            catch (Exception)
            {
                return def;
            }
        }

        /// <summary>
        /// Configの項目を読み込み
        /// </summary>
        /// <param name="line">CsvLine</param>
        /// <param name="columnName">項目名</param>
        /// <param name="def">読み込み失敗時の値(string)</param>
        /// <returns>CSVから読み込んだ値、ただし、読み込み失敗時はdefの値を利用</returns>
        static protected string LoadConfigItem(ICsvLine line, string columnName, string def)
        {
            try
            {
                return line[columnName];
            }
            catch (Exception)
            {
                return def;
            }
        }

        /// <summary>
        /// Configの項目を読み込み
        /// </summary>
        /// <param name="line">CsvLine</param>
        /// <param name="columnName">項目名</param>
        /// <param name="def">読み込み失敗時の値(bool)</param>
        /// <returns>CSVから読み込んだ値、ただし、読み込み失敗時はdefの値を利用</returns>
        static protected bool LoadConfigItem(ICsvLine line, string columnName, bool def)
        {
            try
            {
                return bool.Parse(line[columnName]);
            }
            catch (Exception)
            {
                return def;
            }
        }
    }
}
