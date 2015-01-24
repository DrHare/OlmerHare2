/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using System;
    using Apex.DataStructures;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Interface for objects that steer along a path
    /// </summary>
    [Obsolete("Use IMovable instead, all properties have been relocated to that interface.")]
    public interface ISteerable : IMovable
    {
    }
}
