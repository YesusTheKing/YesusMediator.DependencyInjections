using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesusMediator.Requests;
using YesusMediator.Handlers;
using System.Reflection;
using YesusMediator.Mediators;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace YesusMediator.DependencyInjections
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediator(this IServiceCollection services,  ServiceLifetime lifetime,params Type[] markers)
        {
            var hanlderInfo = new Dictionary<Type, Type>();
            foreach(var marker in markers)
            {
                var assembly = marker.Assembly;

                var requests = GetRequestTypes(assembly, typeof(IRequest<>));
                var hanlders = GetRequestTypes(assembly, typeof(IHandler<,>));

                requests.ForEach(requestType =>
                {
                    hanlderInfo[requestType] = hanlders.SingleOrDefault(x => requestType == x.GetInterface("IHandler`2")!.GetGenericArguments()[0])!;
                });
                var serviceDescriptor = hanlders.Select(x => new ServiceDescriptor(x, x, lifetime));
                services.TryAdd(serviceDescriptor);


            }

            services.AddSingleton<IMediator>(x => new Mediator(hanlderInfo, x.GetRequiredService));

            return services;
        }

        public static List<Type> GetRequestTypes(Assembly assembly,Type typeToMatch)
        {
            var iHandlerTypes = assembly.ExportedTypes.Where(type =>
            {
                var genericTypes = type.GetInterfaces().Where(x => x.IsInterface).ToList();
                var implementHandlerType = genericTypes.Any(x => x.GetGenericTypeDefinition() == typeToMatch);
                return !type.IsInterface && !type.IsAbstract && implementHandlerType;
            }).ToList();

            return iHandlerTypes;
        }

        public static List<Type> GetHanlderImpRequestTypes(Assembly assembly, Type typeToMatch)
        {
            var iRequestTypes = assembly.ExportedTypes.Where(type =>
            {
                var genericTypes = type.GetInterfaces().Where(x => x.IsInterface).ToList();
                var implementRequestType = genericTypes.Any(x => x.GetGenericTypeDefinition() == typeToMatch);
                return !type.IsInterface && !type.IsAbstract && implementRequestType;
            }).ToList();

            return iRequestTypes;
        }


    }
}
