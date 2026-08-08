using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimModel.Config;
using SimModel.Domain;
using SimModel.Model;
using SimModel.Service;
using System.IO.Abstractions;

namespace SimModelTest
{
    /// <summary>
    /// テスト用Fixture
    /// DIコンテナへの登録と、Mastersの読み込みを行う
    /// ファイルの書き込みはモック化し、書き込みのかわりにログが残る
    /// </summary>
    public class MasterSetUpFixture
    {

        /// <summary>
        /// DIコンテナのプロバイダ
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// 内部で使用するReadOnlyFileのインスタンス
        /// </summary>
        private ReadOnlyFile ROFile { get; }

        /// <summary>
        /// WriteAllTextの呼び出しをログとして残すためのDictionary
        /// </summary>
        public Dictionary<string, List<string>> WriteLog
        {
            get
            {
                return ROFile.WriteLog;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MasterSetUpFixture()
        {
            var services = new ServiceCollection();
            services.AddSingleton<Simulator, Simulator>();
            services.AddSingleton<SearcherFactory, SearcherFactory>();
            services.AddSingleton<DataManagement, DataManagement>();
            services.AddSingleton<FileOperation, FileOperation>();
            services.AddSingleton<CharmAppraiser, CharmAppraiser>();
            services.AddSingleton<LogicConfig, LogicConfig>();
            services.AddSingleton<Masters, Masters>();

            var fileSystem = new FileSystem();
            ROFile = new ReadOnlyFile(fileSystem);
            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.SetupGet(x => x.File).Returns(ROFile);
            fileSystemMock.SetupGet(x => x.Directory).Returns(fileSystem.Directory); // MakeSaveForder用
            services.AddSingleton<IFileSystem>(fileSystemMock.Object);

            ServiceProvider = services.BuildServiceProvider();
            ServiceProvider.GetRequiredService<DataManagement>().LoadData();
        }

        /// <summary>
        /// ReadOnlyFileのWriteLogを全てクリアする
        /// </summary>
        public void ClearAllWriteLog()
        {
            ROFile.ClearAllWriteLog();
        }

        /// <summary>
        /// ReadOnlyFileのWriteLogをクリアする
        /// </summary>
        /// <param name="path"></param>
        public void ClearWriteLog(string path)
        {
            ROFile.ClearWriteLog(path);
        }
    }
}
