// Copyright (c) 2026 Gianni Rosa Gallina.
// Copyright (c) Microsoft. All rights reserved.

#pragma warning disable VSTHRD002 // Synchronous waits are required by OpenTelemetry enrichment callbacks.

using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Harness.Shared.Console;

/// <summary>
/// Provides factory methods for creating pre-configured OpenTelemetry tracing for harness samples.
/// </summary>
public static class HarnessTracing
{
    /// <summary>
    /// Creates a <see cref="TracerProvider"/> that captures spans from the specified source and HTTP client activity,
    /// enriching HTTP spans with full request/response headers and bodies, and exports all spans to:
    /// <list type="bullet">
    ///   <item><description>A timestamped text file in the application base directory</description></item>
    ///   <item><description>OTLP/gRPC endpoint (default: http://localhost:4317) for OpenTelemetry collectors</description></item>
    /// </list>
    /// </summary>
    /// <param name="sourceName">The activity source name to subscribe to (e.g., "Harness.Research").</param>
    /// <param name="otlpEndpoint">Optional OTLP/gRPC endpoint URL. Defaults to http://localhost:4317.</param>
    /// <returns>A configured <see cref="TracerProvider"/>, or <see langword="null"/> if the builder returns null.</returns>
    public static TracerProvider? CreateFileTracerProvider(string sourceName, string? otlpEndpoint = null)
    {
        var traceLogPath = Path.Combine(AppContext.BaseDirectory, $"traces_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid()}.log");

        return Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(sourceName))
            .AddSource(sourceName)
            .AddHttpClientInstrumentation((options) =>
            {
                options.EnrichWithHttpRequestMessage = (activity, request) =>
                {
                    activity.SetTag("http.request.headers", request.Headers.ToString());
                    if (request.Content != null)
                    {
                        activity.SetTag("http.request.content.headers", request.Content.Headers.ToString());
                        var content = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        activity.SetTag("http.request.content.body", content);
                    }
                };

                options.EnrichWithHttpResponseMessage = (activity, response) =>
                {
                    activity.SetTag("http.response.headers", response.Headers.ToString());
                    if (response.Content != null)
                    {
                        activity.SetTag("http.response.content.headers", response.Content.Headers.ToString());
                        var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        activity.SetTag("http.response.content.body", content);
                    }
                };
            })
            .AddProcessor(new SimpleActivityExportProcessor(new FileSpanExporter(traceLogPath)))
            .AddOtlpExporter(options =>
            {
                options.Protocol = OtlpExportProtocol.Grpc;
                options.Endpoint = new Uri(otlpEndpoint ?? "http://localhost:4317");
            })
            .Build();
    }
}
