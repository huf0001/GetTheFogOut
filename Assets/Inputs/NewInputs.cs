// GENERATED AUTOMATICALLY FROM 'Assets/Inputs/Input.inputactions'

using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class NewInputs : IInputActionCollection
{
    private InputActionAsset asset;
    public NewInputs()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Input"",
    ""maps"": [
        {
            ""name"": ""Input Map"",
            ""id"": ""f38af95d-6c5e-4b5c-8cf6-f2a9f8b738fe"",
            ""actions"": [
                {
                    ""name"": ""Camera Pan"",
                    ""type"": ""Button"",
                    ""id"": ""7fd0b4f2-7f6b-4a78-9dbe-933c2b5cf472"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Camera Pan Keyboard"",
                    ""type"": ""Button"",
                    ""id"": ""76613869-5093-4486-9db5-bdb35be21341"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""289407e2-7e6a-4855-af83-b5198d531329"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Camera Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""5fbd3e36-264a-4e71-8116-280108621eca"",
                    ""path"": ""2DVector"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Camera Pan Keyboard"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""0933cc3f-943b-4c7d-86af-69120a212b2a"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Camera Pan Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""eb3b90be-fb50-4592-93eb-920bc52837aa"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Camera Pan Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""53584c1b-af36-4141-b29f-6a6c670fd81a"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Camera Pan Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""135eac7b-c3d0-464b-8b2b-e5fcb095f449"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Camera Pan Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Input Map
        m_InputMap = asset.GetActionMap("Input Map");
        m_InputMap_CameraPan = m_InputMap.GetAction("Camera Pan");
        m_InputMap_CameraPanKeyboard = m_InputMap.GetAction("Camera Pan Keyboard");
    }

    ~NewInputs()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Input Map
    private readonly InputActionMap m_InputMap;
    private IInputMapActions m_InputMapActionsCallbackInterface;
    private readonly InputAction m_InputMap_CameraPan;
    private readonly InputAction m_InputMap_CameraPanKeyboard;
    public struct InputMapActions
    {
        private NewInputs m_Wrapper;
        public InputMapActions(NewInputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @CameraPan => m_Wrapper.m_InputMap_CameraPan;
        public InputAction @CameraPanKeyboard => m_Wrapper.m_InputMap_CameraPanKeyboard;
        public InputActionMap Get() { return m_Wrapper.m_InputMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(InputMapActions set) { return set.Get(); }
        public void SetCallbacks(IInputMapActions instance)
        {
            if (m_Wrapper.m_InputMapActionsCallbackInterface != null)
            {
                CameraPan.started -= m_Wrapper.m_InputMapActionsCallbackInterface.OnCameraPan;
                CameraPan.performed -= m_Wrapper.m_InputMapActionsCallbackInterface.OnCameraPan;
                CameraPan.canceled -= m_Wrapper.m_InputMapActionsCallbackInterface.OnCameraPan;
                CameraPanKeyboard.started -= m_Wrapper.m_InputMapActionsCallbackInterface.OnCameraPanKeyboard;
                CameraPanKeyboard.performed -= m_Wrapper.m_InputMapActionsCallbackInterface.OnCameraPanKeyboard;
                CameraPanKeyboard.canceled -= m_Wrapper.m_InputMapActionsCallbackInterface.OnCameraPanKeyboard;
            }
            m_Wrapper.m_InputMapActionsCallbackInterface = instance;
            if (instance != null)
            {
                CameraPan.started += instance.OnCameraPan;
                CameraPan.performed += instance.OnCameraPan;
                CameraPan.canceled += instance.OnCameraPan;
                CameraPanKeyboard.started += instance.OnCameraPanKeyboard;
                CameraPanKeyboard.performed += instance.OnCameraPanKeyboard;
                CameraPanKeyboard.canceled += instance.OnCameraPanKeyboard;
            }
        }
    }
    public InputMapActions @InputMap => new InputMapActions(this);
    public interface IInputMapActions
    {
        void OnCameraPan(InputAction.CallbackContext context);
        void OnCameraPanKeyboard(InputAction.CallbackContext context);
    }
}
