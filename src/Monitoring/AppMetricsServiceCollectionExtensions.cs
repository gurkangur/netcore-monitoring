using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.AspNetCore.Endpoints;
using App.Metrics.AspNetCore.Tracking;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Monitoring
{
    public static class AppMetricsServiceCollectionExtensions
    {
        public static IServiceCollection AddMonitoringServices(this IServiceCollection services, AppMetricsOptions appMetricsOptions)
        {
            if (appMetricsOptions?.IsEnabled ?? false)
            {
                var metrics = AppMetrics.CreateDefaultBuilder()
                .Configuration.Configure(appMetricsOptions.MetricsOptions)
                .Report.ToInfluxDb(appMetricsOptions.InfluxDbOptions.Url, appMetricsOptions.InfluxDbOptions.Database, TimeSpan.FromSeconds(appMetricsOptions.InfluxDbOptions.FlushInterval))
                .OutputMetrics.AsPrometheusPlainText()
                .Build();

                var options = new MetricsWebHostOptions
                {
                    EndpointOptions = endpointsOptions =>
                    {
                        endpointsOptions.MetricsEndpointEnabled = appMetricsOptions.MetricEndpointsOptions.MetricsEndpointEnabled;
                        endpointsOptions.MetricsTextEndpointEnabled = appMetricsOptions.MetricEndpointsOptions.MetricsTextEndpointEnabled;
                        endpointsOptions.EnvironmentInfoEndpointEnabled = appMetricsOptions.MetricEndpointsOptions.EnvironmentInfoEndpointEnabled;
                        endpointsOptions.MetricsTextEndpointOutputFormatter = metrics.OutputMetricsFormatters.OfType<MetricsPrometheusTextOutputFormatter>().First();
                        endpointsOptions.MetricsEndpointOutputFormatter = metrics.OutputMetricsFormatters.OfType<MetricsPrometheusTextOutputFormatter>().First();
                    },

                    TrackingMiddlewareOptions = trackingMiddlewareOptions =>
                    {
                        trackingMiddlewareOptions.ApdexTrackingEnabled = appMetricsOptions.MetricsWebTrackingOptions.ApdexTrackingEnabled;
                        trackingMiddlewareOptions.ApdexTSeconds = appMetricsOptions.MetricsWebTrackingOptions.ApdexTSeconds;
                        trackingMiddlewareOptions.IgnoredHttpStatusCodes = appMetricsOptions.MetricsWebTrackingOptions.IgnoredHttpStatusCodes;
                        trackingMiddlewareOptions.IgnoredRoutesRegexPatterns = appMetricsOptions.MetricsWebTrackingOptions.IgnoredRoutesRegexPatterns;
                        trackingMiddlewareOptions.OAuth2TrackingEnabled = appMetricsOptions.MetricsWebTrackingOptions.OAuth2TrackingEnabled;
                    },
                };

                services.AddMetrics(metrics);

                services.AddMetricsReportingHostedService(options.UnobservedTaskExceptionHandler);
                services.AddMetricsEndpoints(options.EndpointOptions);
                services.AddMetricsTrackingMiddleware(options.TrackingMiddlewareOptions);

                services.AddSingleton<IStartupFilter>(new DefaultMetricsEndpointsStartupFilter());
                services.AddSingleton<IStartupFilter>(new DefaultMetricsTrackingStartupFilter());
            }

            return services;
        }
        public static IMvcBuilder AddMonitoringServices(this IMvcBuilder mvcBuilder)
        {
            mvcBuilder.AddMetrics();
            return mvcBuilder;
        }
    }
}
