namespace Roblox.LoadBalancing;

using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using EventLog;

using Exceptions;

/// <summary>
/// Implements a round-robin load balancing algorithm that distributes incoming traffic evenly across healthy service instances.
/// </summary>
public class RoundRobinBalancer : LoadBalancerBase
{
    private int _CurrentIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoundRobinBalancer"/> class.
    /// </summary>
    /// <param name="settings">The settings for the load balancer.</param>
    /// <param name="logger">The logger for the load balancer.</param>
    /// <exception cref="ArgumentNullException">
    /// - <paramref name="settings"/> is null.
    /// - <paramref name="logger"/> is null.
    /// </exception>
    public RoundRobinBalancer(ISettings settings, ILogger logger) 
        : base(settings, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoundRobinBalancer"/>.
    /// </summary>
    /// <param name="initialInstances">The initial list of service instances to register with the load balancer.</param>
    /// <param name="settings">The settings for the load balancer.</param>
    /// <param name="logger">The logger for the load balancer.</param>
    /// <exception cref="ArgumentNullException">
    /// - <paramref name="initialInstances"/> is null.
    /// - <paramref name="settings"/> is null.
    /// - <paramref name="logger"/> is null.
    /// </exception>
    public RoundRobinBalancer(IReadOnlyList<IServiceInstance> initialInstances, ISettings settings, ILogger logger) 
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

            var instance = _HealthyServiceInstances[_CurrentIndex % _HealthyServiceInstances.Count];
            Interlocked.Increment(ref _CurrentIndex);

            return instance;
        }
    }
}