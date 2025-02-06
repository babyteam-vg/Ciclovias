using TMPro;
using UnityEngine;

public class ConstructionMaterial : MonoBehaviour
{
    public static ConstructionMaterial Instance { get; private set; }

    public int constructionMaterial;

    [SerializeField] private TextMeshProUGUI amountText;

    // === Methods ===
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Don't Destroy When Loading Another Scene
    }

    private void Start()
    {
        amountText.text = constructionMaterial.ToString();
    }

    // Add Construction Material
    public void AddMaterial(int cantidad)
    {
        constructionMaterial += cantidad;
        amountText.text = constructionMaterial.ToString();
    }

    // Consume Construction Material
    public bool ConsumeMaterial(int cantidad)
    {
        if (constructionMaterial >= cantidad)
        {
            constructionMaterial -= cantidad;
            amountText.text = constructionMaterial.ToString();
            return true;
        }
        else
            return false;
    }
}
