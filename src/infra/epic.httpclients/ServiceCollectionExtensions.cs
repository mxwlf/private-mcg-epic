using Mcg.Edge.Fhir.Epic.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Mcg.Edge.Fhir.Epic.HttpClients;

/// <summary>
/// Extension methods for registering Epic client services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Epic OAuth2 client to the service collection and binds configuration from the specified section.
    /// Expected configuration structure:
    /// - epic:auth:clientid
    /// - epic:auth:tokenendpoint
    /// - epic:auth:privatekey
    /// - epic:auth:jwtexpirationseconds (optional, defaults to 240)
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="configurationSectionPath">The configuration section path (default: "epic:auth")</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddEpicOAuth2Client(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSectionPath = "epic:auth")
    {
        // Bind and validate the configuration
        services.AddOptions<EpicAuthClientOptions>()
            .Configure(options => configuration.GetSection(configurationSectionPath).Bind(options))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register the EpicOAuth2Client
        services.AddSingleton<EpicOAuth2Client>();

        return services;
    }

    /// <summary>
    /// Adds the Epic OAuth2 client to the service collection with direct configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure the options</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddEpicOAuth2Client(
        this IServiceCollection services,
        Action<EpicAuthClientOptions> configureOptions)
    {
        services.AddOptions<EpicAuthClientOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<EpicOAuth2Client>();

        return services;
    }

    /// <summary>
    /// Adds the Epic OAuth2 client to the service collection with custom OptionsBuilder configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="configurationSectionPath">The configuration section path</param>
    /// <param name="configureOptionsBuilder">Action to further configure the OptionsBuilder</param>
    /// <returns>The OptionsBuilder for further configuration</returns>
    public static OptionsBuilder<EpicAuthClientOptions> AddEpicOAuth2ClientWithOptions(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSectionPath,
        Action<OptionsBuilder<EpicAuthClientOptions>>? configureOptionsBuilder = null)
    {
        var optionsBuilder = services.AddOptions<EpicAuthClientOptions>()
            .Configure(options => configuration.GetSection(configurationSectionPath).Bind(options))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Allow further customization of the options builder
        configureOptionsBuilder?.Invoke(optionsBuilder);

        services.AddSingleton<EpicOAuth2Client>();

        return optionsBuilder;
    }

    /// <summary>
    /// Adds the Epic Medications HTTP client to the service collection and binds configuration from the specified section.
    /// Expected configuration structure:
    /// - epic:medications:baseurl (required)
    /// - epic:medications:getcurrentmedicationspath (optional, has default)
    /// - epic:medications:getmedicationadministrationhistorypath (optional, has default)
    /// - epic:medications:timeoutseconds (optional, defaults to 30)
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="configurationSectionPath">The configuration section path (default: "epic:medications").</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddEpicMedicationsClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSectionPath = "epic:medications")
    {
        // Bind and validate the configuration
        services.AddOptions<EpicMedicationsClientOptions>()
            .Configure(options => configuration.GetSection(configurationSectionPath).Bind(options))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register the HTTP client with IHttpClientFactory
        services.AddHttpClient<IEpicCurrentMedicationsClient, EpicMedicationsHttpClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptionsMonitor<EpicMedicationsClientOptions>>().CurrentValue;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        return services;
    }

    /// <summary>
    /// Adds the Epic Medications HTTP client to the service collection with direct configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure the options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddEpicMedicationsClient(
        this IServiceCollection services,
        Action<EpicMedicationsClientOptions> configureOptions)
    {
        services.AddOptions<EpicMedicationsClientOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register the HTTP client with IHttpClientFactory
        services.AddHttpClient<IEpicCurrentMedicationsClient, EpicMedicationsHttpClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptionsMonitor<EpicMedicationsClientOptions>>().CurrentValue;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        return services;
    }

    /// <summary>
    /// Adds the Epic Medications HTTP client to the service collection with custom OptionsBuilder configuration.
    /// Provides access to IHttpClientBuilder for advanced HTTP client configuration (retry policies, handlers, etc.).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="configurationSectionPath">The configuration section path.</param>
    /// <param name="configureHttpClient">Optional action to configure the HttpClient.</param>
    /// <returns>The IHttpClientBuilder for further HTTP client configuration.</returns>
    public static IHttpClientBuilder AddEpicMedicationsClientWithHttpClientBuilder(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSectionPath,
        Action<IServiceProvider, System.Net.Http.HttpClient>? configureHttpClient = null)
    {
        // Bind and validate the configuration
        services.AddOptions<EpicMedicationsClientOptions>()
            .Configure(options => configuration.GetSection(configurationSectionPath).Bind(options))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register the HTTP client with IHttpClientFactory and return the builder
        var builder = services.AddHttpClient<IEpicCurrentMedicationsClient, EpicMedicationsHttpClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptionsMonitor<EpicMedicationsClientOptions>>().CurrentValue;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            // Allow custom configuration
            configureHttpClient?.Invoke(serviceProvider, client);
        });

        return builder;
    }
}
