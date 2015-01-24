namespace Apex.WorldGeometry
{
    using UnityEngine;

    /// <summary>
    /// Configuration settings for grids created at runtime using <see cref="GridComponent.Create(UnityEngine.GameObject, GridConfig)"/>
    /// </summary>
    public sealed class GridConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GridConfig"/> class.
        /// </summary>
        public GridConfig()
        {
            this.sizeX = 10;
            this.sizeZ = 10;
            this.cellSize = 2f;
            this.generateHeightmap = true;
            this.granularity = 0.1f;
            this.lowerBoundary = 1f;
            this.upperBoundary = 10f;
            this.maxScaleHeight = 0.5f;
            this.maxWalkableSlopeAngle = 30f;
            this.obstacleSensitivityRange = 0.5f;
            this.subSectionsCellOverlap = 2;
            this.subSectionsX = 2;
            this.subSectionsZ = 2;
        }

        /// <summary>
        /// Gets or sets the friendly name of the grid. Used in messages and such.
        /// </summary>
        public string friendlyName { get; set; }

        /// <summary>
        /// The origin, i.e. center of the grid
        /// </summary>
        public Vector3 origin { get; set; }

        /// <summary>
        /// size along the x-axis.
        /// </summary>
        public int sizeX { get; set; }

        /// <summary>
        /// size along the z-axis.
        /// </summary>
        public int sizeZ { get; set; }

        /// <summary>
        /// The cell size.
        /// </summary>
        public float cellSize { get; set; }

        /// <summary>
        /// The obstacle sensitivity range, meaning any obstacle within this range of the cell center will cause the cell to be blocked.
        /// </summary>
        public float obstacleSensitivityRange { get; set; }

        /// <summary>
        /// Whether or not to generate a height map to enable units to follow a terrain of differing heights.
        /// </summary>
        public bool generateHeightmap { get; set; }

        /// <summary>
        /// The upper boundary (y - value) of the matrix.
        /// </summary>
        public float upperBoundary { get; set; }

        /// <summary>
        /// The lower boundary (y - value) of the matrix.
        /// </summary>
        public float lowerBoundary { get; set; }

        /// <summary>
        /// Gets the granularity of the height map, i.e. the distance between height samples.
        /// </summary>
        /// <value>
        /// The granularity of the height map.
        /// </value>
        public float granularity { get; set; }

        /// <summary>
        /// The sub sections along the x-axis.
        /// </summary>
        public int subSectionsX { get; set; }

        /// <summary>
        /// The sub sections along the z-axis.
        /// </summary>
        public int subSectionsZ { get; set; }

        /// <summary>
        /// The sub sections cell overlap
        /// </summary>
        public int subSectionsCellOverlap { get; set; }

        /// <summary>
        /// The maximum angle at which a cell is deemed walkable
        /// </summary>
        public float maxWalkableSlopeAngle { get; set; }

        /// <summary>
        /// The maximum height that the unit can scale, i.e. walk onto even if its is a vertical move. Stairs for instance.
        /// </summary>
        public float maxScaleHeight { get; set; }
    }
}
