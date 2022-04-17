using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private static bool _isInputEnabled = true;
    private static bool _isSelectionActive = false;
    private Parameters _parameters;
    private TileType _tileType;
    private Coordinate _coordinate;
    private Vector2 _initialPosition;
    private Coroutine _coroutine;
    private SpriteRenderer _renderer;

    private void Awake()
    {
        _parameters = GameManager.Instance.parameters;
        _renderer = transform.GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        EventManager.OnChangeInput += ChangeInput;
    }

    private void OnDisable()
    {
        EventManager.OnChangeInput -= ChangeInput;
    }

    private void ChangeInput(bool isEnabled)
    {
        _isInputEnabled = isEnabled;
    }

    public void OnMouseDown()
    {
        if (!_isInputEnabled || _isSelectionActive) return;

        _isSelectionActive = true;
        Debug.Log($"Pointer down: {gameObject.name}");
        _initialPosition = Input.mousePosition;
        _coroutine = StartCoroutine(CheckSwipe());
    }

    public void OnMouseUp()
    {
        _isSelectionActive = false;
        Debug.Log($"Pointer up: {gameObject.name}");
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
    }

    public void Initialize(Coordinate coordinate, TileType tileType)
    {
        _coordinate = coordinate;
        _tileType = tileType;
        _renderer.sprite = _parameters.SpriteList[_tileType];
    }

    public void UpdateCoordinates(Coordinate coordinate)
    {
        _coordinate = coordinate;
    }
    
    private IEnumerator CheckSwipe()
    {
        var isSwiped = false;
        while (!isSwiped)
        {
            var currentPosition = Input.mousePosition;
            var otherCoordinate = new Coordinate(_coordinate.xCoord, _coordinate.yCoord);
            if (currentPosition.x - _initialPosition.x >= _parameters.swipePixelCount)
            {
                otherCoordinate.yCoord += 1;
                EventManager.SwapTile(_coordinate, otherCoordinate);
                _isInputEnabled = false;
                yield break;
            }
            else if (currentPosition.x - _initialPosition.x <= -_parameters.swipePixelCount)
            {
                otherCoordinate.yCoord -= 1;
                EventManager.SwapTile(_coordinate, otherCoordinate);
                _isInputEnabled = false;
                yield break;
            }
            else if (currentPosition.y - _initialPosition.y >= _parameters.swipePixelCount)
            {
                otherCoordinate.xCoord += 1;
                EventManager.SwapTile(_coordinate, otherCoordinate);
                _isInputEnabled = false;
                yield break;
            }
            else if (currentPosition.y - _initialPosition.y <= -_parameters.swipePixelCount)
            {
                otherCoordinate.xCoord -= 1;
                EventManager.SwapTile(_coordinate, otherCoordinate);
                _isInputEnabled = false;
                yield break;
            }

            yield return null;
        }
    }

    public void Appear()
    {
        Debug.Log($"Appear: ({_coordinate.xCoord}, {_coordinate.yCoord})");
        StartCoroutine(Utils.AnimateScale(transform, Vector3.zero, Vector3.one,
            _parameters.tileAnimationDuration));
    }

    public void Disappear()
    {
        Debug.Log($"Disappear: ({_coordinate.xCoord}, {_coordinate.yCoord})");
        StartCoroutine(Utils.AnimateScale(transform, Vector3.one, Vector3.zero,
            _parameters.tileAnimationDuration));
    }
    public TileType GetTileType()
    {
        return _tileType;
    }

    public void MoveToPosition()
    {
        Debug.Log($"MoveToPosition: ({_coordinate.xCoord}, {_coordinate.yCoord})");
        StartCoroutine(Utils.LocalMove(transform, transform.localPosition, Vector3.zero, 
            _parameters.slideDuration));
    }

    public void MoveToPosition(Vector3 targetPosition)
    {
        Debug.Log($"MoveToPosition: ({_coordinate.xCoord}, {_coordinate.yCoord}) - {targetPosition}");
        StartCoroutine(Utils.GlobalMove(transform, transform.position, targetPosition,
            _parameters.slideDuration));
    }

    public void FailSwap(Coordinate direction)
    {
        StartCoroutine(_FailSwap(direction));
    }

    private IEnumerator _FailSwap(Coordinate direction)
    {
        var initialPosition = transform.localPosition;
        var tileSize = _parameters.tileSize;
        var targetPosition = initialPosition +
                             new Vector3(tileSize * direction.yCoord, tileSize * direction.xCoord, initialPosition.z);
        var elapsedTime = 0f;
        var duration = _parameters.slideDuration;
        while (elapsedTime < duration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            var progress = elapsedTime / duration;
            transform.localPosition = Vector3.Lerp(initialPosition, targetPosition,
                _parameters.failSwapPosition.Evaluate(progress));
        }

        transform.localPosition = initialPosition;
        EventManager.ChangeInput(true);
    }
}