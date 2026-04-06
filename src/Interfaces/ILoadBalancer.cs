namespace Roblox.LoadBalancing;

using System.Collections.Generic;

using Exceptions;

/// <summary>
/// Interface for a generic load balancer that distributes incoming traffic across multiple service instances.
/// </summary>
public interface ILoadBalancer
{
    /// <summary>
    /// Gets the list of service instances that can receive traffic.
    /// </summary>
    IReadOnlyList<IServiceInstance> HealthyServiceInstances { get; }

    /// <summary>
    /// Gets any healthy service instance using the load balancing algorithm implemented by the load balancer.
    /// </summary>
    /// <remarks>
    /// The specific load balancing algorithm (e.g., round-robin, least connections, etc.) 
    /// is determined by the implementation of the load balancer.
    /// </remarks>
    /// <returns>A healthy service instance that can receive traffic.</returns>
    /// <exception cref="NoServiceInstanceAvailableException">Thrown when there are no healthy service instances available to receive traffic.</exception>
    IServiceInstance GetInstance();

    /// <summary>
    /// Registers a new service instance with the load balancer.
    /// The load balancer will monitor the health of the instance and include it in the pool of healthy instances if it is healthy.
    /// </summary>
    /// <param name="instance">The service instance to register.</param>
    void RegisterInstance(IServiceInstance instance);

    /// <summary>
    /// Unregisters a service instance from the load balancer.
    /// The load balancer will stop monitoring the health of the instance and remove it from the pool of healthy instances.
    /// </summary>
    /// <param name="instanceId">The ID of the service instance to unregister.</param>
    void UnregisterInstance(string instanceId);
}