using System;
using System.Collections.Generic;
using NaughtyAttributes;
using SimpleUI.Extensions;
using SimpleUI.Interfaces;
using SimpleUI.Interfaces.ObjectState;
using SimpleUI.Interfaces.Zenject;
using SimpleUI.Models.Convert;
using SimpleUI.Scriptables.Core;
using SimpleUI.Scriptables.Manager;
using SimpleUI.Services;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace SimpleUI.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks,        false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
    /// <summary>
    /// Реализация менеджера экранов для MonoBehavior и zenject контейнера <br></br>
    /// Можно без проблем создать и удалить в любой момент, где будет работать Inject параметров из zenject
    /// </summary>
    public class ScreensManagerInstance : MonoBehaviour, IScreenStorageSource, IZenjectKernelEvents, IConstructed
    {
        public ScreensManager Manager { get; private set; }
        public IKernelUI KernelUI { get; private set; }
        public ScreensContainer Container { get; private set; }
        public bool IsConstructed { get; private set; }
        public bool IsInitialized { get; private set; }
        
        public event Action OnInitialize;
        public event Action OnDispose;
        public event Action OnLateDispose;
        public event Action OnTick;
        public event Action OnFixedTick;
        public event Action OnLateTick;
        

        [SerializeField, Expandable] private BaseScreenStorage storage;
        [SerializeField, ReadOnly] private ScreenBase[] spawnedScreens = Array.Empty<ScreenBase>();

        [Header("Parameters")]
        [SerializeField] private ScreensManagerConfig managerSettings = new();
        [SerializeField] private RenderMode renderMode = RenderMode.WorldSpace;

        private bool IsScreenSpaceOverlay => renderMode.IsScreenSpaceOverlay();
        [FormerlySerializedAs("eventCamera")] // Не трогать
        [HideIf(nameof(IsScreenSpaceOverlay))]
        [SerializeField] private Camera worldCamera;

        private bool IsScreenSpaceCamera => renderMode.IsScreenSpaceCamera();
        [ShowIf(nameof(IsScreenSpaceCamera))]
        [SerializeField] private CanvasToCameraModel cameraModel;

        private bool IsWorldSpace => renderMode.IsWorldSpace();
        [ShowIf(nameof(IsWorldSpace))]
        [SerializeField] private CanvasToWorldModel worldModel;
        
        public Transform Parent => transform;
        public IScreensFactory Factory { get; private set; }
        
        public BaseScreenStorage Storage
        {
            get => storage;
            set => storage = value;
        }
        public ScreenBase[] SpawnedScreens
        {
            get => spawnedScreens;
            set => spawnedScreens = value;
        }

        private IZenjectKernel kernel;
        
        /// <summary> Конструктор, вызывается до Awake, требует только container </summary>
        [Inject]
        public void Construct(DiContainer container, [InjectOptional] SimpleUISettings settings)
        {
            if (IsConstructed)
            {
                Debug.LogError($"Instance is already constructed", gameObject);
                return;
            }

            var kernelUISettings = settings ? settings.LocalKernelUI : SimpleUISettings.DefaultLocalKernelUI;
            var screenStorageSettings = settings ? settings.ScreenStorage : new ScreenStorageConfig();
            
            var kernelUI = new KernelUI(kernelUISettings);
            KernelUI = kernelUI; kernel = kernelUI;
            
            Factory = new ScreensFactory(container, KernelUI);
            Manager = new ScreensManager(managerSettings);
            Container = new ScreensContainer(Manager, screenStorageSettings);
            
            IsConstructed = true;
        }
        
        public void Start()
        {
            if (!IsConstructed) return;
            if (IsInitialized) return;
            
            Container.Add(this);
            foreach (var screen in Container.Get(this))
                ConvertScreen(screen);
            
            kernel.Construct();
            kernel.Initialize();
            OnInitialize?.Invoke();
            IsInitialized = true;
        }
        public void OnDestroy()
        {
            if (!IsConstructed) return;
            if (!IsInitialized) return;
            
            kernel.Dispose();
            OnDispose?.Invoke();
            kernel.LateDispose();
            OnLateDispose?.Invoke();
            
            Container.Remove(this);
            IsInitialized = false;
        }

        private void Update()
        {
            if (!IsConstructed) return;
            kernel.Tick();
            OnTick?.Invoke();
        }
        private void FixedUpdate()
        {
            if (!IsConstructed) return;
            kernel.FixedTick();
            OnFixedTick?.Invoke();
        }
        private void LateUpdate()
        {
            if (!IsConstructed) return;
            kernel.LateTick();
            OnLateTick?.Invoke();
        }
        
        private void ConvertScreen(ScreenBase screen)
        {
            screen.transform.SetParent(transform);
            
            switch (renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    screen.Canvas.ConvertToOverlayMode();
                    break;
                case RenderMode.ScreenSpaceCamera:
                    screen.Canvas.ConvertToCameraMode(cameraModel, worldCamera);
                    break;
                case RenderMode.WorldSpace:
                    screen.Canvas.ConvertToWorldMode(worldModel, worldCamera);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        [Button]
        private void OnValidate()
        {
            spawnedScreens = GetComponentsInChildren<ScreenBase>(true);
            
            if (this.InProjectContext())
            {
                Debug.LogError($"Don't declare {nameof(ScreensManagerInstance)}" +
                               $" in {nameof(ProjectContext)}, project/local system" +
                               $"has been deprecated since v5 version");
            }
        }

        [ContextMenu("Convert/" + nameof(ConvertExtensions.MoveStorageToSpawned))]
        private void MoveStorageToSpawned()
        {
            ConvertExtensions.MoveStorageToSpawned(this);
        }
        [ContextMenu("Convert/" + nameof(ConvertExtensions.InstanceToInstaller))]
        private void InstanceToInstaller()
        {
            ConvertExtensions.InstanceToInstaller(this);
        }
    }
}