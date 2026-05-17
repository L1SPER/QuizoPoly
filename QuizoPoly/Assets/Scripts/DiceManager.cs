using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DiceManager : MonoBehaviour
{
    [Header("Zarlar")]
    public DiceRoller dice1;
    public DiceRoller dice2;

    [Header("UI")]
    public TMP_Text dice1Text;
    public TMP_Text dice2Text;
    public TMP_Text totalText;

    public void RollBothDice()
    {
        if (dice1.IsRolling() || dice2.IsRolling())
            return;

        StartCoroutine(RollAndWait());
    }

    IEnumerator RollAndWait()
    {
        // UI'yi temizle
        if (dice1Text) dice1Text.text = "?";
        if (dice2Text) dice2Text.text = "?";
        if (totalText) totalText.text = "...";

        // İki zarı da fırlat
        dice1.RollDice();
        dice2.RollDice();

        // İkisi de durana kadar bekle
        while (dice1.IsRolling() || dice2.IsRolling())
        {
            yield return null;
        }

        // Sonuçları UI'ye yaz
        int v1 = dice1.currentValue;
        int v2 = dice2.currentValue;
        int total = v1 + v2;

        if (dice1Text) dice1Text.text = v1.ToString();
        if (dice2Text) dice2Text.text = v2.ToString();
        if (totalText) totalText.text = $"Toplam: {total}";

        Debug.Log($"Zar 1: {v1}, Zar 2: {v2}, Toplam: {total}");
    }
    public bool IsRolling()
    {
        return dice1.IsRolling() || dice2.IsRolling();
    }

    public int GetTotal()
    {
        return dice1.currentValue + dice2.currentValue;
    }
}