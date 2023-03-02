
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;
using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PrismaMvcServiceCollectionExtensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type t in types)
            {
                if (t.Namespace == "Services" && t.Name.EndsWith("Service"))
                {
                    services.AddScoped(t);
                }
            }
        }
        public static void AddRepositories(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type t in types)
            {
                if (t.Namespace == "Repositories" && t.Name.EndsWith("Repository"))
                {
                    services.AddScoped(t);
                }
            }
        }
    }
}