using TMPro;
using UnityEngine;

// Muestra cuántos coleccionables lleva el jugador. Se "resetea" solo: al morir, la
// escena se recarga entera (EnemyCatchPlayer), así que LevelManager vuelve a nacer
// en 0 sin necesidad de lógica extra aquí.
[RequireComponent(typeof(TextMeshProUGUI))]
public class CollectibleCounterUI : MonoBehaviour
{
    private TextMeshProUGUI label;

    void Start()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (LevelManager.Instance == null) return;
        label.text = $"Disquetes: {LevelManager.Instance.CurrentCollected} / {LevelManager.Instance.totalRequired}";
    }
}
