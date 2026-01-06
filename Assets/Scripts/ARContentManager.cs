using Config;
using UnityEngine;
using Vuforia;

public class ARContentManager : MonoBehaviour
{
    [Header("Model 3D Berurutan untuk Marker Ini")]
    [SerializeField] private GameObject[] stepModels;
    
    [Header("ANIMATION")]
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip animClip;
    
    private int currentIndex;

    private void Start()
    {
        currentIndex = int.MaxValue; 
        UpdateModelVisibility(); 
        currentIndex = 0;
    }

    private void OnDisable()
    {
        UIManager.Instance.btnPlayAnimation.onClick.RemoveAllListeners();
        UIManager.Instance.btnStopAnimation.onClick.RemoveAllListeners();
    }

    public void OnTargetFound()
    {
        if(UIManager.Instance.btnPlayAnimation) UIManager.Instance.btnPlayAnimation.onClick.AddListener(PlayAnimation);
        if(UIManager.Instance.btnStopAnimation) UIManager.Instance.btnStopAnimation.onClick.AddListener(StopAnimation);
        
        Debug.Log($"[ARContentManager] Marker '{name}' terdeteksi! Memulai setup navigasi...");

        if (stepModels == null || stepModels.Length == 0)
        {
            Debug.LogError($"[ARContentManager] Array 'Step Models' di marker '{name}' masih KOSONG! Isi di Inspector.");
            return;
        }

        if (stepModels.Length > 1)
        {
            if (UIManager.Instance.btnARNext == null || UIManager.Instance.btnARPrev == null)
            {
                Debug.LogError("[ARContentManager] GAWAT! Tombol Next/Prev belum dipasang di Inspector UIManager!");
            }
            else
            {
                SetButtonNavigationEnabled(true);
                Debug.Log("[ARContentManager] Sukses mengambil referensi tombol dari UIManager.");

                UIManager.Instance.btnARNext.onClick.RemoveAllListeners();
                UIManager.Instance.btnARNext.onClick.AddListener(NextModel);
                Debug.Log("[ARContentManager] Listener 'NextModel' dipasang ke tombol Next.");

                UIManager.Instance.btnARPrev.onClick.RemoveAllListeners();
                UIManager.Instance.btnARPrev.onClick.AddListener(PrevModel);
                Debug.Log("[ARContentManager] Listener 'PrevModel' dipasang ke tombol Prev.");
            }
        }
        else
        {
            SetButtonNavigationEnabled(false);
        }
        
        currentIndex = 0;
        UpdateModelVisibility();
        UpdateButtonState();
        Debug.Log($"[ARContentManager] Model 3D di-reset ke index 0. Total model: {stepModels.Length}");
        
        if (GameManager.Instance != null)
        {
            string markerName = GetComponent<ImageTargetBehaviour>().TargetName;
            GameManager.Instance.OnMarkerFound(markerName);
        }
    }
    
    private void NextModel()
    {
        Debug.Log("[ARContentManager] Tombol NEXT diklik!"); 
        if (currentIndex < stepModels.Length - 1)
        {
            currentIndex++;
            UpdateModelVisibility();
            Debug.Log($"[ARContentManager] Pindah ke model index: {currentIndex}");
        }
    }

    private void PrevModel()
    {
        Debug.Log("[ARContentManager] Tombol PREV diklik!"); 
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateModelVisibility();
            Debug.Log($"[ARContentManager] Pindah ke model index: {currentIndex}");
        }
    }

    void UpdateModelVisibility()
    {
        for (int i = 0; i < stepModels.Length; i++)
        {
            if (stepModels[i] == null)
            {
                Debug.LogWarning($"[ARContentManager] Model di index {i} KOSONG (Missing) di Inspector!");
                continue;
            }
            stepModels[i].SetActive(i == currentIndex);
        }
    }
    
    void UpdateButtonState()
    {
        if (UIManager.Instance.btnARPrev) UIManager.Instance.btnARPrev.interactable = (currentIndex > 0);
        if (UIManager.Instance.btnARNext) UIManager.Instance.btnARNext.interactable = (currentIndex < stepModels.Length - 1);
    }

    void SetButtonNavigationEnabled(bool enabled)
    {
       if(UIManager.Instance.btnARNext != null) UIManager.Instance.btnARNext.gameObject.SetActive(enabled);
       if(UIManager.Instance.btnARPrev != null) UIManager.Instance.btnARPrev.gameObject.SetActive(enabled);
    }

    public void PlayAnimation()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        AnimationConfig cachedAnimationConfig = GameManager.Instance.AnimationConfig;
        if (cachedAnimationConfig == null)
        {
            return;
        }
        if (cachedAnimationConfig.GenericAnimController == null)
        {
           return;
        }
        cachedAnimationConfig.GenericAnimController[cachedAnimationConfig.GetAnimGenericClipEntryName()] = animClip;
        if (animator != null)
        {
            Debug.Log("[ARContentManager] Memulai animasi...");
            animator.SetTrigger(cachedAnimationConfig.StopAnimationParamName);  
            animator.SetTrigger(cachedAnimationConfig.PlayAnimationParamName);
        }
    }

    public void StopAnimation()
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        if (GameManager.Instance.AnimationConfig == null)
        {
            return;
        }
        if (animator == null)
        {
            Debug.Log("[ARContentManager] Menghentikan animasi...");
            return;
        }
        animator.SetTrigger(GameManager.Instance.AnimationConfig.StopAnimationParamName);  
    }
    
    public void OnTargetLost()
    {
        if (UIManager.Instance != null)   UIManager.Instance.HideAllARPopups();

        if (UIManager.Instance.btnARNext  != null) UIManager.Instance.btnARNext.onClick.RemoveAllListeners();
        if (UIManager.Instance.btnARPrev != null) UIManager.Instance.btnARPrev.onClick.RemoveAllListeners();
        if (UIManager.Instance.btnPlayAnimation != null) UIManager.Instance.btnPlayAnimation.onClick.RemoveAllListeners();
        if (UIManager.Instance.btnStopAnimation != null) UIManager.Instance.btnStopAnimation.onClick.RemoveAllListeners();
        Debug.Log($"[ARContentManager] Marker '{name}' hilang. Listener tombol dibersihkan.");
    }
}