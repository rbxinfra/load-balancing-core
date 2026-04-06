namespace Roblox.LoadBalancing;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

using EventLog;

using Exceptions;

/// <summary>
/// Implements a weighted load balancing algorithm that distributes incoming traffic across healthy service instances based on their assigned weights.
/// </summary>
public class WeightedBalancer : LoadBalancerBase
{
    private readonly Random _Random = new();
    private readonly ConcurrentDictionary<string, int> _InstanceWeights = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WeightedBalancer"/> class.
    /// </summary>
    /// <param name="settings">The settings for the load balancer.</param>
    /// <param name="logger">The logger for the load balancer.</param>
    /// <exception cref="ArgumentNullException">
    /// - <paramref name="settings"/> is null.
    /// - <paramref name="logger"/> is null.
    /// </exception>
    public WeightedBalancer(ISettings settings, ILogger logger) 
        : base(settings, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeightedBalancer"/>.
    /// </summary>
    /// <param name="initialInstances">The initial list of service instances to register with the load balancer with their weights.</param>
    /// <param name="settings">The settings for the load balancer.</param>
    /// <param name="logger">The logger for the load balancer.</param>
    /// <exception cref="ArgumentNullException">
    /// - <paramref name="initialInstances"/> is null.
    /// - <paramref name="settings"/> is null.
    /// - <paramref name="logger"/> is null.
    /// </exception>
    public WeightedBalancer(IReadOnlyList<(IServiceInstance Instance, int Weight)> initialInstances, ISettings settings, ILogger logger) 
        : this(settings, logger)
    {
        if (initialInstances is null)
            throw new ArgumentNullException(nameof(initialInstances));

        foreach (var (instance, weight) in initialInstances)
        {
            RegisterInstance(instance);
            SetInstanceWeight(instance.Id, weight);
        }
    }

    /// <summary>
    /// Sets the weight for a specific service instance. The load balancer will use these weights to determine how to distribute traffic among healthy instances, with higher-weighted instances receiving more traffic.
    /// </summary>
    /// <param name="instanceId">The ID of the service instance to set the weight for.</param>
    /// <param name="weight">The weight to assign to the service instance. Must be a non-negative integer, where higher values indicate a greater share of traffic.</param>
    /// <exception cref="ArgumentException">
    /// - <paramref name="instanceId"/> is null or empty.
    /// - <paramref name="weight"/> is negative.
    /// </exception>
    public void SetInstanceWeight(string instanceId, int weight)
    {
        if (string.IsNullOrEmpty(instanceId))
            throw new ArgumentException("Instance ID cannot be null or empty.", nameof(instanceId));

        if (weight < 0)
            throw new ArgumentException("Weight must be a non-negative integer.", nameof(weight));

        _InstanceWeights[instanceId] = weight;
    }

    /// <inheritdoc cref="ILoadBalancer.GetInstance"/>
    public override IServiceInstance GetInstance()
    {
        lock (_HealthyInstancesLock)
        {
            if (!_HealthyServiceInstances.Any())
                throw new NoServiceInstanceAvailableException();

            var totalWeight = _HealthyServiceInstances.Sum(instance =>
            {
                if (_InstanceWeights.TryGetValue(instance.Id, out var weight))
                    return weight;

                // Default weight is 1 if not specified
                return 1;
            });
            var randomValue = _Random.Next(totalWeight);
            var cumulativeWeight = 0;

            foreach (var instance in _HealthyServiceInstances)
            {
                var weight = _InstanceWeights.TryGetValue(instance.Id, out var w) ? w : 1;
                cumulativeWeight += weight;

                if (randomValue < cumulativeWeight)
                    return instance;
            }

            // Fallback in case of an error in weight calculation
            return _HealthyServiceInstances.First();
        }
    }
}