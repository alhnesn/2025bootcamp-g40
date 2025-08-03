using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections; // Needed for IEnumerator
using System.Collections.Generic; // Needed for List
using Unity.EditorCoroutines.Editor; // For EditorCoroutineUtility


public class IngredientThumbnailGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Ingredient Thumbnails")]
    public static void ShowWindow()
    {
        GetWindow<IngredientThumbnailGenerator>("Ingredient Thumbnails");
    }

    private IngredientDatabase database;
    private bool isGeneratingThumbnails = false;

    void OnGUI()
    {
        GUILayout.Label("Ingredient Thumbnail Generator", EditorStyles.boldLabel);

        database = (IngredientDatabase)EditorGUILayout.ObjectField("Ingredient Database", database, typeof(IngredientDatabase), false);

        if (database == null)
        {
            EditorGUILayout.HelpBox("Please assign an Ingredient Database", MessageType.Warning);
            return;
        }

        // Disable the button while generation is in progress
        GUI.enabled = !isGeneratingThumbnails;
        if (GUILayout.Button("Generate All Thumbnails"))
        {
            isGeneratingThumbnails = true;
            EditorCoroutineUtility.StartCoroutine(GenerateThumbnailsRoutine(), this); // Start as a coroutine
        }
        GUI.enabled = true; // Re-enable GUI elements

        if (isGeneratingThumbnails)
        {
            EditorGUILayout.HelpBox("Generating thumbnails... Please wait.", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This will:\n1. Generate thumbnails for all ingredient prefabs\n2. Save them as PNG files\n3. Update the database with sprite references", MessageType.Info);
    }

    // Use a Coroutine for asynchronous thumbnail generation
    private IEnumerator GenerateThumbnailsRoutine()
    {
        string folderPath = "Assets/UI/IngredientThumbnails";

        // Create folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/UI", "IngredientThumbnails");
        }

        // Get all prefabs to process
        List<GameObject> prefabsToProcess = new List<GameObject>(database.ingredientPrefabs);
        int processedCount = 0;

        // Ensure all objects are fully loaded if not already
        foreach (GameObject prefab in prefabsToProcess)
        {
             if (prefab != null)
             {
                 AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(prefab));
             }
        }
        
        // This is a crucial step for editor coroutines.
        // It tells the editor to keep redrawing and processing events.
        EditorApplication.QueuePlayerLoopUpdate();

        foreach (GameObject prefab in prefabsToProcess)
        {
            if (prefab == null) continue;

            Ingredient ingredient = prefab.GetComponent<Ingredient>();
            if (ingredient == null)
            {
                Debug.LogWarning($"Prefab {prefab.name} does not have an Ingredient component.");
                continue;
            }

            Texture2D thumbnail = null;
            int maxAttempts = 100; // Prevent infinite loop, adjust as needed
            int attempt = 0;

            // Poll for the thumbnail to become available
            while (thumbnail == null && attempt < maxAttempts)
            {
                thumbnail = AssetPreview.GetAssetPreview(prefab);
                if (thumbnail == null)
                {
                    // Yield to allow Unity to generate the preview
                    yield return null; // Wait for the next editor frame
                    attempt++;
                }
            }

            if (thumbnail == null)
            {
                Debug.LogWarning($"Failed to generate thumbnail for {prefab.name} after {maxAttempts} attempts. Skipping.");
                continue; // Skip this prefab if thumbnail still not available
            }

            // Create a new readable Texture2D
            Texture2D readableTexture = new Texture2D(thumbnail.width, thumbnail.height, thumbnail.format, false);
            Graphics.CopyTexture(thumbnail, readableTexture);

            // Save as PNG
            string fileName = $"{ingredient.ingredientName}_thumbnail.png";
            string filePath = Path.Combine(folderPath, fileName);

            byte[] pngData = readableTexture.EncodeToPNG();
            File.WriteAllBytes(filePath, pngData);

            Debug.Log($"Generated thumbnail for {ingredient.ingredientName}");
            processedCount++;
            EditorUtility.DisplayProgressBar("Generating Thumbnails", $"Processing {ingredient.ingredientName} ({processedCount}/{prefabsToProcess.Count})", (float)processedCount / prefabsToProcess.Count);
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar(); // Clear progress bar

        isGeneratingThumbnails = false; // Reset flag when done
        Debug.Log("All thumbnails generated and assigned!");
    }
}