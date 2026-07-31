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
            // ここでSimModelのサービスを登録する
            // 本来はIFと実装を分離すべきだが、一旦見送り(必要に応じて変更する)
            services.AddSingleton<Simulator, Simulator>();
            services.AddSingleton<SearcherFactory, SearcherFactory>();
            services.AddSingleton<DataManagement, DataManagement>();
            services.AddSingleton<FileOperation, FileOperation>();
            services.AddSingleton<CharmAppraiser, CharmAppraiser>();
            return services;
        }
    }
}
