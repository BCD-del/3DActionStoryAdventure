// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEngine;

namespace MA.Core.Bridge
{
    static class TerrainDataBridge
    {
        internal static void RemoveTreePrototypeBridged(this TerrainData terrainData, int index)
            => terrainData.RemoveTreePrototype(index);

        internal static void RemoveDetailPrototypeBridged(this TerrainData terrainData, int index)
            => terrainData.RemoveDetailPrototype(index);
    }
}
