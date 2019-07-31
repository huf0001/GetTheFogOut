// GENERATED AUTOMATICALLY FROM 'Assets/Inputs/Input.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
                    ""id"": ""7fd0b4f2-7f6b-4a78-9dbe-933c2b5cf472"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": """",
                    ""bindings"": []
                },
                {
                    ""name"": ""Pause"",
                    ""id"": ""67a2325a-d0ca-413f-8a6c-f1d902a4f9c2"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": """",
                    ""bindings"": []
                },
                {
                    ""name"": ""Zoom"",
                    ""id"": ""bae499a2-ed5d-4f68-ab8f-cf98afbde622"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": """",
                    ""bindings"": []
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""289407e2-7e6a-4855-af83-b5198d531329"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Controller"",
                    ""action"": ""Camera Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""5fbd3e36-264a-4e71-8116-280108621eca"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Camera Pan"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": ""up"",
                    ""id"": ""0933cc3f-943b-4c7d-86af-69120a212b2a"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard"",
                    ""action"": ""Camera Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true,
                    ""modifiers"": """"
                },
                {
                    ""name"": ""down"",
                    ""id"": ""eb3b90be-fb50-4592-93eb-920bc52837aa"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard"",
                    ""action"": ""Camera Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true,
                    ""modifiers"": """"
                },
                {
                    ""name"": ""left"",
                    ""id"": ""53584c1b-af36-4141-b29f-6a6c670fd81a"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard"",
                    ""action"": ""Camera Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true,
                    ""modifiers"": """"
                },
                {
                    ""name"": ""right"",
                    ""id"": ""135eac7b-c3d0-464b-8b2b-e5fcb095f449"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard"",
                    ""action"": ""Camera Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""1eb6b520-048a-4ff3-a599-95a0d0a5bdb1"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": "";Controller"",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""fb8792c0-f09d-4096-b4d0-7b310c385342"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": "";Keyboard"",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""5034d39a-6f71-4045-ac95-928bcce35a5e"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard"",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": ""Controller"",
                    ""id"": ""cb7f5ff1-f2b5-4db2-844e-74c9fb1f7bc7"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""dbb70058-e99e-4f83-a208-609012bb76e4"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Controller"",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true,
                    ""modifiers"": """"
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""44da343b-4734-4908-9257-41d21c9acb6b"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Controller"",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true,
                    ""modifiers"": """"
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard"",
            ""basedOn"": """",
            ""bindingGroup"": ""Keyboard"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": true,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Controller"",
            ""basedOn"": """",
            ""bindingGroup"": ""Controller"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": true,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Input Map
        m_InputMap = asset.GetActionMap("Input Map");
        m_InputMap_CameraPan = m_InputMap.GetAction("Camera Pan");
        m_InputMap_Pause = m_InputMap.GetAction("Pause");
        m_InputMap_Zoom = m_InputMap.GetAction("Zoom");
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

    public ReadOnlyArray<InputControlScheme> controlSchemes
    {
        get => asset.controlSchemes;
    }

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
    private InputActionMap m_InputMap;
    private IInputMapActions m_InputMapActionsCallbackInterface;
    private InputAction m_InputMap_CameraPan;
    private InputAction m_InputMap_Pause;
    private InputAction m_InputMap_Zoom;
    public struct InputMapActions
    {
        private NewInputs m_Wrapper;
        public InputMapActions(NewInputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @CameraPan { get { return m_Wrapper.m_InputMap_CameraPan; } }
        public InputAction @Pause { get { return m_Wrapper.m_InputMap_Pause; } }
        public InputAction @Zoom { get { return m_Wrapper.m_InputMap_Zoom; } }
        public InputActionMap Get() { return m_Wrapper.m_InputMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(InputMapActions set) { return set.Get(); }
        public void SetCallbacks(IInputMapActions instance)
        {
            if (m_Wrapper.m_InputMapActionsCallbackInterface != null)
            {
                CameraPan.started -= m_Wrapper.m_InputMapActionsCallbackInterface.OnCameraPan;
                CameraPan.performed -= m_Wrapper.m_InputMapActionsCallbackInterface.OnCameraPan;
                CameraPan.canceled -= m_Wrapper.m_InputMapActionsCallbackInterface.OnCameraPan;
                Pause.started -= m_Wrapper.m_InputMapActionsCallbackInterface.OnPause;
                Pause.performed -= m_Wrapper.m_InputMapActionsCallbackInterface.OnPause;
                Pause.canceled -= m_Wrapper.m_InputMapActionsCallbackInterface.OnPause;
                Zoom.started -= m_Wrapper.m_InputMapActionsCallbackInterface.OnZoom;
                Zoom.performed -= m_Wrapper.m_InputMapActionsCallbackInterface.OnZoom;
                Zoom.canceled -= m_Wrapper.m_InputMapActionsCallbackInterface.OnZoom;
            }
            m_Wrapper.m_InputMapActionsCallbackInterface = instance;
            if (instance != null)
            {
                CameraPan.started += instance.OnCameraPan;
                CameraPan.performed += instance.OnCameraPan;
                CameraPan.canceled += instance.OnCameraPan;
                Pause.started += instance.OnPause;
                Pause.performed += instance.OnPause;
                Pause.canceled += instance.OnPause;
                Zoom.started += instance.OnZoom;
                Zoom.performed += instance.OnZoom;
                Zoom.canceled += instance.OnZoom;
            }
        }
    }
    public InputMapActions @InputMap
    {
        get
        {
            return new InputMapActions(this);
        }
    }
    private int m_KeyboardSchemeIndex = -1;
    public InputControlScheme KeyboardScheme
    {
        get
        {
            if (m_KeyboardSchemeIndex == -1) m_KeyboardSchemeIndex = asset.GetControlSchemeIndex("Keyboard");
            return asset.controlSchemes[m_KeyboardSchemeIndex];
        }
    }
    private int m_ControllerSchemeIndex = -1;
    public InputControlScheme ControllerScheme
    {
        get
        {
            if (m_ControllerSchemeIndex == -1) m_ControllerSchemeIndex = asset.GetControlSchemeIndex("Controller");
            return asset.controlSchemes[m_ControllerSchemeIndex];
        }
    }
    public interface IInputMapActions
    {
        void OnCameraPan(InputAction.CallbackContext context);
        void OnPause(InputAction.CallbackContext context);
        void OnZoom(InputAction.CallbackContext context);
    }
}
