using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public delegate void PlayerLoaded();
public delegate void PlayerExecuteBuildingOperation(PlayerOperation operation);
public delegate void PlayerExecuteMiningOperation(Vector3 mousePosition);
public delegate void PlayerExecuteDeleteBuildingOperation(Vector3 mousePosition);

public class PlayerInteractive : MonoBehaviour
{
    [SerializeField] private GameObject _exitWindow

    [SerializeField] private PlayerOperation _chosenOperation;

    [SerializeField] private float _miningDistance;

    private BuildingCreator _buildingCreator;

    private Tilemap _tilemap;

    public PlayerLoaded OnPlayerLoaded;

    public PlayerExecuteBuildingOperation OnPlayerSetBuilding;

    public PlayerExecuteMiningOperation OnPlayerMining;

    public PlayerExecuteDeleteBuildingOperation OnPlayerDeleteBuilding;

    public ResourceProvider ResourceProviderTarget;

    [SerializeField] private GameObject _produceArea;

    private void OnEnable()
    {
        if (Session.Instance != null)
            Session.Instance.OnSessionInitialized += SetPlayerPosition;

        ResourceProviderTarget = null;

        _buildingCreator = FindObjectOfType<BuildingCreator>();

        OperationButton[] operationButtons = FindObjectsOfType<OperationButton>();

        foreach (OperationButton buildingButton in operationButtons)
            buildingButton.OnOperationButtonIsPressed += RecieveOperationType;
    }

    private void OnDisable()
    {
        if (Session.Instance != null)
            Session.Instance.OnSessionInitialized -= SetPlayerPosition;
    }

    private void Start()
    {
        _tilemap = FindObjectOfType<Tilemap>();

        _chosenOperation = PlayerOperation.Mining;

        ShowExitWindow();

        OnPlayerLoaded?.Invoke();
    }
    
    private void SetPlayerPosition(SessionData data)
    {
        if (data != null)
            gameObject.transform.position = data.PlayerPosition;
    }

    private void RecieveOperationType(PlayerOperation operation) => _chosenOperation = operation;

    private void LeftClick()
    {
        if (!Utils.MouseOverUI())
        {
            if (_chosenOperation != PlayerOperation.Mining)
            {
                OnPlayerSetBuilding?.Invoke(_chosenOperation);
            }
            else
            {
                if (ResourceProviderTarget == null)
                {
                    Vector3 targetPosition = Utils.MousePosOnGridV3(_tilemap);

                    if (Vector3.Distance(transform.position, targetPosition) < _miningDistance)
                        OnPlayerMining?.Invoke(targetPosition);
                }
            }
        }
    }
    private void RightClick()
    {
        if (ResourceProviderTarget == null)
        {
            OnPlayerDeleteBuilding?.Invoke(Utils.MousePosOnGridV3(_tilemap));
        }
        else
        {
            ResourceProviderTarget.StopAllCoroutines();
            ResourceProviderTarget = null;
        }
    }

    private void ShowExitWindow()
    {
        _exitWindow.SetActive(_exitWindow.activeInHierarchy == true ? false : true);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            LeftClick();
        if (Input.GetMouseButtonDown(1))
            RightClick();
        if (Input.GetKeyDown(KeyCode.Escape))
            ShowExitWindow();

        if(_produceArea != null)
        {
            _produceArea.transform.position = Utils.MousePosOnGridV3(_tilemap);
        }
    }

    public void ShowProduceArea(GameObject area)
    {
        if (_chosenOperation != PlayerOperation.Mining)
        {
            if (_produceArea != null)
                Destroy(_produceArea);

            _produceArea = Instantiate(area, Utils.MousePosOnGridV3(_tilemap), Quaternion.identity);
        }
        else
        {
            _produceArea = null;
        }
    }
}
