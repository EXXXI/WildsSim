using WildsSim.ViewModels;
using WildsSim.Views;
using System;
using System.Windows;
using NLog;
using Microsoft.Extensions.DependencyInjection;
using SimModel.Service;

namespace WildsSim
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// ロガー
        /// </summary>
        static Logger logger = LogManager.GetCurrentClassLogger();

        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// 開始時処理
        /// MainViewModelをバインドしたMainViewを開く
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
       {
            // DIコンテナにサービスを登録
            var services = new ServiceCollection();
            services.AddSimModelServices();
            ServiceProvider = services.BuildServiceProvider();

            base.OnStartup(e);

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyUnhandledExceptionHandler);

            var w = new MainView();
            var vm = new MainViewModel();

            w.DataContext = vm;
            w.Show();
        }

        /// <summary>
        /// 予期せぬエラー時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        static void MyUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            logger.Error(e, "エラーが発生しました。");
        }
    }
}
