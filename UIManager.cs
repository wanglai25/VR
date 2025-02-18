using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;
using TMPro; // If using TextMeshPro

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject Hand;
    private SteamVR_Action_Boolean toggleMenuAction;
    [SerializeField] private Camera vrCamera; // Reference to the VR Camera

    public GameObject mainMenuPanel;
    public GameObject projectOverviewPanel;
    public GameObject logBookPanel;
    public GameObject strataInformationPanel;
    public GameObject tutorialsPanel;
    public GameObject KeyboardPrefab;
    public GameObject MalletPrefab;
    public TextMeshProUGUI strataDisplayText; // Reference to the TextMeshPro Text component
    public GameObject rightLineStartPoint; // 右线起点的引用
    public GameObject leftLineStartPoint; // 左线起点的引用
    private bool onRightLine = true; // 玩家一开始在右线上
    private Transform player; // 用于保存玩家对象的Transform引用

    public Transform insidePosition; // Position inside the tunnel
    public Transform outsidePosition; // Position outside at the 'OutSide' teleport point
    private CharacterController playerCharacterController; // Character Controller component
    private bool isOutside = false; // Current perspective state

    // public VRKeyboardManager keyboardManager; // Reference to the VRKeyboardManager

    private void Awake()
    {
        toggleMenuAction = SteamVR_Actions.default_ViewMainMenu;
        toggleMenuAction.AddOnStateDownListener(ToggleMainMenu, SteamVR_Input_Sources.Any);
    }

    private void Start()
    {
        Debug.Log("UIManager Start");

        // 验证 Outside Position 引用
        if (outsidePosition != null)
        {
            Debug.Log($"Outside Position is set to: {outsidePosition.position}");
        }
        else
        {
            Debug.LogError("Outside Position reference is not set in Inspector!");
        }

        // 验证 Inside Position 引用
        if (insidePosition != null)
        {
            Debug.Log($"Inside Position is set to: {insidePosition.position}");
        }
        else
        {
            Debug.LogError("Inside Position reference is not set in Inspector!");
        }

        // Find and verify player
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player != null)
        {
            playerCharacterController = player.GetComponent<CharacterController>();
            Debug.Log($"Initial Player Position: {player.position}");
        }
        else
        {
            Debug.LogError("Player not found! Check if Player tag is set.");
        }

        // Get and verify PlayerMovementController
        var playerMovement = player.GetComponent<PlayerMovementController>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovementController not found on player!");
            return;
        }

        // Initialize other components
        if (Hand != null)
        {
            var laserPointer = Hand.GetComponent<SteamVR_LaserPointer>();
            if (laserPointer != null)
            {
                laserPointer.PointerIn += OnPointerIn;
                laserPointer.PointerOut += OnPointerOut;
                laserPointer.PointerClick += OnPointerClick;
            }
            else
            {
                Debug.LogError("SteamVR_LaserPointer not found on Hand!");
            }
        }
        else
        {
            Debug.LogError("Hand reference not set!");
        }

        HideAllPanels();
    }

    private void Update()
    {
        if (strataDisplayText.gameObject.activeSelf)
        {
            // Adjust the text to face the VR camera
            strataDisplayText.transform.LookAt(vrCamera.transform);
            strataDisplayText.transform.Rotate(0, 180, 0); // Correct orientation

            // Position the text in front of the camera
            Vector3 offset = vrCamera.transform.up * -0.7f; // Raise the text up a bit if it's too low
            Vector3 textPosition = vrCamera.transform.position + vrCamera.transform.forward * 3.0f + offset; // 3 meters in front of the camera
            strataDisplayText.transform.position = textPosition;
        }
    }


    private void OnDestroy()
    {
        if (toggleMenuAction != null)
        {
            toggleMenuAction.RemoveOnStateDownListener(ToggleMainMenu, SteamVR_Input_Sources.Any);
        }
    }

    public void SwitchLine()
    {
        if (leftLineStartPoint == null || rightLineStartPoint == null)
        {
            Debug.LogError("Line start points not assigned!");
            return;
        }

        if (playerCharacterController != null)
        {
            Vector3 newPosition = onRightLine ?
                leftLineStartPoint.transform.position :
                rightLineStartPoint.transform.position;

            playerCharacterController.enabled = false;
            player.position = newPosition;
            playerCharacterController.enabled = true;
        }
        else
        {
            Debug.LogError("CharacterController not found!");
        }

        onRightLine = !onRightLine;
    }

    public void SwitchPerspective()
    {
        Debug.Log("SwitchPerspective called");

        if (insidePosition == null || outsidePosition == null)
        {
            Debug.LogError($"Position references not set! Inside: {insidePosition}, Outside: {outsidePosition}");
            return;
        }

        if (playerCharacterController == null)
        {
            Debug.LogError("CharacterController not found!");
            return;
        }

        PlayerMovementController playerMovement = player.GetComponent<PlayerMovementController>();

        if (playerMovement != null)
        {
            // 暂时禁用 CharacterController
            playerCharacterController.enabled = false;

            if (isOutside)
            {
                Debug.Log($"Switching to inside position: {insidePosition.position}");
                // 同时设置 player 和 CharacterController 的位置
                player.position = insidePosition.position;
                player.rotation = insidePosition.rotation;
                playerCharacterController.transform.position = insidePosition.position;
                playerCharacterController.transform.rotation = insidePosition.rotation;
                playerMovement.SetCanMoveVertically(false);
            }
            else
            {
                Debug.Log($"Switching to outside position: {outsidePosition.position}");
                // 同时设置 player 和 CharacterController 的位置
                player.position = outsidePosition.position;
                player.rotation = outsidePosition.rotation;
                playerCharacterController.transform.position = outsidePosition.position;
                playerCharacterController.transform.rotation = outsidePosition.rotation;
                playerMovement.SetCanMoveVertically(true);
            }

            // 重新启用 CharacterController
            playerCharacterController.enabled = true;

            // 切换状态
            isOutside = !isOutside;

            // 打印最终位置进行验证
            Debug.Log($"Final position - Player: {player.position}, CharacterController: {playerCharacterController.transform.position}");
        }
        else
        {
            Debug.LogError("PlayerMovementController component not found on the player object.");
        }
    }




    private void ToggleMainMenu(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (mainMenuPanel.activeSelf)
        {
            HideAllPanels();
        }
        else
        {
            HideAllPanels();
            mainMenuPanel.SetActive(true);
        }
    }

    private void OnPointerClick(object sender, PointerEventArgs e)
    {
        if (e.target != null && e.target.name.StartsWith("BaseCube"))
        {
            DisplayStrataType(e.target.gameObject);
        }

        switch (e.target.name)
        {
            case "ProjectOverviewButton":
                ShowPanel(projectOverviewPanel);
                break;
            case "LogBookButton":
                ShowPanel(logBookPanel);
                KeyboardPrefab.SetActive(true);
                MalletPrefab.SetActive(true);
                break;
            case "StrataInformationButton":
                ShowPanel(strataInformationPanel);
                break;
            case "TutorialsButton":
                ShowPanel(tutorialsPanel);
                break;
            case "SwitchLineButton": // 确保这是您的切换线路按钮的名称
                SwitchLine(); // 调用瞬移的方法
                break;
            case "SwitchPerspectiveButton":
                SwitchPerspective(); // Call the new perspective switch method
                break;
            default:
                Debug.Log("Unknown button clicked: " + e.target.name);
                break;
        }
    }

    private void OnPointerOut(object sender, PointerEventArgs e)
    {
        Debug.Log("Laser has exited " + e.target.name);
    }

    private void OnPointerIn(object sender, PointerEventArgs e)
    {
        Debug.Log("Laser has entered " + e.target.name);
    }

    private void ShowPanel(GameObject panel)
    {
        HideAllPanels();
        panel.SetActive(true);
    }

    private void HideAllPanels()
    {
        mainMenuPanel.SetActive(false);
        projectOverviewPanel.SetActive(false);
        logBookPanel.SetActive(false);
        strataInformationPanel.SetActive(false);
        tutorialsPanel.SetActive(false);
        KeyboardPrefab.SetActive(false);
        MalletPrefab.SetActive(false);
        strataDisplayText.gameObject.SetActive(false);
    }

    private void DisplayStrataType(GameObject cube)
    {
        var materialName = cube.GetComponent<Renderer>().material.name.Split(' ')[0];

        string strataType = materialName switch
        {
            "Material_1" => "Miscellaneous fill",
            "Material_2" => "Plain fill",
            "Material_3" => "Silty clay",
            "Material_4" => "Silt",
            "Material_5" => "Argillaceous silt",
            "Material_6" => "Fine sand",
            "Material_7" => "Silty sand",
            "Material_8" => "Gravelly sand",
            _ => "Unknown material"
        };

        //strataDisplayText.text = "The strata type of this location is: " + strataType;
        strataDisplayText.text = strataType;
        strataDisplayText.gameObject.SetActive(true);
        Invoke("HideStrataDisplay", 6.0f); // Hide after 5 seconds
    }

    private void HideStrataDisplay()
    {
        strataDisplayText.gameObject.SetActive(false);
    }
}