// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using UnityEngine.LowLevel;

namespace MA.Core
{
    /// <summary>Provides utility functions for working with Unity's Player Loop System.</summary>
    public static class PlayerLoopUtility
    {
        /// <summary>Specifies where to add a loop in the subSystemList.</summary>
        public enum AddMode { Beginning, End }

        /// <summary>Tries to add an update function to a specific Unity internal update type.</summary>
        /// <param name="function">The custom update function to add.</param>
        /// <param name="ownerType">The owner type, mainly for debugging.</param>
        /// <param name="playerLoopSystemType">The type of the player loop system to add to.</param>
        /// <param name="addMode">Specifies where to add the function in the list.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public static bool TryAddToPlayerLoop(PlayerLoopSystem.UpdateFunction function, Type ownerType, Type playerLoopSystemType, AddMode addMode)
        {
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            bool success = TryAddToPlayerLoop(function, ownerType, ref playerLoop, playerLoopSystemType, addMode);
            PlayerLoop.SetPlayerLoop(playerLoop);
            return success;
        }

        /// <summary>Tries to add an update function to a specific Unity internal update type.</summary>
        /// <param name="function">The custom update function to add.</param>
        /// <param name="ownerType">The owner type, mainly for debugging.</param>
        /// <param name="playerLoop">The player loop to modify.</param>
        /// <param name="playerLoopSystemType">The type of the player loop system to add to.</param>
        /// <param name="addMode">Specifies where to add the function in the list.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public static bool TryAddToPlayerLoop(PlayerLoopSystem.UpdateFunction function, Type ownerType, ref PlayerLoopSystem playerLoop, Type playerLoopSystemType, AddMode addMode)
        {
            if (playerLoop.type == playerLoopSystemType)
            {
                int oldListLength = playerLoop.subSystemList?.Length ?? 0;
                Array.Resize(ref playerLoop.subSystemList, oldListLength + 1);

                PlayerLoopSystem system = new PlayerLoopSystem {
                    type = ownerType,
                    updateDelegate = function
                };

                if (addMode == AddMode.Beginning)
                {
                    Array.Copy(playerLoop.subSystemList, 0, playerLoop.subSystemList, 1, playerLoop.subSystemList.Length - 1);
                    playerLoop.subSystemList[0] = system;
                }
                else if (addMode == AddMode.End)
                {
                    playerLoop.subSystemList[oldListLength] = system;
                }

                return true;
            }

            if (playerLoop.subSystemList != null)
            {
                for(int i = 0; i < playerLoop.subSystemList.Length; ++i)
                {
                    if (TryAddToPlayerLoop(function, ownerType, ref playerLoop.subSystemList[i], playerLoopSystemType, addMode))
                        return true;
                }
            }
            
            return false;
        }

        /// <summary>Tries to remove a loop system of a specific type from the parent loop system.</summary>
        /// <param name="childSystemType">The type of the child system to remove.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public static bool TryRemoveLoopSystem(Type childSystemType)
        {
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            bool success = TryRemoveLoopSystem(ref playerLoop, childSystemType);
            PlayerLoop.SetPlayerLoop(playerLoop);
            return success;
        }

        /// <summary>Tries to remove a loop system of a specific type from the parent loop system.</summary>
        /// <param name="parentLoopSystem">The parent loop system to modify.</param>
        /// <param name="childSystemType">The type of the child system to remove.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public static bool TryRemoveLoopSystem(ref PlayerLoopSystem parentLoopSystem, Type childSystemType)
        {
            if (parentLoopSystem.subSystemList == null) 
                return false;

            int systemPosition = FindSystemPosition(parentLoopSystem.subSystemList, childSystemType);
            if (systemPosition != -1)
            {
                RemoveSystemAt(ref parentLoopSystem, systemPosition);
                return true;
            }
                
            for (int i = 0; i < parentLoopSystem.subSystemList.Length; ++i)
            {
                if (TryRemoveLoopSystem(ref parentLoopSystem.subSystemList[i], childSystemType))
                    return true;
            }

            return false;
        }
        
        /// <summary>Finds the index position of the specified system type in a subsystem list.</summary>
        /// <param name="subSystemList">The list of subsystems to search.</param>
        /// <param name="systemType">The type of system to find.</param>
        /// <returns>The index position if found; otherwise, -1.</returns>
        static int FindSystemPosition(PlayerLoopSystem[] subSystemList, Type systemType)
        {
            for (int i = 0; i < subSystemList.Length; i++)
            {
                if (subSystemList[i].type == systemType)
                    return i;
            }
            
            return -1;
        }
        
        /// <summary>Removes the system at the specified index position.</summary>
        /// <param name="parentLoopSystem">Reference to the parent loop system.</param>
        /// <param name="systemPosition">Index position of the system to remove.</param>
        static void RemoveSystemAt(ref PlayerLoopSystem parentLoopSystem, int systemPosition)
        {
            PlayerLoopSystem[] newSubsystemList = new PlayerLoopSystem[parentLoopSystem.subSystemList.Length - 1];
    
            if (systemPosition > 0)
                Array.Copy(parentLoopSystem.subSystemList, newSubsystemList, systemPosition);

            if (systemPosition < parentLoopSystem.subSystemList.Length - 1)
                Array.Copy(parentLoopSystem.subSystemList, systemPosition + 1, newSubsystemList, systemPosition, parentLoopSystem.subSystemList.Length - systemPosition - 1);

            parentLoopSystem.subSystemList = newSubsystemList;
        }
    }
}
