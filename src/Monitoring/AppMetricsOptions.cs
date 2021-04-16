using App.Metrics;
using App.Metrics.AspNetCore.Endpoints;
using App.Metrics.AspNetCore.Tracking;

namespace Monitoring
{
    public class AppMetricsOptions
    {
        public bool IsEnabled { get; set; }
        public InfluxDbOptions InfluxDbOptions { get; set; }
        public MetricsOptions MetricsOptions { get; set; }

        public MetricsWebTrackingOptions MetricsWebTrackingOptions { get; set; }

        public MetricEndpointsOptions MetricEndpointsOptions { get; set; }
    }

    public class InfluxDbOptions
    {
        public string Url { get; set; }
        public string Database { get; set; }
        public double FlushInterval { get; set; }
    }
}
