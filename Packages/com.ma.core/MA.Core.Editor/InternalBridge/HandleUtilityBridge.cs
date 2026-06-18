// Copyright © Magnetic Arcade. All Rights Reserved.

using MA.Core.Bridge;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MA.Core.Editor.Bridge
{
    static class HandleUtilityBridge
    {
        static RaycastHit[] s_RaySnapHits = new RaycastHit[100];
        
        // Objects to ignore when raysnapping (typically the objects being dragged by the handles)
        public static Transform[] IgnoreRaySnapObjects = null;
        
        public static bool TryPlaceOnGrid(Ray ray, out RaycastHit resultHit)
        {
            resultHit = default;
            resultHit.distance = Mathf.Infinity;
            resultHit.normal = Vector3.up;

            if (!TryGetPlane(out Plane targetPlane))
                return false;

            if (targetPlane.Raycast(ray, out float hitDistance))
            {
                resultHit.distance = hitDistance;
                resultHit.normal = targetPlane.normal;
                return true;
            }

            return false;
        }
        
        static bool TryGetPlane(out Plane plane)
        {
            plane = new Plane();

            SceneView sceneView = SceneView.lastActiveSceneView;
            Transform cameraTransform = sceneView.camera.transform;
            if (SceneView.lastActiveSceneView.showGrid)
            {
                SceneViewGrid.GridRenderAxis axis = sceneView.sceneViewGrids.gridAxis;
                Vector3 point = sceneView.sceneViewGrids.GetPivot(axis);

                Vector3 normal = sceneView.in2DMode ?
                    Vector3.forward :
                    new Vector3(
                        axis == SceneViewGrid.GridRenderAxis.X ? 1 : 0,
                        axis == SceneViewGrid.GridRenderAxis.Y ? 1 : 0,
                        axis == SceneViewGrid.GridRenderAxis.Z ? 1 : 0
                    );

                //Invert normal if camera is facing the other side of the plane
                if (Vector3.Dot(cameraTransform.forward, normal) > 0)
                    normal *= -1f;

                plane = new Plane(normal, point);

                //If the camera if on the right side of the plane, return this plane
                if (plane.GetSide(cameraTransform.position))
                    return true;
            }

            return false;
        }
        
        // Casts /ray/ against the scene.
        public static object RaySnap(Ray ray, int layerMask)
        {
            Camera cam = Camera.current;

            if (cam == null)
                return null;

            ulong sceneCullingMask = cam.GetSceneCullingMask();

            bool hitAny = false;
            RaycastHit raycastHit = default(RaycastHit);
            raycastHit.distance = Mathf.Infinity;

            if (sceneCullingMask == SceneCullingMasks.MainStageSceneViewObjects)
            {
                // Default code path for Scene view that is just displaying the Main Stage.
                // Note that even if Prefab Mode is open, special Scene views can still show the Main Stage!
                // We only check against default physics scene here, and shouldn't ignore Prefab instances
                // that are opened in Prefab Mode in Context.
                hitAny |= GetNearestHitFromPhysicsScene(ray, Physics.defaultPhysicsScene, layerMask, false, ref raycastHit);
            }
            else
            {
                // Code path is Scene view is displaying a Prefab Stage.
                // Here we dig down from the top of the stage history stack and continue
                // including each stage as long as they are displayed as context. Prefab instances
                // that are hidden due to being opened in Prefab Mode in Context should be ignored.
                var stageHistory = StageNavigationManager.instance.stageHistory;
                for (int i = stageHistory.Count - 1; i >= 0; i--)
                {
                    Stage stage = stageHistory[i];
                    var previewSceneStage = stage as PreviewSceneStage;
                    PhysicsScene physics = previewSceneStage != null ? previewSceneStage.scene.GetPhysicsScene() : Physics.defaultPhysicsScene;
                    hitAny |= GetNearestHitFromPhysicsScene(ray, physics, layerMask, true, ref raycastHit);
                    var prefabStage = previewSceneStage as PrefabStage;
                    if (prefabStage == null ||
                        prefabStage.mode == PrefabStage.Mode.InIsolation ||
                        StageNavigationManager.instance.contextRenderMode == StageUtility.ContextRenderMode.Hidden)
                        break;
                }
            }

            if (hitAny)
                return raycastHit;
            return null;
        }

        static bool GetNearestHitFromPhysicsScene(Ray ray, PhysicsScene physicsScene, int layerMask, bool ignorePrefabInstance, ref RaycastHit raycastHit)
        {
            float maxDist = raycastHit.distance;
            int numHits = physicsScene.Raycast(ray.origin, ray.direction, s_RaySnapHits, maxDist, layerMask, QueryTriggerInteraction.Ignore);

            // We are not sure at this point if the hits returned from RaycastAll are sorted or not, so go through them all
            float nearestHitDist = maxDist;
            int nearestHitIndex = -1;
            if (IgnoreRaySnapObjects != null)
            {
                for (int i = 0; i < numHits; i++)
                {
                    if (s_RaySnapHits[i].distance < nearestHitDist)
                    {
                        Transform tr = s_RaySnapHits[i].transform;
                        if (SceneVisibilityManager.instance.IsHidden(tr.gameObject))
                            continue;

                        if (ignorePrefabInstance && GameObjectUtility.IsPrefabInstanceHiddenForInContextEditing(tr.gameObject))
                            continue;

                        bool ignore = false;
                        for (int j = 0; j < IgnoreRaySnapObjects.Length; j++)
                        {
                            if (tr == IgnoreRaySnapObjects[j])
                            {
                                ignore = true;
                                break;
                            }
                        }
                        if (ignore)
                            continue;

                        nearestHitDist = s_RaySnapHits[i].distance;
                        nearestHitIndex = i;
                    }
                }
            }
            else
            {
                for (int i = 0; i < numHits; i++)
                {
                    RaycastHit raySnapHit = s_RaySnapHits[i];
                    if (SceneVisibilityManager.instance.IsHidden(raySnapHit.transform.gameObject))
                        continue;

                    if (raySnapHit.distance < nearestHitDist)
                    {
                        nearestHitDist = s_RaySnapHits[i].distance;
                        nearestHitIndex = i;
                    }
                }
            }

            if (nearestHitIndex >= 0)
            {
                raycastHit = s_RaySnapHits[nearestHitIndex];
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}