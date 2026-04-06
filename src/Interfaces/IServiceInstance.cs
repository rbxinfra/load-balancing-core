namespace Roblox.LoadBalancing;

using System;
using System.Net;

/// <summary>
/// Represents a service instance that can receive traffic from the load balancer.
/// </summary>
public interface IServiceInstance
{
    /// <summary>
    /// Gets the unique identifier for the service instance.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the endpoint (IP address and port) of the service instance.
    /// </summary>
    IPEndPoint EndPoint { get; }

    /// <summary>
    /// Gets a value indicating whether the service instance is healthy and can receive traffic.
    /// </summary>
    bool IsHealthy { get; }

    /// <summary>
    /// Gets the timestamp of the service instance's registration with the load balancer.
    /// </summary>
    DateTime RegistrationTime { get; }
}