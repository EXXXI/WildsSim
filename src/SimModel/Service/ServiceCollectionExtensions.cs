// SimModel/DependencyInjection/ServiceCollectionExtensions.cs
using Microsoft.Extensions.DependencyInjection;
using SimModel.Config;
using SimModel.Domain;
using SimModel.Model;

namespace SimModel.Service
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimModelServices(this IServiceCollection services)
        {
            // ここにSimModelのサービスを登録する
            services.AddSingleton<Simulator, Simulator>();
            //services.AddTransient<Searcher, Searcher>();
            services.AddSingleton<DataManagement, DataManagement>();
            services.AddSingleton<FileOperation, FileOperation>();
            // 他のサービスも必要に応じて登録
            return services;
        }
    }
}
