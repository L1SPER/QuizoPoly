using UnityEngine;
using System.Collections;

public class DiceRoller : MonoBehaviour
{
    [Header("Başlangıç Pozisyonu (Inspector'dan ayarla)")]
    public Vector3 startPosition;

    [Header("Atış Ayarları")]
    public float throwForce = 5f;
    public float torqueForce = 10f;
    public Vector3 throwDirection = new Vector3(0, 0.5f, 1f);
    public float delayBeforeThrow = 1f;  // Atış öncesi bekleme

    [Header("Yüzey Değerleri")]
    public int upFaceValue = 2;
    public int downFaceValue = 5;
    public int forwardFaceValue = 1;
    public int backFaceValue = 6;
    public int rightFaceValue = 4;
    public int leftFaceValue = 3;

    [Header("Sonuç")]
    public int currentValue = 0;

    private Rigidbody rb;
    private bool isRolling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        MoveToStartPosition();
    }

    void MoveToStartPosition()
    {
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = startPosition;
        transform.rotation = Random.rotation;
    }

    public void RollDice()
    {
        if (isRolling) return;

        StartCoroutine(RollSequence());
    }

    IEnumerator RollSequence()
    {
        isRolling = true;

        // 1. Adım: Başlangıç pozisyonuna git
        MoveToStartPosition();

        // 2. Adım: 1 saniye bekle (oyuncu zarın yerleştiğini görsün)
        yield return new WaitForSeconds(delayBeforeThrow);

        // 3. Adım: Fiziği aç ve kuvvet uygula
        rb.isKinematic = false;

        rb.AddForce(throwDirection.normalized * throwForce, ForceMode.Impulse);

        Vector3 randomTorque = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ) * torqueForce;

        rb.AddTorque(randomTorque, ForceMode.Impulse);

        // 4. Adım: Zar durana kadar bekle
        yield return new WaitForSeconds(0.5f);  // En az 0.5sn dönsün

        float timeout = 5f;
        float elapsed = 0f;

        while ((rb.linearVelocity.magnitude > 0.05f || rb.angularVelocity.magnitude > 0.05f) && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 5. Adım: Sonucu oku
        currentValue = GetTopFaceValue();
        Debug.Log($"Zar sonucu: {currentValue}");

        isRolling = false;
    }

    int GetTopFaceValue()
    {
        Vector3 up = Vector3.up;

        float topDot = Vector3.Dot(transform.up, up);
        float bottomDot = Vector3.Dot(-transform.up, up);
        float frontDot = Vector3.Dot(transform.forward, up);
        float backDot = Vector3.Dot(-transform.forward, up);
        float rightDot = Vector3.Dot(transform.right, up);
        float leftDot = Vector3.Dot(-transform.right, up);

        float max = topDot;
        int value = upFaceValue;

        if (bottomDot > max) { max = bottomDot; value = downFaceValue; }
        if (frontDot > max) { max = frontDot; value = forwardFaceValue; }
        if (backDot > max) { max = backDot; value = backFaceValue; }
        if (rightDot > max) { max = rightDot; value = rightFaceValue; }
        if (leftDot > max) { max = leftDot; value = leftFaceValue; }

        return value;
    }

    public bool IsRolling()
    {
        return isRolling;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startPosition, 0.3f);
    }
}