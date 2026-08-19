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
    /// テスト用基底クラス
    /// DIコンテナへの登録と、Mastersの読み込みを行う
    /// ファイルの書き込みはモック化し、書き込みのかわりにログが残る
    /// </summary>
    public class TestDataSetUp
    {

        /// <summary>
        /// DIコンテナのプロバイダ
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// 内部で使用するReadOnlyFileのインスタンス
        /// </summary>
        private ReadOnlyFile ROFile { get; }

        /// <summary>
        /// WriteAllTextの呼び出しをログとして残すためのDictionary
        /// </summary>
        protected Dictionary<string, List<string>> WriteLog
        {
            get
            {
                return ROFile.WriteLog;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected TestDataSetUp()
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
            ClearAllWriteLog();
        }

        /// <summary>
        /// ReadOnlyFileのWriteLogを全てクリアする
        /// </summary>
        protected void ClearAllWriteLog()
        {
            ROFile.ClearAllWriteLog();
        }

        /// <summary>
        /// ReadOnlyFileのWriteLogをクリアする
        /// </summary>
        /// <param name="path"></param>
        protected void ClearWriteLog(string path)
        {
            ROFile.ClearWriteLog(path);
        }

        /// <summary>
        /// Masterの再読み込みとWriteLogのクリア
        /// </summary>
        protected void Reload()
        {
            ServiceProvider.GetRequiredService<DataManagement>().LoadData();
            ROFile.ClearAllWriteLog();
        }
    }
}
