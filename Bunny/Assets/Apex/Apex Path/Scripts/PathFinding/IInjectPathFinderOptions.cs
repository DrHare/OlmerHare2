/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    /// <summary>
    /// Interface for components that are able to inject path finder options on a request
    /// </summary>
    public interface IInjectPathFinderOptions
    {
        /// <summary>
        /// Injects the path finder options.
        /// </summary>
        /// <param name="request">The request.</param>
        void InjectPathFinderOptions(IPathRequest request);
    }
}
