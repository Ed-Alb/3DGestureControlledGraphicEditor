using System.Linq;
using UnityEngine;

public class Marker : MonoBehaviour
{
    public struct MousePosArgs
    {
        public int x;
        public int y;
        public int penSize;
        public Color penColor;

        public MousePosArgs(int _x, int _y, int _penSize, Color c)
        {
            x = _x;
            y = _y;
            penSize = _penSize;
            penColor = c;
        }
    }

    private bool active = false;

    Vector3 pos;
    public float zOffset = 2f;
    public float pushedPos = 2.2f;
    public float normalPos = 2f;

    [SerializeField] private Transform _tip;
    [SerializeField] private Transform _handle;

    [Range(1,15)]
    [SerializeField] public int _penSize = 15;

    private Renderer _renderer;
    private Color[] _colors;
    private float _tipHeight;

    private Whiteboard _whiteboard;

    private RaycastHit _touch;
    
    private Vector2 _touchPos;
    private Vector2 _lastTouchPos;
    private Quaternion _lastTouchRot;
    private bool _touchedLastFrame;

    public float _penImageScale = 25f;
    private KinectHandsEvents handsEvents;

    private InteractionType _interaction;

    public delegate void MarkerAction(MousePosArgs e);
    public event MarkerAction OnMarkerDraw;
    public bool isDrawing = false;

    private void Start()
    {
        _interaction = Utilities._interaction;

        if (_interaction == InteractionType.Kinect)
        {
            handsEvents = GameObject.Find("GestureDetectHandler").GetComponent<KinectHandsEvents>();
        }

        _renderer = _tip.GetComponent<Renderer>();
        _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray();
        _tipHeight = _tip.localScale.y;

        if (_handle.gameObject.GetComponent<MeshRenderer>())
        {
            //Debug.Log("aici");
            _handle.gameObject.GetComponent<MeshRenderer>().enabled = active;
        }

        if (_tip.gameObject.GetComponent<MeshRenderer>())
        {
            _tip.gameObject.GetComponent<MeshRenderer>().enabled = active;
        }
    }

    void Update()
    {
        if (active)
        {
            HideMarkerOnWhiteboardExit();
        }

        RenderMarker(active);

        if (WhiteboardHandler._whiteboardActive)
        {
            if (WhiteboardHandler.SketchMode)
            {
                AdjustPenSize();
            }
            else if (WhiteboardHandler.TerrainMode)
            {
                _penSize = 1;

                _penImageScale = Utilities.remap(_penSize, 2f, 15f, 12.5f, 25f);
            }

            if (isActive())
            {
                ControlMarker(this._interaction);
                FollowMouse(this._interaction);
                _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray();
            }

        }
    }

    private void OnValidate()
    {
        if (_penSize < 2)
        {
            _penSize = 2;
        }

        if (_penSize > 15)
        {
            _penSize = 15;
        }
    }

    private void AdjustPenSize()
    {
        Vector2 scrollData = Input.mouseScrollDelta;
        if (scrollData != Vector2.zero)
        {
            _penSize += (int)scrollData.y;
            _penSize = Mathf.Clamp(_penSize, 1, 15);

            _penImageScale = Utilities.remap(_penSize, 2f, 15f, 12.5f, 25f);
        }
    }

    public void FollowMouse(InteractionType interaction)
    {
        if (interaction == InteractionType.Mouse)
        {
            pos = Input.mousePosition;
            pos.z = zOffset;
            transform.position = Camera.main.ScreenToWorldPoint(pos);
        }
        else if (interaction == InteractionType.Kinect)
        {
            if (handsEvents.GetRAction() && handsEvents.GetLAction()) { return; }
            else if (handsEvents.GetRAction())
            {
                pos = GameObject.Find("Right Hand").transform.position;
                pos.z = zOffset;
                transform.position = Camera.main.ScreenToWorldPoint(pos);
            }
            else if (handsEvents.GetLAction())
            {
                pos = GameObject.Find("Left Hand").transform.position;
                pos.z = zOffset;
                transform.position = Camera.main.ScreenToWorldPoint(pos);
            }
        }
    }

    public void ControlMarker(InteractionType interaction)
    {
        if (interaction == InteractionType.Mouse)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDrawing = true;
                zOffset += .2f;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDrawing = false;
                zOffset -= .2f;
                _whiteboard = null;
                _touchedLastFrame = false;
            }

            if (Input.GetMouseButton(0))
            {
                Draw(Input.mousePosition);
            }
        }
        else if (interaction == InteractionType.Kinect)
        {
            if (handsEvents.GetRAction() && handsEvents.GetLAction()) { return; }
            else if (handsEvents.GetRAction())
            {
                isDrawing = true;
                zOffset = pushedPos;
                pos = GameObject.Find("Right Hand").transform.position;
                Draw(pos);
                return;
            }
            else if (handsEvents.GetLAction())
            {
                isDrawing = true;
                zOffset = pushedPos;
                pos = GameObject.Find("Left Hand").transform.position;
                Draw(pos);
                return;
            }
            else if (!handsEvents.GetRAction() && !handsEvents.GetLAction())
            {
                isDrawing = false;
            }
            zOffset = normalPos;
            _whiteboard = null;
            _touchedLastFrame = false;
        }
    }

    private void Draw(Vector3 positionToDraw)
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(positionToDraw), out _touch))
        {
            if (_touch.transform.CompareTag("Whiteboard"))
            {
                if (_whiteboard == null)
                {
                    _whiteboard = _touch.transform.GetComponent<Whiteboard>();
                }

                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

                var x = (int)(_touchPos.x * _whiteboard.textureSize.x - (_penSize / 2));
                var y = (int)(_touchPos.y * _whiteboard.textureSize.y - (_penSize / 2));

                if (y < 0 || y > _whiteboard.textureSize.y ||
                    x < 0 || x > _whiteboard.textureSize.x)
                {
                    return;
                }


                if (_touchedLastFrame)
                {
                    _whiteboard.texture.SetPixels(x, y, _penSize, _penSize, _colors);

                    for (float step = 0.01f; step < 1.00f; step += 0.01f)
                    {
                        var lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, step);
                        var lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, step);
                        if (WhiteboardHandler.TerrainMode)
                        {
                            OnMarkerDraw(new MousePosArgs(lerpX, lerpY, _penSize, _renderer.material.color));
                        }
                        _whiteboard.texture.SetPixels(lerpX, lerpY, _penSize, _penSize, _colors);
                    }
                    transform.rotation = _lastTouchRot;
                    _whiteboard.texture.Apply();
                }

                _lastTouchPos = new Vector2(x, y);
                _lastTouchRot = transform.rotation;
                _touchedLastFrame = true;
                return;
            }
        }

        _whiteboard = null;
        _touchedLastFrame = false;
    }

    public void HideMarkerOnWhiteboardExit()
    {
        if (!WhiteboardHandler._whiteboardActive)
        {
            RenderMarker(false);
        }
    }

    // Useless now
    public void RandomColorChange()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Color Changed");

            Color _tipCol = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f)
            );
            Debug.Log(_tipCol);
            _renderer.material.color = _tipCol;
        }
    }

    public bool isActive()
    {
        return active;
    }

    public void activateMarker(bool state)
    {
        active = state;
    }

    public float getPenImageScale()
    {
        return _penImageScale;
    }

    public void RenderMarker(bool activeState)
    {
        MeshRenderer handleRenderer = _handle.gameObject.GetComponent<MeshRenderer>();
        MeshRenderer tipRenderer = _tip.gameObject.GetComponent<MeshRenderer>();
        if (handleRenderer)
        {
            _handle.gameObject.GetComponent<MeshRenderer>().enabled = activeState;
        }
        if (tipRenderer)
        {
            _tip.gameObject.GetComponent<MeshRenderer>().enabled = activeState;
        }
        active = activeState;
    }
}
