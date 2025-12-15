using Newtonsoft.Json;
using Playbox.Purchases;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PurchaseProductEditor : EditorWindow
{
    private const string FolderPath = "Assets/Resources/Playbox/IAP";
    private const string FileName = "Products.json";
    private string _filePath => Path.Combine(FolderPath, FileName);

    private List<ProductJson> _products = new List<ProductJson>();
    private Vector2 _scrollPos;

    private bool _isDirty = false; 

    [MenuItem("Playbox/Purchase Products Editor")]
    public static void OpenWindow() => GetWindow<PurchaseProductEditor>("Purchase Products");

    private void OnEnable() => LoadProducts();

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Purchase Products Editor", EditorStyles.boldLabel);

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        if (_products.Count == 0)
            EditorGUILayout.LabelField("No products found.");

        for (int i = 0; i < _products.Count; i++)
        {
            var p = _products[i];
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Product {i + 1}");
            p.Id = EditorGUILayout.TextField("Id", p.Id);
            p.Name = EditorGUILayout.TextField("Name", p.Name);
            p.Description = EditorGUILayout.TextField("Description", p.Description);
            p.Price = EditorGUILayout.DoubleField("Price", p.Price);
            p.Currency = EditorGUILayout.TextField("Currency", p.Currency);
            p.Amount = EditorGUILayout.IntField("Amount", p.Amount);
            p.Type = (ProductType)EditorGUILayout.EnumPopup("Type", p.Type);

            if (GUILayout.Button("Remove"))
            {
                _products.RemoveAt(i);
                i--;
                _isDirty = true;
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("Add Product"))
        {
            _products.Add(new ProductJson());
            _isDirty = true;
        }

        if (GUILayout.Button("Save JSON"))
        {
            SaveProducts();
        }
    }

    private void LoadProducts()
    {
        if (!Directory.Exists(FolderPath))
            Directory.CreateDirectory(FolderPath);

        if (File.Exists(_filePath))
        {
            string json = File.ReadAllText(_filePath);
            _products = JsonConvert.DeserializeObject<List<ProductJson>>(json) ?? new List<ProductJson>();
        }
    }

    private void SaveProducts()
    {
        string json = JsonConvert.SerializeObject(_products, Formatting.Indented);
        File.WriteAllText(_filePath, json);
        AssetDatabase.Refresh();
        Debug.Log($"Products saved to {_filePath}");
        _isDirty = false; 
    }

    private void OnDestroy()
    {
        if (_isDirty)
            if (EditorUtility.DisplayDialog("Save Changes?", "You have unsaved changes. Do you want to save them?", "Yes", "No"))
                SaveProducts();
    }
}
