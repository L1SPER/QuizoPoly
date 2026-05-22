using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class QuestionManager : MonoBehaviour
{
    public static QuestionManager Instance { get; private set; }

    [Header("JSON Dosya Adı (StreamingAssets klasöründe)")]
    public string jsonFileName = "questions.json";

    private List<Question> allQuestions = new List<Question>();
    private HashSet<int> usedQuestionIds = new HashSet<int>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        LoadQuestions();
    }

    void LoadQuestions()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"Soru dosyası bulunamadı: {path}");
            return;
        }

        string jsonText = File.ReadAllText(path);

        try
        {
            QuestionDatabase database = JsonUtility.FromJson<QuestionDatabase>(jsonText);
            if (database != null && database.questions != null)
            {
                allQuestions = database.questions;
                Debug.Log($"✓ {allQuestions.Count} soru yüklendi");
            }
            else
            {
                Debug.LogError("JSON parse edildi ama soru listesi boş!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON parse hatası: {e.Message}");
        }
    }

    public Question GetRandomQuestion(Category category, int difficulty)
    {
        if (allQuestions.Count == 0)
        {
            Debug.LogError("Soru bankası boş!");
            return GetFallbackQuestion(category, difficulty);
        }

        string categoryName = category.ToString();

        // 1. Önce: Kullanılmamış + aynı kategori + aynı zorluk
        var matching = allQuestions.Where(q =>
            q.category == categoryName &&
            q.difficulty == difficulty &&
            !usedQuestionIds.Contains(q.id)).ToList();

        if (matching.Count > 0)
        {
            var selected = matching[Random.Range(0, matching.Count)];
            usedQuestionIds.Add(selected.id);
            return selected;
        }

        // 2. Sonra: Kullanılmamış + aynı kategori (farklı zorluk)
        var sameCategoryAny = allQuestions.Where(q =>
            q.category == categoryName &&
            !usedQuestionIds.Contains(q.id)).ToList();

        if (sameCategoryAny.Count > 0)
        {
            var selected = sameCategoryAny[Random.Range(0, sameCategoryAny.Count)];
            usedQuestionIds.Add(selected.id);
            Debug.LogWarning($"'{categoryName}' kategorisinde difficulty={difficulty} bulunamadı, farklı zorlukta soru verildi");
            return selected;
        }

        // 3. Hiç bulunamadı: tüm sorulardan rastgele
        var anyUnused = allQuestions.Where(q => !usedQuestionIds.Contains(q.id)).ToList();
        if (anyUnused.Count > 0)
        {
            var selected = anyUnused[Random.Range(0, anyUnused.Count)];
            usedQuestionIds.Add(selected.id);
            Debug.LogWarning($"'{categoryName}' kategorisinde hiç soru kalmadı, başka kategoriden verildi");
            return selected;
        }

        // 4. Tüm sorular tükendi → reset edip baştan başla
        Debug.LogWarning("Tüm sorular tükendi, kullanılmış soruları sıfırlıyorum");
        usedQuestionIds.Clear();
        return GetRandomQuestion(category, difficulty);
    }

    Question GetFallbackQuestion(Category category, int difficulty)
    {
        // Soru bankası boşsa veya hata olduğunda fallback
        return new Question
        {
            id = -1,
            category = category.ToString(),
            difficulty = difficulty,
            text = $"[Soru bulunamadı - {category} / Difficulty {difficulty}]",
            choices = new string[] { "A şıkkı", "B şıkkı", "C şıkkı", "D şıkkı" },
            correctAnswer = 0
        };
    }

    public int GetRemainingQuestionCount()
    {
        return allQuestions.Count - usedQuestionIds.Count;
    }

    public void ResetUsedQuestions()
    {
        usedQuestionIds.Clear();
    }
}