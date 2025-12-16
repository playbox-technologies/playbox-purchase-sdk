using Playbox.Purchases;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles product purchase via a button in Unity UI. Manages the purchase process,
/// including checking if the product is consumable, handling success/failure, and 
/// disabling the button while waiting for the result.
/// </summary>
[RequireComponent(typeof(Button))]
public class BuyProductOnClick : MonoBehaviour
{
    private PurchaseManager _purchaseManager;
    private Button _purchaseButton;

    [SerializeField] private string _productId;

    private bool _isWaitingForResult;
    private bool _isNonConsumableProduct;

    /// <summary>
    /// Initializes the button and sets up event listeners for purchase actions.
    /// </summary>
    private void Awake()
    {
        _purchaseButton = GetComponent<Button>();
        _purchaseButton.onClick.AddListener(BuyProduct);

        _purchaseManager = PurchaseManagerProvider.GetPurchaseManager();

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

    /// <summary>
    /// Checks if the non-consumable product has already been purchased and hides the button if it has.
    /// </summary>
    private void CheckNonConsume()
    {
        if (_purchaseManager.IsAlreadyPurchased(_productId))
        {
            _purchaseButton.gameObject.SetActive(false);
            return;
        }
    }

    /// <summary>
    /// Unsubscribes from purchase events when the object is destroyed.
    /// </summary>
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

    /// <summary>
    /// Initiates the purchase process for the product. Disables the button while waiting for the result.
    /// </summary>
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

    /// <summary>
    /// Handles a successful purchase by re-enabling the button and hiding it for non-consumable products.
    /// </summary>
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

    /// <summary>
    /// Handles a failed purchase by re-enabling the button and logging the failure reason.
    /// </summary>
    private void HandlePurchaseFailed(IProduct product, string reason)
    {
        if (product == null || product.Id != _productId)
            return;

        _isWaitingForResult = false;
        _purchaseButton.interactable = true;

        Debug.Log($"PurchaseExample({_productId}): Purchase failed. Reason: {reason}");
    }
}