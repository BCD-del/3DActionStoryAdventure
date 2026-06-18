// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using MA.Core.Bridge;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MA.Core
{
    public static class UnityExtensions
    {
        /// <summary>Returns the internal GUID of the scene.</summary>
        /// <param name="scene">The scene.</param>
        /// <returns>The internal GUID of the scene.</returns>
        public static string GetInternalGuid(this Scene scene) 
            => SceneBridge.GetGuid(scene);
        
        /// <summary>Converts a `Vector2Int` to an `int2`.</summary>
        /// <param name="v">The vector.</param>
        /// <returns>The converted vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 AsInt2(this Vector2Int v) 
            => new int2(v.x, v.y);
        
        /// <summary>Returns a color with the given alpha.</summary>
        /// <param name="color">The color.</param>
        /// <param name="alpha">The alpha value.</param>
        /// <returns>The color with the given alpha.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color WithAlpha(this Color color, float alpha) 
            => new Color(color.r, color.g, color.b, alpha);
        
        /// <summary>Returns a color with the given alpha.</summary>
        /// <param name="color">The color.</param>
        /// <param name="alpha">The alpha value.</param>
        /// <returns>The color with its alpha multiplied by the given value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color WithMultipliedAlpha(this Color color, float alpha) 
            => new Color(color.r, color.g, color.b, color.a * alpha);

        /// <summary>Tries to get a component in the parent or ancestor of the given component.</summary>
        /// <param name="component">The component.</param>
        /// <param name="result">The component to get.</param>
        /// <returns>True if the component was found, otherwise false.</returns>
        /// <typeparam name="U">The type of the component.</typeparam>
        /// <typeparam name="T">The type of the component to get.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetComponentInParent<U, T>(this U component, out T result)
            where U : Component
            where T : Component
        {
            result = null;
            if (!component) 
                return false;
            
            if (component.TryGetComponent(out result))
                return true;

            result = component.GetComponentInParent<T>();
            return result;
        }

        /// <summary>Tries to get a component in the parent or ancestor of the given GameObject.</summary>
        /// <param name="gameObject">The GameObject.</param>
        /// <param name="result">The component to get.</param>
        /// <returns>True if the component was found, otherwise false.</returns>
        /// <typeparam name="T">The type of the component to get.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T result)
            where T : Component
        {
            result = null;
            if (!gameObject) 
                return false;
            
            if (gameObject.TryGetComponent(out result))
                return true;

            result = result.GetComponentInParent<T>();
            return result;
        }
    }
}
