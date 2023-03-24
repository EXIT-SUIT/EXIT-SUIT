using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;

using UnityEngine;
using UnityEditor;


namespace UnityEditor.XR.Management
{
    internal class XRPackageInitializationSettings : ScriptableObject
    {
        private static XRPackageInitializationSettings s_PackageSettings = null;
        private static object s_Lock = new object();

        [SerializeField]
        private List<string> m_Settings = new List<string>();

        private XRPackageInitializationSettings(){ }

        internal static XRPackageInitializationSettings Instance
        {
            get
            {
                if (s_PackageSettings == null)
                {
                    lock(s_Lock)
                    {
                        if (s_PackageSettings == null)
                        {
                            s_PackageSettings = ScriptableObject.CreateInstance<XRPackageInitializationSettings>();
                            s_PackageSettings.LoadSettings();
                        }
                    }
                }
                return s_PackageSettings;
            }
        }

        internal void LoadSettings()
        {
            string packageInitPath = Path.Combine("ProjectSettings", "XRPackageSettings.asset");

            if (File.Exists(packageInitPath))
            {
                using (StreamReader sr = new StreamReader(packageInitPath))
                {
                    string settings = sr.ReadToEnd();
                    JsonUtility.FromJsonOverwrite(settings, this);
                }
            }
        }


        internal void SaveSettings()
        {
            string packageInitPath = Path.Combine("ProjectSettings", "XRPackageSettings.asset");
            using (StreamWriter sw = new StreamWriter(packageInitPath))
            {
                string settings = JsonUtility.ToJson(this, true);
                sw.Write(settings);
            }
        }

        internal bool HasSettings(string key)
        {
            return m_Settings.Contains(key);
        }

        internal void AddSettings(string key)
        {
            if (!HasSettings(key))
                m_Settings.Add(key);
        }
    }
}
