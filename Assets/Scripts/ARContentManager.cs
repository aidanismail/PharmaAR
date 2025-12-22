using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class ARContentManager : MonoBehaviour
{
    public static ARContentManager Instance { get; private set; }
    [Header("Model 3D Berurutan untuk Marker Ini")]
    public GameObject[] stepModels;
    public Animator animator;

    public string[] animationClips; 

    private int currentClipIndex = 0; 
    private Button btnPlayAnimation;
    private Button btnStopAnimation;

    private int currentIndex = 0;
    private Button uiBtnNext;
    private Button uiBtnPrev;

    void Start()
    {
        
        btnPlayAnimation = UIManager.Instance.btnPlayAnimation;
        btnStopAnimation = UIManager.Instance.btnStopAnimation;

        btnPlayAnimation.onClick.AddListener(PlayAnimation);
        btnStopAnimation.onClick.AddListener(StopAnimation);
    }

    
    public void OnTargetFound()
    {
        Debug.Log($"[ARContentManager] Marker '{name}' terdeteksi! Memulai setup navigasi...");

        
        if (UIManager.Instance != null)
        {
            uiBtnNext = UIManager.Instance.btnARNext;
            uiBtnPrev = UIManager.Instance.btnARPrev;

            
            if (uiBtnNext == null || uiBtnPrev == null)
            {
                Debug.LogError("[ARContentManager] GAWAT! Tombol Next/Prev belum dipasang di Inspector UIManager!");
            }
            else
            {
                Debug.Log("[ARContentManager] Sukses mengambil referensi tombol dari UIManager.");

                
                uiBtnNext.onClick.RemoveAllListeners();
                uiBtnNext.onClick.AddListener(NextModel);
                Debug.Log("[ARContentManager] Listener 'NextModel' dipasang ke tombol Next.");

                uiBtnPrev.onClick.RemoveAllListeners();
                uiBtnPrev.onClick.AddListener(PrevModel);
                Debug.Log("[ARContentManager] Listener 'PrevModel' dipasang ke tombol Prev.");
            }
        }
        else
        {
            Debug.LogError("[ARContentManager] UIManager Instance tidak ditemukan! Navigasi gagal.");
        }

        
        if (stepModels == null || stepModels.Length == 0)
        {
            Debug.LogError($"[ARContentManager] Array 'Step Models' di marker '{name}' masih KOSONG! Isi di Inspector.");
        }
        else
        {
            currentIndex = 0;
            UpdateModelVisibility();
            Debug.Log($"[ARContentManager] Model 3D di-reset ke index 0. Total model: {stepModels.Length}");
        }

        
        string markerName = GetComponent<ImageTargetBehaviour>().TargetName;
        if (GameManager.Instance != null)
        {
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
        UpdateButtonState();
    }

    void UpdateButtonState()
    {
        if (uiBtnPrev) uiBtnPrev.interactable = (currentIndex > 0);
        if (uiBtnNext) uiBtnNext.interactable = (currentIndex < stepModels.Length - 1);
    }

    public void PlayAnimation()
    {
        if (animator != null)
        {
            
            Debug.Log("[ARContentManager] Memulai animasi...");
            animator.SetTrigger("PlayAnimation");
        }
    }

    public void StopAnimation()
    {
        if (animator != null)
        {
            
            Debug.Log("[ARContentManager] Menghentikan animasi...");
            animator.SetTrigger("StopAnimation");  
        }
    }

    
    public void ChangeAnimation(int clipIndex)
    {
        currentClipIndex = clipIndex;
        PlayAnimation();
    }

    public void OnTargetLost()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideAllARPopups();
        }
        
        if (uiBtnNext) uiBtnNext.onClick.RemoveAllListeners();
        if (uiBtnPrev) uiBtnPrev.onClick.RemoveAllListeners();
        if (btnPlayAnimation != null) btnPlayAnimation.onClick.RemoveAllListeners();
        if (btnStopAnimation != null) btnStopAnimation.onClick.RemoveAllListeners();
        Debug.Log($"[ARContentManager] Marker '{name}' hilang. Listener tombol dibersihkan.");
    }
}