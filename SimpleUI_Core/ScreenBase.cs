using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using SimpleUI.Anim;
using SimpleUI.Interfaces.ObjectState;
using UnityEngine;
using UnityEngine.Serialization;

namespace SimpleUI.Core
{
	/// <summary>
	/// Экран - это самостоятельный скрипт, выполняющий множество функций <br></br>
	/// - Контейнер UIView скриптов с функциями валидации/поиска <br></br>
	/// - Предоставление всех данных для фабрики UIController скриптов <br></br>
	/// - Основная исполнительная единица в ScreensManager <br></br>
	/// - Администрирование SetActive, в том числе и через анимацию <br></br>
	/// </summary>
	[RequireComponent(typeof(Canvas))]
	public abstract class ScreenBase : MonoBehaviour, IConstructed
	{
		[Serializable]
		public struct SpawnViewGroup
		{
			// FormerlySerializedAs не трогать, на данных именах держаться весь UI со старым SimpleUI
			[FormerlySerializedAs("Views")] public UIView[] views;
			[FormerlySerializedAs("Container")] public Transform parent;
		}
		
		// Режим активности экрана если он будет администрироваться менеджером
		[SerializeField] private ScreensManager.ActiveMode managerActiveMode = ScreensManager.ActiveMode.InStackFront;
		
		// Все контроллеры на этом зкране будут создаваться в отдельных суб-контейнерах от этого экрана
		// Перезаписывает похожую переменную в UIView
		[SerializeField] private bool useSubContainers;
		
		[SerializeField, HideInInspector] private Canvas canvas;
		
		[Space]
		[SerializeField, ReadOnly] private UIView[] views = Array.Empty<UIView>();
		
		// FormerlySerializedAs не трогать, на данных именах держаться весь UI со старым SimpleUI
		[FormerlySerializedAs("viewsStorages")]
		[SerializeField] private SpawnViewGroup[] additionalViews = Array.Empty<SpawnViewGroup>();
		
		[SerializeField, ReadOnly] private int viewsCount;
		
		public IReadOnlyList<UIView> Views => views;
		public IReadOnlyList<SpawnViewGroup> AdditionalViews => additionalViews;
		public IEnumerable<UIView> AllViews => views
			.Concat(additionalViews.Select(v => v.views).SelectMany(vs => vs));
		public int ViewsCount => viewsCount;
		
		public ScreensManager.ActiveMode ManagerActiveMode => managerActiveMode;
		public bool UseSubContainers => useSubContainers;
		
		public Canvas Canvas => canvas;
		public Type ScreenType => GetType().UnderlyingSystemType;
		
		public bool IsConstructed { get; private set; }
		private Dictionary<UIView, object> controllers;
		
		public void Construct(Dictionary<UIView, object> newControllers)
		{
			if (IsConstructed) return;
			IsConstructed = true;

			controllers = newControllers;
		}
		
		[SerializeField, ReadOnly] private BaseScreenAnim screenAnim;
		public BaseScreenAnim Anim => screenAnim;
		
		// Ивенты нужны для сценария вне SimpleUI, поэтому такие ивенты вместо SignalBus
		public event Action<ScreenBase> Opened, Closed;
		public event Action<ScreenBase> OpenStarted, OpenEnded;
		public event Action<ScreenBase> CloseStarted, CloseEnded;
		
		private void OnOpenStart() => OpenStarted?.Invoke(this);
		private void OnCloseStart() => CloseStarted?.Invoke(this);
		
		private void OnOpenEnd()
		{
			OpenEnded?.Invoke(this);
			Opened?.Invoke(this);
		}
		private void OnCloseEnd()
		{
			CloseEnded?.Invoke(this);
			Closed?.Invoke(this);
		}
		
		public void SetActive(bool active, bool useAnim = true, bool alwaysAsync = false) 
		{ if (active) Open(useAnim, alwaysAsync); else Close(useAnim, alwaysAsync); }
		public void SetActiveForce(bool active) { if (active) OpenForce(); else CloseForce(); }
		
		/// <summary> Открыть экран, асинхронно если есть анимация, синхронно если нет </summary>
		/// <param name="useAnim">Использовать ли анимацию, если она есть</param>
		/// <param name="alwaysAsync">Операция всегда асинхронная, минимум 1 update frame</param>
		[ContextMenu(nameof(Open), false, 0)]
		public async void Open(bool useAnim = true, bool alwaysAsync = false) => await OpenTask(useAnim, alwaysAsync);
		
		/// <summary> Закрыть экран, асинхронно если есть анимация
		/// (значит экран не закроется моментально!), синхронно если нет </summary>
		/// <param name="useAnim">Использовать ли анимацию, если она есть</param>
		/// <param name="alwaysAsync">Операция всегда асинхронная, минимум 1 update frame</param>
		[ContextMenu(nameof(Close), false, 1)]
		public async void Close(bool useAnim = true, bool alwaysAsync = false) => await CloseTask(useAnim, alwaysAsync);
		
		/// <summary> Открыть экран как задача </summary>
		/// <param name="useAnim">Использовать ли анимацию, если она есть</param>
		/// <param name="alwaysAsync">Операция всегда асинхронная, минимум 1 update frame</param>
		public async UniTask OpenTask(bool useAnim = true, bool alwaysAsync = false)
		{
			if (screenAnim && useAnim)
				await screenAnim.Open(this, OnOpenStart, OnOpenEnd);
			else if (alwaysAsync)
			{
				await UniTask.Yield();
				OpenInternal();
			}
			else OpenInternal();
		}
		/// <summary> Закрыть экран как задача </summary>
		/// <param name="useAnim">Использовать ли анимацию, если она есть</param>
		/// <param name="alwaysAsync">Операция всегда асинхронная, минимум 1 update frame</param>
		public async UniTask CloseTask(bool useAnim = true, bool alwaysAsync = false)
		{
			if (screenAnim && useAnim)
				await screenAnim.Close(this, OnCloseStart, OnCloseEnd);
			else if (alwaysAsync)
			{
				await UniTask.Yield();
				CloseInternal();
			}
			else CloseInternal();
		}

		private void OpenInternal()
		{
			OnOpenStart();
			OpenForce();
			OnOpenEnd();
		}
		private void CloseInternal()
		{
			OnCloseStart();
			CloseForce();
			OnCloseEnd();
		}
		
		public bool ScreenActive { get; private set; }

		public void OnEnable() => ScreenActive = true;
		public void OnDisable() => ScreenActive = false;

		/// <summary> Открыть экран моментально, без анимаций, без ивентов </summary>
		public void OpenForce()
		{
			if (isDestroyed) return;
			
			if (screenAnim) screenAnim.OpenForceCallback(this);
			gameObject.SetActive(true);
		}
		/// <summary> Закрыть экран моментально, без анимаций, без ивентов </summary>
		public void CloseForce()
		{
			if (isDestroyed) return;
			
			gameObject.SetActive(false);
			if (screenAnim) screenAnim.CloseForceCallback(this);
		}
		
		// Самый простой способ проверить, удалён ли объект или нет
		private bool isDestroyed;
		public virtual void OnDestroy()
		{
			isDestroyed = true;
		}

		#region Views
		// Взято из PopupIdentica и адаптировано под SimpleUI
		
		/// <summary> Обычное сравнение типов </summary>
		private static bool EqualsUnderlying(UIView view, Type type) => view.ViewType == type;
		/// <summary> Сравнение типов по наследию </summary>
		private static bool EqualsAssignableUnderlying(UIView view, Type type) => type.IsAssignableFrom(view.ViewType);
		
		public bool Has<TView>(bool assignableComparer = false) where TView : UIView
		{
			return GetInternal<TView>(assignableComparer ? EqualsAssignableUnderlying : EqualsUnderlying, out _);
		}
		public bool Has<TView>(out TView item, bool assignableComparer = false) where TView : UIView
		{
			return GetInternal(assignableComparer ? EqualsAssignableUnderlying : EqualsUnderlying, out item);
		}
		public int Count<TView>(bool assignableComparer = false) where TView : UIView
		{
			return CountAllInternal<TView>(assignableComparer ? EqualsAssignableUnderlying : EqualsUnderlying);
		}
		public int Count<TView>(out ICollection<TView> list, bool assignableComparer = false) where TView : UIView
		{
			list = new List<TView>();
			return GetAllInternal(assignableComparer ? EqualsAssignableUnderlying : EqualsUnderlying, list);
		}
		public int CountNoAlloc<TView>(ICollection<TView> list, bool assignableComparer = false) where TView : UIView
		{
			return GetAllInternal(assignableComparer ? EqualsAssignableUnderlying : EqualsUnderlying, list);
		}
		public TView Get<TView>(bool assignableComparer = false) where TView : UIView
		{
			GetInternal<TView>(assignableComparer ? EqualsAssignableUnderlying : EqualsUnderlying, out var item);
			return item;
		}
		public List<TView> GetAll<TView>(bool assignableComparer = false) where TView : UIView
		{
			var list = new List<TView>();
			GetAllInternal(assignableComparer ? EqualsAssignableUnderlying : EqualsUnderlying, list);
			return list;
		}
		public void GetAllNoAlloc<TView>(ICollection<TView> list, bool assignableComparer = false) where TView : UIView
		{
			GetAllInternal(assignableComparer ? EqualsAssignableUnderlying : EqualsUnderlying, list);
		}

		#region Internal
		private bool GetInternal<TView>(Func<UIView, Type, bool> func, out TView sourceView) where TView : UIView
		{
			var type = typeof(TView);

			foreach (var view in AllViews)
			{
				if (func.Invoke(view, type))
				{
					sourceView = (TView)view;
					return true;
				}
			}
            
			sourceView = null;
			return false;
		}
		private int GetAllInternal<TView>(Func<UIView, Type, bool> func, ICollection<TView> list) where TView : UIView
		{
			var type = typeof(TView);
			list.Clear();

			foreach (var view in AllViews)
			{
				if (func.Invoke(view, type))
					list.Add((TView)view);
			}

			return list.Count;
		}
		private int CountAllInternal<TView>(Func<UIView, Type, bool> func) where TView : UIView
		{
			var type = typeof(TView);
			return AllViews.Count(view => func.Invoke(view, type));
		}
		#endregion

		#endregion

		#region Controller
		// С точки зрения архитектуры, адская некорректная дичь, но для выполнения задач вещь очень удобная
		public bool TryGetController<TView, TController>(bool assignableComparer, out TController controller)
			where TView : UIView where TController : UIController<TView>
		{
			var view = Get<TView>(assignableComparer);
			return TryGetController(view, out controller);
		}
		public bool TryGetController<TView, TController>(TView view, out TController controller)
			where TView : UIView where TController : UIController<TView>
		{
			if (!IsConstructed)
			{
				Debug.LogError("Screen is not constructed (don't resolve in constructor, use IInitialize)", gameObject);
				controller = null;
				return false;
			}
			
			if (controllers.TryGetValue(view, out var abstractController) && abstractController != null)
			{
				controller = (TController)abstractController;
				return true;
			}
			controller = null;
			return false;
		}
		
		public TController GetController<TView, TController>(TView view)
			where TView : UIView where TController : UIController<TView>
		{
			if (!IsConstructed)
			{
				Debug.LogError("Screen is not constructed (don't resolve in constructor, use IInitialize)", gameObject);
				return null;
			}
			
			if (controllers.TryGetValue(view, out var abstractController) && abstractController != null)
				return (TController)abstractController;
			return null;
		}
		#endregion
		
		#region Editor
		[Button(nameof(OnValidate))]
		private void ValidateAllSubContainers()
		{
			FindAllViews(true);
			ResolveViewsSubContainers(true);
			screenAnim = GetComponent<BaseScreenAnim>();
		}

		public virtual void OnValidate()
		{
			FindAllViews();
			ResolveViewsSubContainers();
			screenAnim = GetComponent<BaseScreenAnim>();
		}
		private void FindAllViews(bool debug = false)
		{
			canvas = GetComponent<Canvas>();
			var newViews = GetComponentsInChildren<UIView>(true);
			
			// Данное условие нужно, чтобы лишний раз не вызывать SetDirty
			if (newViews.Equals(views)) return;
			
			views = newViews;
			viewsCount = views.Length + additionalViews.Sum(a => a.views.Length);
			//if (debug) Debug.Log($"Founded {views.Length} views");
		}
		private void ResolveViewsSubContainers(bool debug = false)
		{
			// Проходит по всем типам UIView
			var typeConditions = new Dictionary<Type, bool>();
			foreach (var viewType in views.Select(v => v.ViewType))
			{
				// Если тип уже был найден (то есть в views он повторяется)
				if (typeConditions.TryGetValue(viewType, out var solution))
				{
					// То указать, что он повторяется
					if (!solution)
						typeConditions[viewType] = true;
				}
				// Иначе зарегистрировать уникальный UIView
				else typeConditions.Add(viewType, false);
			}

			// После чего для каждого view можно указать использование суб контейнера
			foreach (var view in views)
				view.UseSubContainer = typeConditions[view.ViewType];

			// Дальше идёт debug секция
			if (!debug) return;
			
			// И для вывода отчёта надо подсчитать, сколько типов
			// уникальны, а сколько повторяются
			int trueCounter = 0, falseCounter = 0;
			foreach (var condition in typeConditions.Values)
			{
				if (condition) trueCounter++;
				else falseCounter++;
			}
			
			Debug.Log($"Validate {trueCounter} repeated components " +
			          $"and {falseCounter} unique components in {views.Length} views");
		}

		#endregion
	}
}