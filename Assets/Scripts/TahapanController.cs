using UnityEngine;
using UnityEngine.UI;

public class TahapanController : MonoBehaviour
{
    [Tooltip("Index tahapan ini (mulai dari 0 untuk Tahap 1).")]
    public int tahapIndex;   

    private Button myButton;

    private Button btnInfo;
    private CanvasGroup myCanvasGroup;

    void Awake()
    {
        myButton = GetComponent<Button>();
        myCanvasGroup = GetComponent<CanvasGroup>();
        btnInfo = GetComponent<Button>();

        if (myButton == null)
            Debug.LogError($"[TahapanController] Button component missing on {gameObject.name}!");
        if (myCanvasGroup == null)
            Debug.LogError($"[TahapanController] CanvasGroup component missing on {gameObject.name}. Tambahkan CanvasGroup untuk kontrol alpha & interaksi.");

        
        if (myButton != null)
        {
            myButton.onClick.RemoveListener(OnTahapClicked); 
            myButton.onClick.AddListener(OnTahapClicked);
        }

        if (btnInfo != null)
        {
            btnInfo.onClick.AddListener(OnInfoButtonClicked);
        }
        else
        {
            Debug.LogError("[TahapanController] Tombol Info tidak ditemukan.");
        }
    }

    private void OnTahapClicked()
    {
        TahapanData data = GameManager.Instance.GetCurrentTahapanData(GameManager.Instance.currentAttemptingTahapIndex);
        if (GameManager.Instance != null)
        {
            Debug.Log($"[TahapanController] Tombol Tahap index {tahapIndex} diklik.");
            GameManager.Instance.StartTahap(tahapIndex);
            GameManager.Instance.HideInfoPopup(data.panelInfo);
        }
        else
        {
            Debug.LogError("[TahapanController] GameManager.Instance is null!");
        }
    }

    private void OnInfoButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log($"[TahapanController] Tombol Info untuk Tahap {tahapIndex} diklik.");
            UIManager.Instance.ShowInfoPanel();  
        }
    }

    
    public void UpdateVisualState(bool isInteractable, float alpha)
    {
        if (myCanvasGroup != null)
        {
            myCanvasGroup.alpha = alpha;
            myCanvasGroup.interactable = isInteractable;
            myCanvasGroup.blocksRaycasts = isInteractable;
        }
    }
}
