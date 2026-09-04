using SimModel.Config;
using SimModel.ExceptionClass;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace SimModelTest.Config
{

    /// <summary>
    /// LogicConfigのテスト
    /// </summary>
    public class LogicConfigTest
    {
        /// <summary>
        /// 通常の設定ファイルを読み込む
        /// </summary>
        [Fact]
        public void LogicConfigTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();

            // テスト
            LogicConfig config = new(fs);
            Assert.Equal(3, config.MaxSlotSize);
            Assert.Equal(20, config.MaxRecentSkillCount);
            Assert.Equal(3, config.MaxCharmSkillCount);
            Assert.Equal(-1, config.MaxDegreeOfParallelism);
            Assert.False(config.AllowUnavailableEquipments);
            Assert.True(config.UseCalcUpperCharm);
            Assert.Equal(2, config.ArtianSkillCount);
            Assert.Equal("マイセット", config.DefaultMySetName);
        }

        /// <summary>
        /// 別設定の設定ファイルを読み込み、反映を確認する
        /// </summary>
        [Fact]
        public void LogicConfigTest_another()
        {
            // MockFileSystemに配置
            string mockFilePath = "conf/logicConfig.csv";
            IFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "スロットの最大の大きさ,最近使ったスキルの記憶容量,追加護石のスキル最大個数,最大並列処理数,入手不可装備の利用有無,下位互換護石の検出有無,アーティア武器のスキル数,マイセットのデフォルト名\r\n" +
                    "4,21,4,5,true,false,3,Myset") }
                });

            // テスト
            LogicConfig config = new(fs);
            Assert.Equal(4, config.MaxSlotSize);
            Assert.Equal(21, config.MaxRecentSkillCount);
            Assert.Equal(4, config.MaxCharmSkillCount);
            Assert.Equal(5, config.MaxDegreeOfParallelism);
            Assert.True(config.AllowUnavailableEquipments);
            Assert.False(config.UseCalcUpperCharm);
            Assert.Equal(3, config.ArtianSkillCount);
            Assert.Equal("Myset", config.DefaultMySetName);
        }

        /// <summary>
        /// 空の設定ファイルを読み込み、デフォルト設定が行われることを確認する
        /// エラーにしないのは、後から項目を追加した際にも、古い設定ファイルを読み込めるようにするため
        /// </summary>
        [Fact]
        public void LogicConfigTest_empty()
        {
            // MockFileSystemに配置
            string mockFilePath = "conf/logicConfig.csv";
            IFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData(string.Empty) }
                });

            // テスト
            LogicConfig config = new(fs);
            Assert.Equal(4, config.MaxSlotSize);
            Assert.Equal(20, config.MaxRecentSkillCount);
            Assert.Equal(3, config.MaxCharmSkillCount);
            Assert.Equal(-1, config.MaxDegreeOfParallelism);
            Assert.False(config.AllowUnavailableEquipments);
            Assert.True(config.UseCalcUpperCharm);
            Assert.Equal(2, config.ArtianSkillCount);
            Assert.Equal("マイセット", config.DefaultMySetName);
        }

        /// <summary>
        /// 空行の設定ファイルを読み込み、デフォルト設定が行われることを確認する
        /// エラーにしないのは、後から項目を追加した際にも、古い設定ファイルを読み込めるようにするため
        /// </summary>
        [Fact]
        public void LogicConfigTest_noData()
        {
            // MockFileSystemに配置
            string mockFilePath = "conf/logicConfig.csv";
            IFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("data\ndummy") }
                });

            // テスト
            LogicConfig config = new(fs);
            Assert.Equal(4, config.MaxSlotSize);
            Assert.Equal(20, config.MaxRecentSkillCount);
            Assert.Equal(3, config.MaxCharmSkillCount);
            Assert.Equal(-1, config.MaxDegreeOfParallelism);
            Assert.False(config.AllowUnavailableEquipments);
            Assert.True(config.UseCalcUpperCharm);
            Assert.Equal(2, config.ArtianSkillCount);
            Assert.Equal("マイセット", config.DefaultMySetName);
        }

        /// <summary>
        /// confフォルダが存在しない場合、例外が出ることを確認する
        /// </summary>
        [Fact]
        public void LogicConfigTest_noDirectory()
        {
            // 空のMockFileSystem
            string mockFilePath = "conf/logicConfig.csv";
            IFileSystem fs = new MockFileSystem();

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => new LogicConfig(fs));
            Assert.Equal($"設定ファイル {mockFilePath} の読み込みに失敗しました。", ex.Message);
        }

        /// <summary>
        /// 設定ファイルが存在しない場合、例外が出ることを確認する
        /// </summary>
        [Fact]
        public void LogicConfigTest_noFile()
        {
            // 空のMockFileSystem
            string mockFilePath = "conf/logicConfig.csv";
            IFileSystem fs = new MockFileSystem();
            fs.Directory.CreateDirectory("conf");

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => new LogicConfig(fs));
            Assert.Equal($"設定ファイル {mockFilePath} の読み込みに失敗しました。", ex.Message);
        }
    }
}
