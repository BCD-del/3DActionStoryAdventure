// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

namespace MA.Core.Bridge
{
    static class SceneBridge
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetGuid(Scene scene) => scene.guid;
    }
}
