using System;
using NaughtyAttributes;
using SimpleUI.Interfaces.ObjectState;
using UnityEngine;

namespace SimpleUI.Core
{
    /// <summary>
    /// Основная единица хранения данных внутри Screen.<br></br>
    /// Выполняет роль связи между Unity и zenject контекстами.<br></br>
    /// Может иметь контроллер, но нужно явно указать его тип
    /// </summary>
    public abstract class UIView : MonoBehaviour, IConstructed
    {
        // Контроллер этого view создаётся в отдельном sub container от экрана, в котором он лежит
        [SerializeField, HideInInspector] private bool useSubContainer;
        [SerializeField, ReadOnly] private ScreenBase screen;
        
        public bool IsConstructed { get; private set; }
        
        public bool UseSubContainer
        {
            get => useSubContainer;
            set => useSubContainer = value;
        }
        
        public ScreensManager Manager { get; private set; }
        public ScreenBase Screen
        {
            get => screen;
            set => screen = value;
        }
        
        public Type ViewType => GetType().UnderlyingSystemType;
        public abstract Type GetControllerType();

        public virtual void OnValidate()
        {
            screen = GetComponentInParent<ScreenBase>();
        }
        public virtual void Construct(ScreensManager manager, ScreenBase screenBase)
        {
            if (IsConstructed) return;
            IsConstructed = true;
            
            Manager = manager;
            screen = screenBase;
        }
    }
}