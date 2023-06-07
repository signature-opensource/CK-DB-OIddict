using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using CK.DB.OpenIddictSql.Dapper;
using CK.DB.OpenIddictSql.Entities;
using CK.DB.OpenIddictSql.Stores;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace CK.DB.OpenIddictSql
{
    public static class OpenIddictSqlExtensions
    {
        /// <summary>
        /// Add OpenIddict Core and registers the Sql stores services in the DI container and
        /// configures OpenIddict to use the related Sql entities by default.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddOpenIddictSql( this IServiceCollection services )
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

            builder.AddApplicationStore<OpenIddictSqlApplicationStore>();
            builder.AddAuthorizationStore<OpenIddictSqlAuthorizationStore>();
            builder.AddScopeStore<OpenIddictSqlScopeStore>();
            builder.AddTokenStore<OpenIddictSqlTokenStore>();

            //TODO: Handle entities types to prevent conflicts.
            SqlMapper.AddTypeHandler( new JsonTypeHandler<Dictionary<CultureInfo, string>>() );
            SqlMapper.AddTypeHandler( new JsonTypeHandler<HashSet<string>>() );
            SqlMapper.AddTypeHandler( new JsonTypeHandler<HashSet<Uri>>() );
            SqlMapper.AddTypeHandler( new JsonTypeHandler<Dictionary<string, JsonElement>>() );
            SqlMapper.AddTypeHandler( new GuidToStringTypeHandler() );
            SqlMapper.AddTypeHandler( new StringToGuidTypeHandler() );

            return builder;
        }
    }
}
