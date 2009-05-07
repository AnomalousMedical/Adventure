﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public struct Ray3
    {
        public Vector3 Origin;
        public Vector3 Direction;

        /// <summary>
	    /// Constructor.
	    /// </summary>
	    /// <param name="origin">The origin of the ray.</param>
	    /// <param name="direction">The direction the ray is facing.</param>
        public Ray3(Vector3 origin, Vector3 direction)
        {
            this.Origin = origin;
            this.Direction = direction;
        }

        /// <summary>
	    /// Get the point at the specified distance along the ray.
	    /// </summary>
	    /// <param name="distance">The distance along the ray.</param>
	    /// <returns>The point at the given distance.</returns>
        public Vector3 getPoint(float distance)
        {
            return Origin + Direction * distance;
        }

        /// <summary>
        /// Determine if all components are numbers i.e != float::NaN.
        /// </summary>
        /// <returns>True if all components are != float::NaN</returns>
        public bool isNumber()
        {
            return Origin.isNumber() && Direction.isNumber();
        }

        /// <summary>
        /// ToString function.
        /// </summary>
        /// <returns>The ray as a string.</returns>
        public override String ToString()
        {
            return "{" + Origin.ToString() + ", " + Direction.ToString() + "}";
        }
    }
}
