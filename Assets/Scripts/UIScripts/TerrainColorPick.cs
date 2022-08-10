using UnityEngine;
using UnityEngine.UI;

public class TerrainColorPick : MonoBehaviour
{
    private InteractionType _interaction;

    [SerializeField]
    private GameObject _penPanelImage;

    [SerializeField]
    private Renderer _markerTipRenderer;

    void Start()
    {
        _interaction = Utilities._interaction;
    }

    private void Update()
    {
        if (_interaction == InteractionType.Mouse)
        {
            RectTransform _rect = GetComponent<RectTransform>();
            Color _regionColor = GetComponent<Image>().color;
            bool insideRect = RectTransformUtility.RectangleContainsScreenPoint(_rect, Input.mousePosition);
            if (insideRect)
            {
                Utilities.PreviewMarkerPanelColor(_regionColor, _penPanelImage);

                if (Input.GetMouseButtonDown(0))
                {
                    Utilities.ChangeMarkerColor(
                        _regionColor,
                        _markerTipRenderer,
                        _penPanelImage
                   );
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_interaction == InteractionType.Kinect)
        {
            if (collision.gameObject.name.Equals("Left Hand"))
            {
                //Debug.Log("Collided");

                Utilities.ChangeMarkerColor(
                    GetComponent<Image>().color,
                    _markerTipRenderer,
                    _penPanelImage
                );
            }
        }
    }
}
