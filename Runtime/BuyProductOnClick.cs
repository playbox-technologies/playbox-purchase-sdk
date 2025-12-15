using Playbox.Purchases;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuyProductOnClick : MonoBehaviour
{
    private PurchaseManager _purchaseManager;
    private Button _purchaseButton;

    [SerializeField] private string _productId;

    private bool _isWaitingForResult;
    
    private bool _isNonConsumableProduct;


    private void Awake()
    {
        _purchaseButton = GetComponent<Button>();
        _purchaseButton.onClick.AddListener(BuyProduct);

        _purchaseManager = DependencyResolver.GetPurchaseManager();

        if (_purchaseManager == null)
        {
            Debug.LogError("PurchaseExample: IPurchaseManager is null. Check DependencyResolver.");
            _purchaseButton.interactable = false;
            return;
        }

        _isNonConsumableProduct = _purchaseManager.GetProductType(_productId) == ProductType.NonConsumable;

        if (_isNonConsumableProduct)
            CheckNonConsume();

        _purchaseManager.OnPurchaseSuccess += HandlePurchaseSuccess;
        _purchaseManager.OnPurchaseFailed += HandlePurchaseFailed;
    }

    private void CheckNonConsume()
    {
        if (_purchaseManager.IsAlreadyPurchased(_productId))
        {
            //I put it away for the duration of the tests
            // _purchaseButton.gameObject.SetActive(false);
            return;
        }
    }

    private void OnDestroy()
    {
        if (_purchaseManager != null)
        {
            _purchaseManager.OnPurchaseSuccess -= HandlePurchaseSuccess;
            _purchaseManager.OnPurchaseFailed -= HandlePurchaseFailed;
        }

        if (_purchaseButton != null)
            _purchaseButton.onClick.RemoveListener(BuyProduct);
    }

    public void BuyProduct()
    {
        if (_purchaseManager == null)
        {
            Debug.LogError("PurchaseExample: BuyProduct called but IPurchaseManager is null.");
            return;
        }

        if (string.IsNullOrEmpty(_productId))
        {
            Debug.LogError("PurchaseExample: ProductId is empty.");
            return;
        }

        if (_isWaitingForResult)
        {
            Debug.LogWarning($"PurchaseExample({_productId}): purchase already in progress.");
            return;
        }

        _isWaitingForResult = true;
        _purchaseButton.interactable = false;

        Debug.Log($"PurchaseExample({_productId}): BuyProduct");
        _purchaseManager.Purchase(_productId);
    }

    private void HandlePurchaseSuccess(IProduct product)
    {
        if (product == null || product.Id != _productId)
            return;

        _isWaitingForResult = false;
        _purchaseButton.interactable = true;

        Debug.Log($"PurchaseExample({_productId}): Purchase successful");

        if (_isNonConsumableProduct)
            CheckNonConsume();
    }

    private void HandlePurchaseFailed(IProduct product, string reason)
    {
        if (product == null || product.Id != _productId)
            return;

        _isWaitingForResult = false;
        _purchaseButton.interactable = true;

        Debug.Log($"PurchaseExample({_productId}): Purchase failed. Reason: {reason}");
    }
}