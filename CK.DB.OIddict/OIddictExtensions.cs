using CK.DB.OIddict.Entities;
using CK.DB.OIddict.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace CK.DB.OIddict
{
    public static class OIddictExtensions
    {
        /// <summary>
        /// Add OpenIddict Core and registers the Sql stores services in the DI container and
        /// configures OpenIddict to use the related Sql entities by default.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddOIddict( this IServiceCollection services )
        {
            services.AddOpenIddict()
                    .AddCore( builder => builder.UseOpenIddictCoreSql() );

            return services;
        }

        public static OpenIddictCoreBuilder UseOpenIddictCoreSql( this OpenIddictCoreBuilder builder )
        {
            builder.SetDefaultApplicationEntity<Application>()
                   .SetDefaultAuthorizationEntity<Authorization>()
                   .SetDefaultScopeEntity<Scope>()
                   .SetDefaultTokenEntity<Token>();

            builder.AddApplicationStore<OIddictApplicationStore>();
            builder.AddAuthorizationStore<OIddictAuthorizationStore>();
            builder.AddScopeStore<OIddictScopeStore>();
            builder.AddTokenStore<OIddictTokenStore>();

            return builder;
        }
    }
}
