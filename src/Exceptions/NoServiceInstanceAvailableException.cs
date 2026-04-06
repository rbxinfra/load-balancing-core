namespace Roblox.LoadBalancing.Exceptions;

using System;

/// <summary>
/// Exception thrown when there are no healthy service instances available to receive traffic from the load balancer
/// </summary>
public class NoServiceInstanceAvailableException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoServiceInstanceAvailableException"/> class with a default error message.
    /// </summary>
    public NoServiceInstanceAvailableException()
        : base("No service instance available for serving request")
    {
    }
}