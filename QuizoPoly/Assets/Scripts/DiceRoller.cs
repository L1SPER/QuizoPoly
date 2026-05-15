using UnityEngine;
using System.Collections;

public class DiceRoller : MonoBehaviour
{
    [Header("Atış Ayarları")]
    public float throwForce = 5f;
    public float torqueForce = 10f;
    public Vector3 throwDirection = new Vector3(0, 0.5f, 1f);

    [Header("Yüzey Değerleri (Inspector'dan ayarla)")]
    [Tooltip("Zar (0,0,0) rotasyonda iken hangi yönde hangi sayı var")]
    public int upFaceValue = 2;        // transform.up yönündeki yüzeyde yazan sayı
    public int downFaceValue = 5;      // -transform.up
    public int forwardFaceValue = 1;   // transform.forward
    public int backFaceValue = 6;      // -transform.forward
    public int rightFaceValue = 3;     // transform.right
    public int leftFaceValue = 4;      // -transform.right

    [Header("Sonuç")]
    public int currentValue = 0;

    private Rigidbody rb;
    private bool isRolling = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void RollDice()
    {
        if (isRolling) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.rotation = Random.rotation;

        rb.AddForce(throwDirection.normalized * throwForce, ForceMode.Impulse);

        Vector3 randomTorque = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ) * torqueForce;

        rb.AddTorque(randomTorque, ForceMode.Impulse);

        StartCoroutine(WaitForDiceToStop());
    }

    IEnumerator WaitForDiceToStop()
    {
        isRolling = true;
        yield return new WaitForSeconds(0.5f);

        while (rb.linearVelocity.magnitude > 0.05f || rb.angularVelocity.magnitude > 0.05f)
        {
            yield return null;
        }

        currentValue = GetTopFaceValue();
        Debug.Log($"Zar sonucu: {currentValue}");

        isRolling = false;
    }

    int GetTopFaceValue()
    {
        Vector3 up = Vector3.up;

        // Hangi yön yukarı bakıyor, en yakın olanı bul
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
}