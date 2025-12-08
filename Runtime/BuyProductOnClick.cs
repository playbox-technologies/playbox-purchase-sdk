using Playbox.Purchases;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PurchaseExample : MonoBehaviour
{
    private IPurchaseManager _purchaseManager;
    private Button _purchaseButton;

    [SerializeField] private string _productId = "world.dreamsim.playboxtestapp.coin";

    private void Awake()
    {
        _purchaseButton = GetComponent<Button>();
        _purchaseButton.onClick.AddListener(BuyProduct);
        _purchaseManager = DependencyResolver.GetPurchaseManager();
    }

    private void Start()
    {
        _purchaseManager.Initialize();
        if (_purchaseManager == null)
        {
            Debug.LogError("PurchaseExample: IPurchaseManager is null. Check DependencyResolver.");
            return;
        }

        _purchaseManager.OnPurchaseSuccess += HandlePurchaseSuccess;
        _purchaseManager.OnPurchaseFailed += HandlePurchaseFailed;
    }

    private void OnDestroy()
    {
        if (_purchaseManager != null)
        {
            _purchaseManager.OnPurchaseSuccess -= HandlePurchaseSuccess;
            _purchaseManager.OnPurchaseFailed -= HandlePurchaseFailed;
        }
    }

    public void BuyProduct()
    {
        if (_purchaseManager == null) return;

        Debug.Log($"PurchaseExample: BuyProduct -> {_productId}");
        _purchaseManager.Purchase(_productId);
    }

    private void HandlePurchaseSuccess(IProduct product)
    {
        Debug.Log($"Purchase successful: {product.Id}");
    }

    private void HandlePurchaseFailed(IProduct product, string reason)
    {
        Debug.Log($"Purchase failed for product {product?.Id}. Reason: {reason}");
    }
}