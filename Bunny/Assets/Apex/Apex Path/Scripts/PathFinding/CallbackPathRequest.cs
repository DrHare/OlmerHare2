﻿/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using System;
    using Apex.Common;
    using Apex.DataStructures;
    using Apex.LoadBalancing;
    using Apex.WorldGeometry;

    /// <summary>
    /// A path request that will do a callback with the result, rather than rely on the requester implementing <see cref="INeedPath"/>.
    /// </summary>
    public class CallbackPathRequest : PathRequestBase, IPathRequest, INeedPath
    {
        private Action<PathResult> _callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackPathRequest"/> class.
        /// </summary>
        /// <param name="requesterRadius">The requesting entity's radius.</param>
        /// <param name="callback">The callback to be called when the result is ready.</param>
        public CallbackPathRequest(float requesterRadius, Action<PathResult> callback)
            : this(requesterRadius, AttributeMask.None, callback)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackPathRequest"/> class.
        /// </summary>
        /// <param name="requesterRadius">The requesting entity's radius.</param>
        /// <param name="requesterAttributes">The requesting entity's attributes.</param>
        /// <param name="callback">The callback to be called when the result is ready.</param>
        public CallbackPathRequest(float requesterRadius, AttributeMask requesterAttributes, Action<PathResult> callback)
        {
            this.radius = requesterRadius;
            this.attributes = requesterAttributes;
            _callback = callback;
        }

        /// <summary>
        /// Gets the requester of this path, i.e. the entity that needs a path.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">This type of request cannot change the requester.</exception>
        public override INeedPath requester
        {
            get { return this; }

            set { throw new InvalidOperationException("This type of request cannot change the requester."); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool isValid
        {
            get { return ((this.fromGrid != null) && (this.toGrid != null)); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has decayed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has decayed; otherwise, <c>false</c>.
        /// </value>
        public bool hasDecayed
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the radius of the requesting entity.
        /// </summary>
        public float radius
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the attributes of the requesting entity.
        /// </summary>
        public AttributeMask attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Completes this request
        /// </summary>
        /// <param name="result">The result.</param>
        void IPathRequest.Complete(PathResult result)
        {
            this.requester.ConsumePathResult(result);
        }

        void INeedPath.ConsumePathResult(PathResult result)
        {
            LoadBalancer.marshaller.ExecuteOnMainThread(() => _callback(result));
        }
    }
}
