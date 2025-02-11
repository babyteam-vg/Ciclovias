using TMPro;
using UnityEngine;

public class ConstructionMaterial : MonoBehaviour
{
    public static ConstructionMaterial Instance { get; private set; }

    public int material;

    [SerializeField] private GameObject materialCounter;
    [SerializeField] private TextMeshProUGUI amountText;

    private Animator animator;

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
        amountText.text = "x" + material.ToString();
        animator = materialCounter.GetComponent<Animator>();
    }

    // Add Construction Material
    public void AddMaterial(int cantidad)
    {
        material += cantidad;
        amountText.text = "x" + material.ToString();
        MaterialCounterAnimation();
    }

    // Consume Construction Material
    public bool ConsumeMaterial(int cantidad)
    {
        if (material >= cantidad)
        {
            material -= cantidad;
            amountText.text = "x" + material.ToString();
            MaterialCounterAnimation();
            return true;
        }
        else
            return false;
    }

    public void MaterialCounterAnimation()
    {
        animator.Play("MaterialCounter");
    }
}
