using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public enum PlayerScrollType
{
    FirstPlayer,
    SecondPlayer
}

public class InfiniteScrollWithSnapping : MonoBehaviour
{
    [Header("Referanslar")] public ScrollRect scrollRect; // İşlemler bu scrollRect için yapılacak

    [Header("Ayarlar")] [Tooltip("Snap (ortala) hızı")]
    public float snappingSpeed = 10f;

    [Tooltip("Ek snap offset (örneğin viewport merkezinde kalması için)")]
    public float snapOffset = 0f;

    [Tooltip("Eğer PlayerPrefs'te kayıt yoksa bu default index kullanılacak")] [SerializeField]
    private int defaultSnapIndex = 0;

    [Header("Oyuncu Scroll Ayarı")] [SerializeField]
    private PlayerScrollType scrollType; // Hangi oyuncuya ait scroll (FirstPlayer/SecondPlayer)

    // Inertia için değişkenler
    private float currentVelocity = 0f;
    private float deceleration = 5f; // Ataletin yavaşlama katsayısı
    private float velocityThreshold = 10f; // Otomatik snap için gereken minimum hız

    private RectTransform contentRect;
    private HorizontalLayoutGroup layoutGroup;
    private float spacing;
    private int leftPadding, rightPadding, itemCount;

    // Elemanların orijinal sıralamasını korumak için liste (sabit indexler)
    private List<RectTransform> originalOrder = new List<RectTransform>();

    // Son kaydedilen sabit index (ilk seferde -1)
    private int lastSavedIndex = -1;

    // Başlangıç snap işleminin tamamlandığını belirten bayrak
    private bool initialSnapComplete = false;

    // Başlangıç için hedef pozisyon (PlayerPrefs'ten okunan index'e göre hesaplanır)
    private Vector2 initialTargetPos;

    // Snap tamamlanma eşik değeri
    private float snapThreshold = 0.1f;

    // Kullanıcı etkileşimini takip eden bayrak (sadece bu scrollRect için)
    private bool isDragging = false;
    private Vector2 previousDragPos;
    [HideInInspector] public int CurrentIndex; // Şu anki elemanın index'i
    void Start()
    {
        if (scrollRect == null)
        {
            enabled = false;
            return;
        }

        contentRect = scrollRect.content;
        itemCount = contentRect.childCount;
        if (itemCount == 0)
        {
            enabled = false;
            return;
        }

        // Her elemente sabit index atayıp orijinal sıralamayı saklıyoruz.
        for (int i = 0; i < itemCount; i++)
        {
            RectTransform child = contentRect.GetChild(i) as RectTransform;
            child.gameObject.name = "Element " + i;
            ElementIndex ei = child.gameObject.GetComponent<ElementIndex>();
            if (ei == null)
                ei = child.gameObject.AddComponent<ElementIndex>();
            ei.fixedIndex = i;
            originalOrder.Add(child);
        }

        // Horizontal Layout Group değerlerini alıyoruz.
        layoutGroup = contentRect.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            spacing = layoutGroup.spacing;
            leftPadding = layoutGroup.padding.left;
            rightPadding = layoutGroup.padding.right;
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            layoutGroup.enabled = false;
        }
        else
        {
            spacing = 0f;
            leftPadding = 0;
            rightPadding = 0;
        }

        // Elemanları layout group değerlerine göre manuel olarak konumlandırıyoruz.
        float currentX = leftPadding;
        for (int i = 0; i < itemCount; i++)
        {
            RectTransform child = contentRect.GetChild(i) as RectTransform;
            float childWidth = child.rect.width;
            child.anchoredPosition = new Vector2(currentX + childWidth * child.pivot.x, child.anchoredPosition.y);
            currentX += childWidth + spacing;
        }

        // Başlangıçta PlayerPrefs'te kaydedilmiş index'e göre hedef pozisyonu hesaplayalım.
        string key = (scrollType == PlayerScrollType.FirstPlayer) ? "FirstPlayer" : "SecondPlayer";
        int savedIndex = defaultSnapIndex;
        if (PlayerPrefs.HasKey(key))
        {
            savedIndex = PlayerPrefs.GetInt(key);
            if (savedIndex < 0 || savedIndex >= itemCount)
                savedIndex = defaultSnapIndex;
        }
        else
        {
            PlayerPrefs.SetInt(key, defaultSnapIndex);
            PlayerPrefs.Save();
            savedIndex = defaultSnapIndex;
        }

        // Orijinal sıradan kaydedilen index'e ait elemanı alalım.
        RectTransform targetChild = null;
        if (savedIndex < originalOrder.Count)
            targetChild = originalOrder[savedIndex];
        if (targetChild != null)
        {
            // Hedef: (targetChild.anchoredPosition.x + contentRect.anchoredPosition.x) = snapOffset
            // → contentRect.anchoredPosition.x = snapOffset - targetChild.anchoredPosition.x
            initialTargetPos = new Vector2(snapOffset - targetChild.anchoredPosition.x, contentRect.anchoredPosition.y);
            lastSavedIndex = savedIndex;
        }

        CurrentIndex = lastSavedIndex; 
    }

    void Update()
    {
        if (isMovingWithButton) return;
        // Kullanıcı tıklamasını GetMouseButtonDown ile algılıyoruz.
        if (Input.GetMouseButtonDown(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(scrollRect.GetComponent<RectTransform>(),
                    Input.mousePosition))
            {
                isDragging = true;
                previousDragPos = Input.mousePosition;
                currentVelocity = 0f;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // İlk snap işlemi henüz tamamlanmadıysa, o kısım Update'de çalışsın.
        if (!initialSnapComplete)
        {
            contentRect.anchoredPosition = Vector2.Lerp(contentRect.anchoredPosition, initialTargetPos,
                Time.deltaTime * snappingSpeed);
            if (Vector2.Distance(contentRect.anchoredPosition, initialTargetPos) < snapThreshold)
            {
                contentRect.anchoredPosition = initialTargetPos;
                initialSnapComplete = true;
                CharacterSelectionManager.Instance.SelectedCharacterChanged();
            }

            return;
        }

        if (contentRect.childCount > 0)
        {
            float halfViewportWidth = scrollRect.viewport.rect.width / 2f;
            float leftBound = -halfViewportWidth;
            float rightBound = halfViewportWidth;
            RectTransform firstChild = contentRect.GetChild(0) as RectTransform;
            RectTransform lastChild = contentRect.GetChild(contentRect.childCount - 1) as RectTransform;
            Vector3 firstChildCenterWorld = firstChild.TransformPoint(firstChild.rect.center);
            Vector3 lastChildCenterWorld = lastChild.TransformPoint(lastChild.rect.center);
            Vector3 firstChildViewportPos = scrollRect.viewport.InverseTransformPoint(firstChildCenterWorld);
            Vector3 lastChildViewportPos = scrollRect.viewport.InverseTransformPoint(lastChildCenterWorld);
            float firstChildHalfWidth = firstChild.rect.width / 2f;
            float lastChildHalfWidth = lastChild.rect.width / 2f;
            if (firstChildViewportPos.x + firstChildHalfWidth < leftBound)
            {
                float newX = lastChild.anchoredPosition.x + lastChild.rect.width + spacing;
                firstChild.anchoredPosition = new Vector2(newX, firstChild.anchoredPosition.y);
                firstChild.SetAsLastSibling();
            }
            else if (lastChildViewportPos.x - lastChildHalfWidth > rightBound)
            {
                float newX = firstChild.anchoredPosition.x - lastChild.rect.width - spacing;
                lastChild.anchoredPosition = new Vector2(newX, lastChild.anchoredPosition.y);
                lastChild.SetAsFirstSibling();
            }
        }

        // Eğer kullanıcı hala drag yapıyorsa, drag kodunu çalıştır.
        if (isDragging)
        {
            Vector2 currentDragPos = Input.mousePosition;
            float deltaX = currentDragPos.x - previousDragPos.x;
            contentRect.anchoredPosition += new Vector2(deltaX, 0);
            currentVelocity = deltaX / Time.deltaTime;
            previousDragPos = currentDragPos;

            // Teleport (sonsuz scroll) mantığını drag sırasında çalıştırabilirsiniz.
        }
        else
        {
            // Kullanıcı drag yapmayı bıraktıysa:
            if (Mathf.Abs(currentVelocity) > velocityThreshold)
            {
                // Hız eşik değerinin üzerindeyse, inertia (atalet) ile hareket etsin.
                contentRect.anchoredPosition += new Vector2(currentVelocity * Time.deltaTime, 0);
                currentVelocity = Mathf.Lerp(currentVelocity, 0, deceleration * Time.deltaTime);
            }
            else
            {
                // Hız eşik altına düştüyse, otomatik snap devreye girsin.
                float minDistance = float.MaxValue;
                RectTransform closestChild = null;
                for (int i = 0; i < contentRect.childCount; i++)
                {
                    RectTransform child = contentRect.GetChild(i) as RectTransform;
                    float effectivePos = child.anchoredPosition.x + contentRect.anchoredPosition.x;
                    float distance = Mathf.Abs(effectivePos - snapOffset);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestChild = child;
                    }
                }

                if (closestChild != null)
                {
                    Vector2 targetPos = new Vector2(snapOffset - closestChild.anchoredPosition.x,
                        contentRect.anchoredPosition.y);
                    contentRect.anchoredPosition = Vector2.Lerp(contentRect.anchoredPosition, targetPos,
                        Time.deltaTime * snappingSpeed);
                    int currentFixedIndex = closestChild.GetComponent<ElementIndex>().fixedIndex;
                    if (currentFixedIndex != lastSavedIndex)
                    {
                        SaveSelectedIndex(currentFixedIndex);
                        lastSavedIndex = currentFixedIndex;
                    }
                }
            }
        }
    }
    private bool isMovingWithButton = false; // Buton ile kaydırma işlemi yapılıp yapılmadığını kontrol eder.

  public void MoveScroll(int direction)
{
    if (isMovingWithButton || direction == 0 || contentRect.childCount == 0)
        return;

    isMovingWithButton = true; // Update'in diğer işlemlerini geçici olarak devre dışı bırak

    // Mevcut en yakın elemanı bul
    float minDistance = float.MaxValue;
    RectTransform closestChild = null;
    int closestIndex = 0;

    for (int i = 0; i < contentRect.childCount; i++)
    {
        RectTransform child = contentRect.GetChild(i) as RectTransform;
        float effectivePos = child.anchoredPosition.x + contentRect.anchoredPosition.x;
        float distance = Mathf.Abs(effectivePos - snapOffset);

        if (distance < minDistance)
        {
            minDistance = distance;
            closestChild = child;
            closestIndex = i;
        }
    }

    // Yeni hedef elemanı belirle
    int targetIndex = Mathf.Clamp(closestIndex + direction, 0, contentRect.childCount - 1);
    RectTransform targetChild = contentRect.GetChild(targetIndex) as RectTransform;

    if (targetChild != null)
    {
        Vector2 targetPos = new Vector2(snapOffset - targetChild.anchoredPosition.x, contentRect.anchoredPosition.y);
        StartCoroutine(SmoothScroll(targetPos, targetChild));
    }
}

// Kaydırmayı yumuşatmak için Coroutine
private IEnumerator SmoothScroll(Vector2 targetPos, RectTransform targetChild)
{
    float elapsedTime = 0f;
    float duration = 0.3f; // Kaydırma süresi (isteğe bağlı ayarlanabilir)

    Vector2 startPos = contentRect.anchoredPosition;

    while (elapsedTime < duration)
    {
        contentRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, elapsedTime / duration);
        elapsedTime += Time.deltaTime;
        yield return null;
    }

    contentRect.anchoredPosition = targetPos;
    isMovingWithButton = false; // Kaydırma tamamlandı, Update tekrar çalışabilir

    // Yeni seçili index'i kaydet
    int newFixedIndex = targetChild.GetComponent<ElementIndex>().fixedIndex;
    SaveSelectedIndex(newFixedIndex);
}

    public void SaveSelectedIndex(int index)
    {
        CurrentIndex = index; 
        CharacterSelectionManager.Instance.SelectedCharacterChanged();
        string key = (scrollType == PlayerScrollType.FirstPlayer) ? "FirstPlayer" : "SecondPlayer";
        PlayerPrefs.SetInt(key, index);
        PlayerPrefs.Save();
    }
    public void ChangeColorToNormal()
    {
        for (int i = 0; i < contentRect.childCount; i++)
        {
            contentRect.GetChild(i).GetComponent<Image>().color = Color.white;
        }
    }
    public void ChangeColorToGrey(int index)
    {
        for (int i = 0; i < contentRect.childCount; i++)
        {
            if (contentRect.GetChild(i).GetComponent<ElementIndex>().fixedIndex == index)
            {
                contentRect.GetChild(i).GetComponent<Image>().color = Color.grey;
            }
        }
        
    }
    
}