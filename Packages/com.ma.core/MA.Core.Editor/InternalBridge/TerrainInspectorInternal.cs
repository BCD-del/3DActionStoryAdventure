// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using UnityEditor;

namespace MA.Core.Editor.Bridge
{
    class TerrainInspectorInternal : TerrainInspector
    {
        [NonSerialized] bool m_TempDisabled;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
        
        protected void SetDisabled(bool disabled)
        {
            if (m_TempDisabled != disabled)
            {
                m_TempDisabled = disabled;
                if (m_TempDisabled)
                    OnDisable();
                else
                    OnEnable();
            }
        }
    }
}