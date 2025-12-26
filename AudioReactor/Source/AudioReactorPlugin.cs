#if FLAX_EDITOR
using System;
using System.IO;
using FlaxEngine;
using FlaxEditor;
using FlaxEditor.Content;
using FlaxEditor.GUI;

namespace AudioReactor
{
    public class AudioReactorPlugin : EditorPlugin
    {
        // Settings
        private const string DataFolderName = "MusicData";
        private string _safeHousePath;

        public override void InitializeEditor()
        {
            base.InitializeEditor();

            // 1. Setup path: ProjectRoot/MusicData
            _safeHousePath = Path.Combine(Globals.ProjectFolder, DataFolderName);

            // 2. Auto-Create Folder on Load
            if (!Directory.Exists(_safeHousePath))
            {
                Directory.CreateDirectory(_safeHousePath);
                Debug.Log("🎹 AudioReactor: Created 'MusicData' safe house folder.");
            }

            // 3. Hook into the Import System
            Editor.Instance.ContentImporting.ImportFile += OnImportFile;

            // 4. Add a Menu Button to view files
            Editor.Instance.UI.MenuTools.ContextMenu.AddButton("Open MusicData Folder", () => 
            {
                System.Diagnostics.Process.Start("explorer.exe", _safeHousePath);
            });
        }

        public override void DeinitializeEditor()
        {
            // Clean up hooks when plugin turns off
            Editor.Instance.ContentImporting.ImportFile -= OnImportFile;
            base.DeinitializeEditor();
        }

        private void OnImportFile(IFileEntryAction entry, ref bool cancel)
        {
            // We only care about WAV files
            if (!entry.SourceUrl.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) return;

            try
            {
                // Ensure folder exists (just in case)
                if (!Directory.Exists(_safeHousePath)) Directory.CreateDirectory(_safeHousePath);

                string fileName = Path.GetFileName(entry.SourceUrl);
                string destPath = Path.Combine(_safeHousePath, fileName);

                // Check if it's already there to avoid unnecessary work
                if (!File.Exists(destPath))
                {
                    File.Copy(entry.SourceUrl, destPath);
                    Debug.Log($"🕵️ AudioReactor: Intercepted '{fileName}' and copied to Safe House!");
                }
                else
                {
                    Debug.Log($"✅ AudioReactor: '{fileName}' is already in the Safe House.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("❌ AudioReactor Failed to copy wav: " + ex.Message);
            }
            
            // We DO NOT cancel the import. 
            // We let Flax proceed to create the .flax asset so the AudioSource can use it!
        }
    }
}
#endif