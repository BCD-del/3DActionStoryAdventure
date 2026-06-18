using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace MA.Mathematics
{
    /// <summary>Represents a 3D cell coordinate.</summary>
    [Serializable]
    public struct CellCoordinate
        : IEquatable<CellCoordinate>
        , IComparable<CellCoordinate>
    {
        /// <summary>The x coordinate.</summary>
        public int X;
        /// <summary>The y coordinate.</summary>
        public int Y;
        /// <summary>The z coordinate.</summary>
        public int Z;
        /// <summary>The maximum dimension of the cell coordinate.</summary>
        public int MaxDimension
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => math.max(X, math.max(Y, Z));
        }

        /// <summary>Constructs a new cell coordinate.</summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="z">The z-coordinate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CellCoordinate(int x, int y, int z) => (X, Y, Z) = (x, y, z);

        /// <summary>Constructs a new cell coordinate.</summary>
        /// <param name="xyz">A 3D integer vector representing the coordinates.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CellCoordinate(int3 xyz) => (X, Y, Z) = (xyz.x, xyz.y, xyz.z);
        
        /// <summary>Creates a new cell coordinate that encodes the given world position using the given cell size.</summary>
        /// <param name="position">The world position to encode.</param>
        /// <param name="cellSize">The size of the cell.</param>
        /// <returns>A new <see ref="CellCoordinate"/> representing the cell at the given world position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CellCoordinate FromPosition(float3 position, int cellSize) 
            => new((int)math.floor(position.x / cellSize), 
                   (int)math.floor(position.y / cellSize),
                   (int)math.floor(position.z / cellSize));

        /// <summary>Returns the minimum point of a cell coordinate.</summary>
        /// <param name="cell">The cell coordinate.</param>
        /// <param name="cellSize">The size of the cell.</param>
        /// <returns>A 3D float vector representing the minimum point of the cell coordinate.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetMin(in CellCoordinate cell, int cellSize) 
            => new(cell.X * cellSize, 
                   cell.Y * cellSize,
                   cell.Z * cellSize);

        /// <summary>Returns the maximum point of a cell coordinate.</summary>
        /// <param name="cell">The cell coordinate.</param>
        /// <param name="cellSize">The size of the cell.</param>
        /// <returns>A 3D float vector representing the maximum point of the cell coordinate.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetMax(in CellCoordinate cell, int cellSize) 
            => new(cell.X * cellSize + cellSize, 
                   cell.Y * cellSize + cellSize, 
                   cell.Z * cellSize + cellSize);

        /// <summary>Creates a bounding box that encloses a range of cell coordinates.</summary>
        /// <param name="cell">The cell coordinate.</param>
        /// <param name="cellSize">The size of the cell.</param>
        /// <returns>An AxisAlignedBox that encloses the range of cell coordinates.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetCenter(in CellCoordinate cell, int cellSize) 
            => (GetMin(cell, cellSize) + GetMax(cell, cellSize)) * 0.5f;

        /// <summary>Creates a bounding box that encloses the cell.</summary>
        /// <param name="cell">The cell coordinate.</param>
        /// <param name="cellSize">The size of the cell.</param>
        /// <returns>An AxisAlignedBox that encloses the cell.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AxisAlignedBox GetCellBounds(in CellCoordinate cell, int cellSize)
        {
            float3 min = GetMin(cell, cellSize);
            float3 max = GetMax(cell, cellSize);
            return new AxisAlignedBox(min, max);
        }

        /// <summary>Creates a bounding box that encloses a range of cell coordinates.</summary>
        /// <param name="gridMin">The minimum cell coordinate.</param>
        /// <param name="gridMax">The maximum cell coordinate.</param>
        /// <param name="cellSize">The size of the cell.</param>
        /// <returns>An AxisAlignedBox that encloses the range of cell coordinates.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AxisAlignedBox GetGridBounds(in CellCoordinate gridMin, in CellCoordinate gridMax, int cellSize)
        {
            AxisAlignedBox minBounds = GetCellBounds(gridMin, cellSize);
            AxisAlignedBox maxBounds = GetCellBounds(gridMax, cellSize);
            return minBounds + maxBounds;
        }

        /// <summary>Returns the minimum cell coordinate of a pair of cell coordinates.</summary>
        /// <param name="a">The first cell coordinate.</param>
        /// <param name="b">The second cell coordinate.</param>
        /// <returns>The minimum cell coordinate of the two input cell coordinates.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CellCoordinate Min(in CellCoordinate a, in CellCoordinate b) => new CellCoordinate(math.min(a, b));

        /// <summary>Returns the maximum cell coordinate of a pair of cell coordinates.</summary>
        /// <param name="a">The first cell coordinate.</param>
        /// <param name="b">The second cell coordinate.</param>
        /// <returns>The maximum cell coordinate of the two input cell coordinates.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CellCoordinate Max(in CellCoordinate a, in CellCoordinate b) => new CellCoordinate(math.max(a, b));

        /// <summary>Returns true if `rhs` is equal to this cell coordinate.</summary>
        /// <param name="rhs">The cell coordinate to compare with.</param>
        /// <returns>True if the two cell coordinates are equal, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(CellCoordinate rhs) => X == rhs.X && Y == rhs.Y && Z == rhs.Z;

        /// <summary>Returns true if the given object is a cell coordinate and is equal to this cell coordinate.</summary>
        /// <param name="o">The object to compare with.</param>
        /// <returns>True if the object is a <see ref="CellCoordinate"/> and is equal to this cell coordinate, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object o) => o is CellCoordinate converted && Equals(converted);

        /// <summary>Returns a hash code for this cell coordinate.</summary>
        /// <returns>An integer hash code for this cell coordinate.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => (int)unchecked((X * 0x4C7F6DD1u + Y * 0x4822A3E9u + Z * 0xAAC3C25Du) + 0xD21D0945u);

        /// <summary>Compares this cell coordinate to another cell coordinate.</summary>
        /// <param name="other">The cell coordinate to compare with.</param>
        /// <returns>An integer that indicates the relative order of the two cell coordinates.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(CellCoordinate other)
        {
            int xCompare = X.CompareTo(other.X);
            if (xCompare != 0) return xCompare;
            int zCompare = Z.CompareTo(other.Z);
            if (zCompare != 0) return zCompare;
            return Y.CompareTo(other.Y);
        }
        
        /// <summary>Returns a string representation of this cell coordinate.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"CellCoordinate({X}, {Y}, {Z})";

        /// <summary>Converts this cell coordinate to a 3D integer vector.</summary>
        /// <returns>A 3D integer vector representing this cell coordinate.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int3(in CellCoordinate to) => new int3(to.X, to.Y, to.Z);

        /// <summary>Converts a 3D integer vector to a <see ref="CellCoordinate"/>.</summary>
        /// <param name="to">The 3D integer vector to convert.</param>
        /// <returns>A <see ref="CellCoordinate"/> representing the 3D integer vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator CellCoordinate(in int3 to) => new CellCoordinate(to.x, to.y, to.z);
        
        /// <summary>Adds two cell coordinates together.</summary>
        /// <param name="a">The first cell coordinate to add.</param>
        /// <param name="b">The second cell coordinate to add.</param>
        /// <returns>The sum of the two cell coordinates.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CellCoordinate operator +(in CellCoordinate a, in CellCoordinate b) => new CellCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        
        /// <summary>Subtracts one cell coordinate from another.</summary>
        /// <param name="a">The cell coordinate to subtract from.</param>
        /// <param name="b">The cell coordinate to subtract.</param>
        /// <returns>The difference of the two cell coordinates.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CellCoordinate operator -(in CellCoordinate a, in CellCoordinate b) => new CellCoordinate(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        /// <summary>Returns true if the given cell coordinates are equal.</summary>
        /// <param name="lhs">The first cell coordinate to compare.</param>
        /// <param name="rhs">The second cell coordinate to compare.</param>
        /// <returns>True if the two cell coordinates are equal, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in CellCoordinate lhs, in CellCoordinate rhs) => lhs.Equals(rhs);

        /// <summary>Returns true if the given cell coordinates are not equal.</summary>
        /// <param name="lhs">The first cell coordinate to compare.</param>
        /// <param name="rhs">The second cell coordinate to compare.</param>
        /// <returns>True if the two cell coordinates are not equal, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in CellCoordinate lhs, in CellCoordinate rhs) => !lhs.Equals(rhs);

        /// <summary>Returns true if the `lhs` cell coordinate is less than the `rhs` cell coordinate.</summary>
        /// <param name="lhs">The left-hand side cell coordinate.</param>
        /// <param name="rhs">The right-hand side cell coordinate.</param>
        /// <returns>True if the left-hand side cell coordinate is less than the right-hand side cell coordinate, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(in CellCoordinate lhs, in CellCoordinate rhs) => lhs.X < rhs.X && lhs.Y < rhs.Y && lhs.Z < rhs.Z;
        
        /// <summary>Returns true if the `lhs` cell coordinate is less than or equal to the `rhs` cell coordinate.</summary>
        /// <param name="lhs">The left-hand side cell coordinate.</param>
        /// <param name="rhs">The right-hand side cell coordinate.</param>
        /// <returns>True if the left-hand side cell coordinate is less than or equal to the right-hand side cell coordinate, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(in CellCoordinate lhs, in CellCoordinate rhs) => lhs.X <= rhs.X && lhs.Y <= rhs.Y && lhs.Z <= rhs.Z;

        /// <summary>Returns true if the `lhs` cell coordinate is greater than the `rhs` cell coordinate.</summary>
        /// <param name="lhs">The left-hand side cell coordinate.</param>
        /// <param name="rhs">The right-hand side cell coordinate.</param>
        /// <returns>True if the left-hand side cell coordinate is greater than the right-hand side cell coordinate, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(in CellCoordinate lhs, in CellCoordinate rhs) => lhs.X > rhs.X && lhs.Y > rhs.Y && lhs.Z > rhs.Z;

        /// <summary>Returns true if the `lhs` cell coordinate is greater than or equal to the `rhs` cell coordinate.</summary>
        /// <param name="lhs">The left-hand side cell coordinate.</param>
        /// <param name="rhs">The right-hand side cell coordinate.</param>
        /// <returns>True if the left-hand side cell coordinate is greater than or equal to the right-hand side cell coordinate, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(in CellCoordinate lhs, in CellCoordinate rhs) => lhs.X >= rhs.X && lhs.Y >= rhs.Y && lhs.Z >= rhs.Z;
    }
}
