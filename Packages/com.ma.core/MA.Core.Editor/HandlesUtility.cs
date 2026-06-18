// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Collections.Generic;
using MA.Mathematics;
using Unity.Burst;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace MA.Core.Editor
{
    /// <summary> Utility class for drawing Unity Handles.</summary>
    [InitializeOnLoad]
    static class HandlesUtility
    {
        const string k_LineAATexPath = "Textures/LineAATex";
        static readonly Texture2D s_DenseLineAATex = Resources.Load<Texture2D>(k_LineAATexPath);
        static readonly Vector3[] s_AAWireDiscBuffer = new Vector3[32];
        
        class HandlesAction
        {
            public Action DrawFunc;
            public float Duration;
        }
        
        static readonly List<HandlesAction> s_DrawQueue = new List<HandlesAction>();
        static readonly UnityEngine.Pool.ObjectPool<HandlesAction> s_ActionPool = new UnityEngine.Pool.ObjectPool<HandlesAction>(() => new HandlesAction());

        static HandlesUtility()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            if (Event.current.type == EventType.Repaint)
            {
                for (int i = 0; i < s_DrawQueue.Count; i++)
                {
                    var gizmoAction = s_DrawQueue[i];
                    gizmoAction.DrawFunc();
                    
                    if (gizmoAction.Duration >= 0f)
                    {
                        gizmoAction.Duration -= Time.deltaTime;
                        if (gizmoAction.Duration <= 0f)
                        {
                            s_ActionPool.Release(gizmoAction);
                            s_DrawQueue.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }
        
        [BurstDiscard]
        public static void DrawDelayed(Action drawFunc, float duration = 1.0f)
        {
            var gizmoAction = s_ActionPool.Get();
            gizmoAction.DrawFunc = drawFunc;
            gizmoAction.Duration = duration;
            s_DrawQueue.Add(gizmoAction);
        }

        /// <summary>Draws a wire disc using the given position, normal, radius and thickness.</summary>
        public static void DrawAAWireDisc(Vector3 position, Vector3 normal, float radius, float thickness)
        {
            float3 p = position;
            float3 n = normal;
            n.CalculatePerpendicularAxes(out float3 right, out float3 _);

            float angleStep = 360f / (s_AAWireDiscBuffer.Length - 1);
            for (int i = 0; i < s_AAWireDiscBuffer.Length - 1; i++)
            {
                s_AAWireDiscBuffer[i] = p + right * radius;
                right = Quaternion.AngleAxis(angleStep, normal) * right;
            }

            s_AAWireDiscBuffer[^1] = s_AAWireDiscBuffer[0];

            Texture2D aaTex = thickness > 2f ? s_DenseLineAATex : null;
            Handles.DrawAAPolyLine(aaTex, thickness, s_AAWireDiscBuffer);
        }
    }
}
