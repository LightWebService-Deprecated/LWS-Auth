using System;
using System.Linq;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LWSAuthServiceTest.Extension;

public static class ServiceCollectionExtension
{
    public static void RemoveProductionMassTransit(this IServiceCollection serviceCollection)
    {
        var massTransitServices = serviceCollection.FirstOrDefault(a =>
            a.ServiceType == typeof(IHostedService) && a.ImplementationFactory != null &&
            a.ImplementationFactory.Method.ReturnType == typeof(MassTransitHostedService));
        
        serviceCollection.Remove(massTransitServices);
        var massTransitDescriptors = serviceCollection.Where(a =>
            a.ServiceType.Namespace.Contains("MassTransit", StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var eachDescriptor in massTransitDescriptors)
        {
            serviceCollection.Remove(eachDescriptor);
        }
    } 
}