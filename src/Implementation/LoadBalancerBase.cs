namespace Roblox.LoadBalancing;

using System;
using System.Threading;
using System.Collections.Generic;

using EventLog;

/// <summary>
/// Base class for load balancers that provides common functionality for managing service instances and their health status.
/// </summary>
public abstract class LoadBalancerBase : ILoadBalancer
{
    /// <summary>
    /// List of all registered service instances, regardless of their health status.
    /// </summary>
    protected readonly List<IServiceInstance> _ServiceInstances = new();

    /// <summary>
    /// List of healthy service instances that can receive traffic. This list is updated periodically based on the health status of registered service instances and the grace period settings.
    /// </summary>
    protected readonly List<IServiceInstance> _HealthyServiceInstances = new();

    /// <summary>
    /// Lock object used to synchronize access to the list of healthy service instances.
    /// </summary>
    protected readonly object _HealthyInstancesLock = new();

    /// <summary>
    /// The <see cref="ISettings"/> instance containing the configuration settings for the load balancer.
    /// </summary>
    protected readonly ISettings _Settings;

    /// <summary>
    /// The <see cref="ILogger"/> instance used for logging information, warnings, and errors related to the load balancer's operations.
    /// </summary>
    protected readonly ILogger _Logger;

    /// <inheritdoc cref="ILoadBalancer.HealthyServiceInstances"/>
    public IReadOnlyList<IServiceInstance> HealthyServiceInstances
    {
        get
        {
            lock (_HealthyInstancesLock)
                return _HealthyServiceInstances.AsReadOnly();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadBalancerBase"/>.
    /// </summary>
    /// <param name="settings">The settings for the load balancer.</param>
    /// <param name="logger">The logger for the load balancer.</param>
    /// <exception cref="ArgumentNullException">
    /// - <paramref name="settings"/> is null.
    /// - <paramref name="logger"/> is null.
    /// </exception>
    protected LoadBalancerBase(ISettings settings, ILogger logger)
    {
        _Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (_Settings.UpdateHealthyInstancesWorkerEnabled)
        {
            new Timer(
                _ => UpdateHealthyInstances(), 
                null, 
                TimeSpan.Zero, 
                _Settings.UpdateHealthyInstancesInterval
            );

            _Logger.Information("Started UpdateHealthyInstances worker with interval {0}", _Settings.UpdateHealthyInstancesInterval);
        }
    }

    /// <inheritdoc cref="ILoadBalancer.RegisterInstance(IServiceInstance)"/>
    public void RegisterInstance(IServiceInstance instance)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        lock (_HealthyInstancesLock)
        {
            _ServiceInstances.Add(instance);
            _Logger.Information("Registered service instance {0} at {1}", instance.Id, instance.EndPoint);

            if (instance.IsHealthy)
                _HealthyServiceInstances.Add(instance);
        }
    }

    /// <inheritdoc cref="ILoadBalancer.UnregisterInstance(string)"/>
    public void UnregisterInstance(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            throw new ArgumentException("Instance ID cannot be null or empty.", nameof(instanceId));

        lock (_HealthyInstancesLock)
        {
            _ServiceInstances.RemoveAll(instance => instance.Id == instanceId);
            _HealthyServiceInstances.RemoveAll(instance => instance.Id == instanceId);
            
            _Logger.Information("Unregistered service instance with ID {0}", instanceId);
        }
    }

    /// <summary>
    /// Determines whether a given service instance is currently within its grace period based on the load balancer's settings and the instance's registration time.
    /// </summary>
    /// <param name="instance">The <see cref="IServiceInstance"/> to check for grace period status.</param>
    /// <returns>[true] if the instance is within its grace period; otherwise, [false].</returns>
    protected virtual bool IsInGracePeriod(IServiceInstance instance)
    {
        if (!_Settings.GracePeriodFeatureEnabled || !_Settings.NewServiceInstanceGracePeriodEnabled)
            return false;

        var timeSinceRegistration = DateTime.UtcNow - instance.RegistrationTime;
        
        return timeSinceRegistration < _Settings.InitialServiceInstanceGracePeriod;
    }

    /// <summary>
    /// Updates the list of healthy service instances by checking the health status of all registered service instances and applying the grace period logic as necessary.
    /// </summary>
    protected virtual void UpdateHealthyInstances()
    {
        lock (_HealthyInstancesLock)
        {
            _HealthyServiceInstances.Clear();
            _HealthyServiceInstances.AddRange(
                _ServiceInstances.FindAll(
                    instance => instance.IsHealthy && !IsInGracePeriod(instance)
                ));     
        }

        _Logger.Information("Updated healthy service instances. Count: {0}", _HealthyServiceInstances.Count);
    }

    /// <inheritdoc cref="ILoadBalancer.GetInstance"/>
    public abstract IServiceInstance GetInstance();
}