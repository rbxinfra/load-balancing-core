namespace Roblox.LoadBalancing;

using System;
using System.Linq;
using System.Collections.Generic;

using EventLog;

using Exceptions;

/// <summary>
/// Implements a random load balancing algorithm that distributes incoming traffic randomly across healthy service instances.
/// </summary>
public class RandomBalancer : LoadBalancerBase
{
    private readonly Random _Random = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RandomBalancer"/> class.
    /// </summary>
    /// <param name="settings">The settings for the load balancer.</param>
    /// <param name="logger">The logger for the load balancer.</param>
    /// <exception cref="ArgumentNullException">
    /// - <paramref name="settings"/> is null.
    /// - <paramref name="logger"/> is null.
    /// </exception>
    public RandomBalancer(ISettings settings, ILogger logger) : base(settings, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RandomBalancer"/>.
    /// </summary>
    /// <param name="initialInstances">The initial list of service instances to register with the
    /// load balancer with their weights.</param>
    /// <param name="settings">The settings for the load balancer.</param>
    /// <param name="logger">The logger for the load balancer.</param>
    /// <exception cref="ArgumentNullException">
    /// - <paramref name="initialInstances"/> is null.
    /// - <paramref name="settings"/> is null.
    /// - <paramref name="logger"/> is null.
    /// </exception>
    public RandomBalancer(IReadOnlyList<IServiceInstance> initialInstances, ISettings settings, ILogger logger) 
        : this(settings, logger)
    {
        if (initialInstances is null)
            throw new ArgumentNullException(nameof(initialInstances));

        foreach (var instance in initialInstances)
            RegisterInstance(instance);
    }

    /// <inheritdoc cref="ILoadBalancer.GetInstance"/>
    public override IServiceInstance GetInstance()
    {
        lock (_HealthyInstancesLock)
        {
            if (!_HealthyServiceInstances.Any())
                throw new NoServiceInstanceAvailableException();

            var index = _Random.Next(_HealthyServiceInstances.Count);
            return _HealthyServiceInstances[index];
        }
    }
}