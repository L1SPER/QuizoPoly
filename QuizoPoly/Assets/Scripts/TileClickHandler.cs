using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TileClickHandler : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Image colorImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Color nameColor;
    [SerializeField] private TextMeshProUGUI priceText;

    private void Update()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray,out RaycastHit hit))
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null)
                {
                    infoPanel.SetActive(true);
                    colorImage.color = tile.groupColor;
                    nameText.text = tile.tileName;
                    nameText.color=nameColor;
                    priceText.text = tile.basePrice.ToString("N0", new System.Globalization.CultureInfo("tr-TR"));
                }
                else
                {
                    infoPanel.SetActive(false);
                }
            }
            else
            {
                infoPanel.SetActive(false);
            }
        }
    }
}
