using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class PurchaseProductEditor : EditorWindow
{
    private const string FolderPath = "Assets/PurchaseProduct";
    private const string FileName = "Products.json";
    private string _filePath => Path.Combine(FolderPath, FileName);

    private List<ProductDefinition> _products = new List<ProductDefinition>();
    private Vector2 _scrollPos;

    [MenuItem("Playbox/Purchase Products Editor")]
    public static void OpenWindow()
    {
        GetWindow<PurchaseProductEditor>("Purchase Products");
    }

    private void OnEnable()
    {
        LoadProducts();
    }

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

            if (GUILayout.Button("Remove"))
            {
                _products.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("Add Product"))
        {
            _products.Add(new ProductDefinition());
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
            _products = JsonConvert.DeserializeObject<List<ProductDefinition>>(json) ?? new List<ProductDefinition>();
        }
    }

    private void SaveProducts()
    {
        string json = JsonConvert.SerializeObject(_products, Formatting.Indented);
        File.WriteAllText(_filePath, json);
        AssetDatabase.Refresh();
        Debug.Log($"Products saved to {_filePath}");
    }
}

[System.Serializable]
public class ProductDefinition
{
    public string Id = "new_id";
    public string Name = "New Product";
    public string Description = "Description";
    public double Price = 0.99;
    public string Currency = "USD";
}