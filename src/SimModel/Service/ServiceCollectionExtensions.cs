// SimModel/DependencyInjection/ServiceCollectionExtensions.cs
using Microsoft.Extensions.DependencyInjection;

namespace SimModel.Service
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimModelServices(this IServiceCollection services)
        {
            // TODO: ここにSimModelのサービスを登録する
            //services.AddSingleton<ISimulator, Simulator>();
            // 他のサービスも必要に応じて登録
            return services;
        }
    }
}
