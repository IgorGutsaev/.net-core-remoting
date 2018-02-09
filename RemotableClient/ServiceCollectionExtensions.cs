using Microsoft.Extensions.DependencyInjection;
using RemotableInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableClient
{
    public static class ServiceCollectionExtensions
    {
        //public static IServiceCollection AddRemote(this IServiceCollection serviceCollection,/* Type serviceType,*/ Uri address)
        //{
        //    return serviceCollection.AddSingleton<IMyService>(sp => {
        //        var broker = sp.GetRequiredService<IRemoteServiceBroker>();

        //        return new MyServiceWrapper(broker.ActivateService(typeof(IMyService)));
        //    });
        //}
    }
}
