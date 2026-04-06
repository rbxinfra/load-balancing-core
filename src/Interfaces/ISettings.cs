namespace Roblox.LoadBalancing;

using System;

/// <summary>
/// Represents the settings for the load balancing system. 
/// </summary>
public interface ISettings
{
    /// <summary>
    /// Gets a value indicating whether the grace period for new service instances is enabled.
    /// If enabled, new service instances will be given a grace period during which they may not receive traffic, allowing them to warm up before handling requests.
    /// </summary>
    bool NewServiceInstanceGracePeriodEnabled { get; }

    /// <summary>
    /// Gets the duration of the grace period for new service instances. 
    /// During this period, new service instances may not receive traffic, 
    /// allowing them to warm up before handling requests.
    /// </summary>
    TimeSpan InitialServiceInstanceGracePeriod { get; }

    /// <summary>
    /// Gets a value indicating whether the grace period feature is enabled.
    /// If enabled, new service instances will be given a grace period during which they may not
    /// receive traffic, allowing them to warm up before handling requests.
    /// </summary>
    bool GracePeriodFeatureEnabled { get; }

    /// <summary>
    /// Gets the interval at which healthy service instances are updated.
    /// During each update, the load balancer will check the health of service instances 
    /// and update the list of healthy instances accordingly.
    /// </summary>
    TimeSpan UpdateHealthyInstancesInterval { get; }

    /// <summary>
    /// Gets a value indicating whether the worker responsible for updating healthy service instances is enabled.
    /// If enabled, the load balancer will periodically check the health of service instances 
    /// and update the list of healthy instances accordingly.
    /// </summary>
    bool UpdateHealthyInstancesWorkerEnabled { get; }
}