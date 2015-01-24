/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using Apex.DataStructures;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Represents a path along which a unit can move.
    /// </summary>
    public class Path : StackWithLookAhead<IPositioned>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        public Path()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public Path(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="path">The path points.</param>
        public Path(params Vector3[] path)
            : base(path.Length)
        {
            Ensure.ArgumentNotNull(path, "path");

            for (int i = path.Length - 1; i >= 0; i--)
            {
                Push(path[i].AsPositioned());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="path">The path points.</param>
        public Path(IIndexable<Vector3> path)
            : base(path.count)
        {
            Ensure.ArgumentNotNull(path, "path");

            for (int i = path.count - 1; i >= 0; i--)
            {
                Push(path[i].AsPositioned());
            }
        }

        /// <summary>
        /// Updates the path.
        /// </summary>
        /// <param name="path">The path points.</param>
        public void Update(params Vector3[] path)
        {
            Clear();

            for (int i = path.Length - 1; i >= 0; i--)
            {
                Push(path[i].AsPositioned());
            }
        }

        /// <summary>
        /// Updates the path.
        /// </summary>
        /// <param name="path">The path points.</param>
        public void Update(IIndexable<Vector3> path)
        {
            Clear();

            for (int i = path.count - 1; i >= 0; i--)
            {
                Push(path[i].AsPositioned());
            }
        }
    }
}
