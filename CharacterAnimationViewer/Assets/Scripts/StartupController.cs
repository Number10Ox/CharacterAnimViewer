using UnityEngine;

public class StartupController : MonoBehaviour
{
    [SerializeField]
    private float  delayBeforeShowPopup = 1;
    [SerializeField] private CharacterAnimViewerPopupController popupController;

    private void Start()
    {
        if (popupController != null)
            ShowPopupWithDelay();
    }

    private void ShowPopupWithDelay()
    {
        Invoke(nameof(ShowStartupPopup), delayBeforeShowPopup);
    }

    private void ShowStartupPopup()
    {
        popupController.Show();
    }
} 