using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Snake : MonoBehaviour
{
    [SerializeField] private GameObject _snakeBodyPrefab;
    [SerializeField] private Transform _foodTransform;
    [SerializeField] private List<Collider2D> _wallColliderList;

    private bool _canSetMoveDirection = true;
    private Vector2 _moveDirection;
    private List<Vector2> _movePositionList = new List<Vector2>();
    private Transform _snakeHeadTransform;
    private List<Transform> _snakeBodyTransformList = new List<Transform>();
    
    private void Awake()
    {
        _snakeHeadTransform = transform.GetChild(0);
    }

    private void Start()
    {
        SpawnFood();
        InvokeRepeating(nameof(HandleMovement), 0, .1f);
    }

    private void OnMove(InputValue input)
    {
        if (!_canSetMoveDirection)
        {
            return;
        }

        var value = input.Get();
        
        if (value is Vector2 vector)
        {
            if (_movePositionList.Count == 0 ||
	            vector == Vector2.up && _moveDirection != Vector2.down ||
                vector == Vector2.down && _moveDirection != Vector2.up ||
                vector == Vector2.left && _moveDirection != Vector2.right ||
                vector == Vector2.right && _moveDirection != Vector2.left)
            {
                _moveDirection = vector;
                _canSetMoveDirection = false;
            }
        }
    }

    private void OnEscapePress()
    {
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
    }

    private void HandleMovement()
    {
        if (_moveDirection == Vector2.zero)
        {
            return;
        }

        _movePositionList.Insert(0, _snakeHeadTransform.position);
        _snakeHeadTransform.position += (Vector3)_moveDirection;

        if (_movePositionList.Contains(_snakeHeadTransform.position) || CheckWallCollision(_snakeHeadTransform.position))
        {
            SceneManager.LoadScene(0);
        }

        if (_snakeHeadTransform.position == _foodTransform.position)
        {
            _snakeBodyTransformList.Add(Instantiate(_snakeBodyPrefab, transform).transform);
            SpawnFood();
        }

        if (_movePositionList.Count > _snakeBodyTransformList.Count)
        {
            _movePositionList.RemoveAt(_movePositionList.Count - 1);
        }

        for (var i = 0; i < _snakeBodyTransformList.Count; i++)
        {
            _snakeBodyTransformList[i].position = _movePositionList[i];
        }

        _canSetMoveDirection = true;
    }

    private void SpawnFood()
    {
        var fullSnakePositionList = new List<Vector2>(_movePositionList) { _snakeHeadTransform.position };

        do
        {
            _foodTransform.position = new Vector2(Random.Range(-18, 18), Random.Range(-10, 10));
        } 
        while (fullSnakePositionList.Contains(_foodTransform.position) || CheckWallCollision(_foodTransform.position));
    }
    
    private bool CheckWallCollision(Vector2 point)
    {
        return _wallColliderList.Any(x => x.bounds.Contains(point));
    }
}