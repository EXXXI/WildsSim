// SimModel/DependencyInjection/ServiceCollectionExtensions.cs
using Microsoft.Extensions.DependencyInjection;
using SimModel.Config;
using SimModel.Domain;
using SimModel.Model;
using System.IO.Abstractions;

namespace SimModel.Service
{
    /// <summary>
    /// ServiceCollection拡張メソッドクラス
    /// SimModelのサービス登録用
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// SimModelのサービスを登録
        /// </summary>
        /// <param name="services">サービス</param>
        /// <returns>サービス</returns>
        public static IServiceCollection AddSimModelServices(this IServiceCollection services)
        {
            // ここでSimModelのサービスを登録する
            // 本来はIFと実装を分離すべきだが、一旦見送り(必要に応じて変更する)
            services.AddSingleton<Simulator>();
            services.AddSingleton<DataManagement>();
            services.AddSingleton<FileOperation>();
            services.AddSingleton<CharmAppraiser>();
            services.AddSingleton<LogicConfig>();
            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddSingleton<Masters>();

            return services;
        }
    }
}
