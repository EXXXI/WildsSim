using System.IO.Abstractions;

namespace SimModelTest
{
    /// <summary>
    /// テスト時にファイルを書き換えないようにするためのIFileSystemラッパークラス
    /// </summary>
    class ReadOnlyFile : FileWrapper
    {
        /// <summary>
        /// WriteAllTextの呼び出しをログとして残すためのDictionary
        /// </summary>
        public Dictionary<string, List<string>> WriteLog = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fileSystem"></param>
        public ReadOnlyFile(IFileSystem fileSystem)
            : base(fileSystem)
        {
        }

        /// <summary>
        /// WriteAllTextをオーバーライドして、実際にはファイルを書き込まずにログに書き込む
        /// </summary>
        /// <param name="path">書き込み先</param>
        /// <param name="contents">書き込み内容</param>
        public override void WriteAllText(string path, string contents)
        {
            // 代わりにログに書き込む
            if (!WriteLog.ContainsKey(path))
            {
                WriteLog[path] = new();
            }
            WriteLog[path].Add(contents);
        }

        /// <summary>
        /// 全てのWriteLogをクリアする
        /// </summary>
        public void ClearAllWriteLog()
        {
            WriteLog.Clear();
        }

        /// <summary>
        /// 特定のパスのWriteLogをクリアする
        /// </summary>
        /// <param name="path"></param>
        public void ClearWriteLog(string path)
        {
            if (WriteLog.ContainsKey(path))
            {
                WriteLog[path].Clear();
            }
        }
    }
}
