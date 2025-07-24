using UnityEngine;
using System.Collections;
using TMPro;
using NaughtyAttributes;

public class CoinEffectManager : MonoBehaviour
{
    public GameObject coinPrefab;           // Prefab đồng tiền (world object, không phải UI)
    public Transform targetUITarget;        // Empty object gắn với UI (ví dụ: con của Text)
    public int totalCoins = 10;
    public float spawnRadius = 2f;          // Bán kính spawn (theo world unit)
    public float moveDuration = 0.8f;
    public float spawnInterval = 0.05f;
    public Camera uiCamera;                 // Camera render UI (thường là Main Camera)

    private Vector3 targetScreenPosition;
    private Vector3 targetWorldPosition;

    private void Start()
    {
        // Nếu Canvas là Screen Space, ta cần update vị trí đích mỗi lần
        // Vì UI có thể thay đổi vị trí khi resize màn hình
    }

    [Button]
    public void PlayCoinEffect()
    {
        StartCoroutine(SpawnAndMoveCoins(new Vector3(-0.03f, -1.84f, 4.8f)));
    }

    private IEnumerator SpawnAndMoveCoins(Vector3 spawnPosition)
    {
        // Chuyển vị trí UI thành world position dùng Camera
        targetWorldPosition = GetUIWorldPosition();

        for (int i = 0; i < totalCoins; i++)
        {
            // Vị trí bắt đầu: xung quanh vị trí spawn
            Vector3 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 finalSpawnPos = spawnPosition + randomOffset;

            // Tạo đồng tiền trong world space
            GameObject coin = Instantiate(coinPrefab, finalSpawnPos, Quaternion.identity);

            // Bay về đích (world position tương ứng với UI)
            float elapsed = 0;
            Vector3 startPos = coin.transform.position;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / moveDuration;
                coin.transform.position = Vector3.Lerp(startPos, targetWorldPosition, t);
                yield return null;
            }

            // Xóa sau khi đến đích
            Destroy(coin);

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // Chuyển đổi vị trí UI (UI anchor) thành world position
    private Vector3 GetUIWorldPosition()
    {
        // Dùng RectTransformUtility để chuyển từ UI space sang world space
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            targetUITarget.GetComponent<RectTransform>(),
            targetUITarget.position,
            uiCamera,
            out Vector3 worldPos
        );
        return worldPos;
    }
}