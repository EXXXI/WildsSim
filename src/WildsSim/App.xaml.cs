using WildsSim.ViewModels;
using WildsSim.Views;
using System;
using System.Windows;
using NLog;
using Microsoft.Extensions.DependencyInjection;
using SimModel.Service;
using WildsSim.ViewModels.SubViews;
using SimModel.ExceptionClass;
using System.Text;

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
            services.AddSingleton<ArtianTabViewModel>();
            services.AddSingleton<CharmTabViewModel>();
            services.AddSingleton<CludeTabViewModel>();
            services.AddSingleton<DecoTabViewModel>();
            services.AddSingleton<LicenseTabViewModel>();
            services.AddSingleton<MySetTabViewModel>();
            services.AddSingleton<SimulatorTabViewModel>();
            services.AddSingleton<SkillSelectTabViewModel>();
            ServiceProvider = services.BuildServiceProvider();

            // データのロードのため、Simulatorを最初にインスタンス化
            ServiceProvider.GetRequiredService<Simulator>();

            base.OnStartup(e);

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyUnhandledExceptionHandler);

            var w = new MainView();
            var vm = new MainViewModel();

            w.DataContext = vm;
            w.Show();
        }

        /// <summary>
        /// エラー時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        static void MyUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            StringBuilder message = new();
            if (e is SimulatorException)
            {
                message.AppendLine(e.Message);
            }
            else
            {
                message.AppendLine("予期せぬエラーが発生しました。");
            }
            message.AppendLine("詳細はlogsフォルダ配下のログファイルを参照してください。");
            MessageBox.Show(message.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            logger.Error(e, "エラーが発生しました。");
        }
    }
}
