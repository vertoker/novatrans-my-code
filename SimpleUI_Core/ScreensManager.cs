using System;
using System.Collections.Generic;
using System.Linq;
using SimpleUI.Anim;
using SimpleUI.Extensions;
using SimpleUI.Interfaces.Manager;
using SimpleUI.Interfaces.Manager.Enumerables;
using SimpleUI.Scriptables.Manager;
using UnityEngine;

namespace SimpleUI.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks,        false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
	/// <summary>
	/// Менеджер экранов отвечает за администрирование Screen объектов, предоставляя
	/// простое API для работы с экранами <br></br>
	/// Как его создать: <br></br>
	/// - Через обычный конструктор, он может существовать полностью самостоятельно <br></br>
	/// - Project Manager - создаётся в SimpleUIInstaller, отвечает за все экраны в глобальном контейнере <br></br>
	/// - MonoBehavior Manager - создаётся в ScreensManagerInstance и отвечает за экраны в контексте одного объекта
	/// </summary>
	public class ScreensManager : IScreensManager, 
		IScreensManagerEditable, IScreensManagerProfiling,
		IScreensManagerEnumerable, IScreensManagerStack, IScreensManagerList
	{
		/// <summary> Список всех зарегистрированных экранов </summary>
		private readonly List<ScreenBase> screensList = new();
		/// <summary> Список всех активных экранов, состоит из списка существующих </summary>
		// ВАЖНО: по своему поведению это Stack, но из-за необычных модификаций он является List
		private readonly List<ScreenBase> screensStack = new();
		
		/// <summary> Внутренний счётчик для layer у canvas объектов (Config.UseInternalSortingLayer) </summary>
		private int counterOrderInLayer;
		
		/// <summary> Режимы активности экрана, если он управляется ScreensManager </summary>
		public enum ActiveMode
		{
			/// <summary> Активен если находится в Stack и первым в очереди </summary>
			InStackFront = 0,
			/// <summary> Активен если находится в Stack </summary>
			InStack = 1, 
			/// <summary> Активен если находится в List </summary>
			Always = 2, 
		}

		public IEnumerable<ScreenBase> AllScreens => ScreensList.Concat(ScreensStack);
		public int AllCount => ListCount + StackCount;
		public bool IsEmpty => ListIsEmpty && StackIsEmpty;
		
		public IReadOnlyList<ScreenBase> ScreensList => screensList;
		public int ListCount => screensList.Count;
		public bool ListIsEmpty => ListCount == 0;
		
		public IEnumerable<ScreenBase> ScreensStack => screensStack;
		public int StackCount => screensStack.Count;
		public bool StackIsEmpty => StackCount == 0;

		public ScreensManagerConfig Config { get; }

		public ScreensManager(ScreensManagerConfig config)
		{
			Config = config;
			TryPrintManager();
		}

		/// <summary> Добавляет экран в List </summary>
		public void AddScreen(ScreenBase screen)
		{
			screensList.Add(screen);
			
			// Всегда показывать экран если стоит Always
			if (screen.ManagerActiveMode == ActiveMode.Always)
				screen.OpenForce();
			// Закрыть экран при добавлении
			else if (Config.CloseScreenOnAdd)
				screen.CloseForce();
		}
		public void RemoveScreen(ScreenBase screen)
		{
			// Закрывает экран (не всегда) и удаляет экран из Stack
			InternalClose(screen, AnimParameters.NoAnim);
			
			// Удаляет из List
			screensList.Remove(screen);
			// И только при этом случае закрывается экран с Always
			if (screen.ManagerActiveMode == ActiveMode.Always)
				screen.CloseForce();
		}
		
		public bool Contains<TScreen>() where TScreen : ScreenBase => Contains(typeof(TScreen));
		public bool Contains(Type screenType)
		{
			if (IsEmpty) return false;
			var screen = FindInStack(screenType);
			return screen;
		}

		public bool InStack<TScreen>() where TScreen : ScreenBase => InStack(typeof(TScreen));
		public bool InStack(Type screenType)
		{
			if (StackIsEmpty) return false;
			var screen = FindInStack(screenType);
			return screen;
		}
		
		public bool InList<TScreen>() where TScreen : ScreenBase => InList(typeof(TScreen));
		public bool InList(Type screenType)
		{
			if (ListIsEmpty) return false;
			var screen = FindInList(screenType);
			return screen;
		}
		
		public bool IsOpened<TScreen>() where TScreen : ScreenBase => IsOpened(typeof(TScreen));
		public bool IsOpened(Type screenType)
		{
			if (StackIsEmpty) return false;
			var screen = screensStack.Last();
			return screen.ScreenType == screenType;
		}
		
		public void Open<TScreen>(AnimParameters parameters = null) 
			where TScreen : ScreenBase => Open(typeof(TScreen), parameters);
		public void Open(Type screenType, AnimParameters parameters = null)
		{
			var screen = FindInList(screenType);
			if (AssertEmptyScreen(screen, NullScreenInListMessage)) return;
			
			ValidateNull(ref parameters);
			InternalOpen(screen, parameters);
		}

		public void Close<TScreen>(AnimParameters parameters = null) 
			where TScreen : ScreenBase => Close(typeof(TScreen), parameters);
		public void Close(Type screenType, AnimParameters parameters = null)
		{
			var screen = FindInList(screenType);
			if (AssertEmptyScreen(screen, NullScreenInListMessage)) return;
			
			ValidateNull(ref parameters);
			InternalClose(screen, parameters);
		}
		
		public void BackUntil<TScreen>(bool include = false, AnimParameters parameters = null) 
			where TScreen : ScreenBase => BackUntil(typeof(TScreen), include, parameters);
		public void BackUntil(Type screenType, bool include = false, AnimParameters parameters = null)
		{
			ValidateNull(ref parameters);
			while (!StackIsEmpty && screensStack.Last().ScreenType != screenType)
				InternalBack(parameters);
			if (include && !StackIsEmpty)
				InternalBack(parameters);
		}

		public void Back(AnimParameters parameters = null)
		{
			ValidateNull(ref parameters);
			InternalBack(parameters);
		}
		public void Reset(AnimParameters parameters = null)
		{
			ValidateNull(ref parameters);
			while (!StackIsEmpty)
				InternalBack(parameters);
		}
		
		public void SetParent<TScreen>(Transform parent) where TScreen : ScreenBase
			=> SetParent(typeof(TScreen), parent);
		public void SetParent(Type screenType, Transform parent)
		{
			var screen = FindInList(screenType);
			screen.transform.SetParent(parent, false);
		}

		public TScreen Find<TScreen>() where TScreen : ScreenBase => (TScreen)Find(typeof(TScreen));
		public ScreenBase Find(Type screenType)
		{
			var screen = FindInStack(screenType);
			return screen ? screen : FindInList(screenType);
		}
		public TScreen FindInList<TScreen>() where TScreen : ScreenBase => (TScreen)FindInList(typeof(TScreen));
		public ScreenBase FindInList(Type screenType) => screensList.FirstOrDefault(s => s && s.ScreenType == screenType);
		public TScreen FindInStack<TScreen>() where TScreen : ScreenBase => (TScreen)FindInStack(typeof(TScreen));
		public ScreenBase FindInStack(Type screenType) => screensStack.FirstOrDefault(s => s && s.ScreenType == screenType);

		private const string NullScreenInListMessage = "Can't find screen in list";
		private static bool AssertEmptyScreen(ScreenBase screen, string message)
		{
			if (!screen)
			{
				Debug.LogError(message);
				return true;
			}
			return false;
		}
		private static void ValidateNull(ref AnimParameters parameters)
		{
			parameters ??= AnimParameters.Default;
		}
		
		private void InternalOpen(ScreenBase toOpenScreen, AnimParameters parameters)
		{
			if (Config.UseInternalSortingLayer)
			{
				toOpenScreen.Canvas.sortingOrder = counterOrderInLayer;
				counterOrderInLayer++;
			}
			
			if (!StackIsEmpty)
			{
				var toCloseScreen = screensStack.Last();
				
				// Закрываем front экран (только если он StackFront, так как он больше не front)
				if (toCloseScreen.ManagerActiveMode == ActiveMode.InStackFront)
					toCloseScreen.Close(parameters.useAnimClose);
			}
			
			// Открываем новый текущий экран (если он не активен)
			// А активен он только если Always
			if (toOpenScreen.ManagerActiveMode != ActiveMode.Always)
				toOpenScreen.Open(parameters.useAnimOpen);
			
			screensStack.Add(toOpenScreen);
			
			TryPrintManager();
		}
		
		private void InternalClose(ScreenBase toCloseScreen, AnimParameters parameters)
		{
			if (StackIsEmpty) return;
			
			var screenIndex = screensStack.IndexOf(toCloseScreen);
			if (screenIndex == -1) return;
			
			var stackCount = StackCount;

			// Удаляем из стэка (по индексу)
			screensStack.RemoveAt(screenIndex);
			// И закрываем (только если он не Always)
			if (toCloseScreen.ManagerActiveMode != ActiveMode.Always)
				toCloseScreen.Close(parameters.useAnimClose);
			
			if (Config.UseInternalSortingLayer)
			{
				InternalRecalculateLayers(screenIndex);
				counterOrderInLayer--;
			}
			
			// Если экран стоял самым первым, то его надо открыть
			if (screenIndex == stackCount)
				InternalTryOpenLast(parameters);
			
			TryPrintManager();
		}

		private void InternalBack(AnimParameters parameters)
		{
			if (StackIsEmpty) return;

			if (Config.UseInternalSortingLayer)
				counterOrderInLayer--;
			
			// Удаляем из стэка (front)
			var toCloseScreen = screensStack.Pop();
			// И закрываем (только если он не Always)
			if (toCloseScreen.ManagerActiveMode != ActiveMode.Always)
				toCloseScreen.Close(parameters.useAnimClose);
			
			InternalTryOpenLast(parameters);
			TryPrintManager();
		}

		/// <summary>
		/// Открываем новый текущий экран (возможно)
		/// </summary>
		private void InternalTryOpenLast(AnimParameters parameters)
		{
			if (StackIsEmpty) return;
			
			var toOpenScreen = screensStack.Last();
			// Открывает последний экран (только при StackFront, так как он становиться последним)
			if (toOpenScreen.ManagerActiveMode == ActiveMode.InStackFront)
				toOpenScreen.Open(parameters.useAnimOpen);
		}

		/// <summary>
		/// Перерасчёт layer индексов только для тех экранов,
		/// у которых поменялась позиция в стэке
		/// </summary>
		private void InternalRecalculateLayers(int startIndex)
		{
			for (var i = startIndex; i < StackCount; i++)
				screensStack[i].Canvas.sortingOrder = i;
		}

		private void TryPrintManager()
		{
			if (!Config.UseDebugStatistics) return;
			PrintManager();
		}
		public void PrintManager()
		{
			Debug.Log("--- Status of ScreensManager ---");
			PrintManagerList();
			PrintManagerStack();
		}
		public void PrintManagerList()
		{
			var screens = screensList.Select(s => s.name);
			var data = string.Join('\n', screens);
			Debug.Log("Screens List");
			if (!string.IsNullOrEmpty(data)) Debug.Log(data);
		}
		public void PrintManagerStack()
		{
			var screens = screensStack.Select(s => s.name);
			var data = string.Join('\n', screens);
			Debug.Log("Screens Stack");
			if (!string.IsNullOrEmpty(data)) Debug.Log(data);
		}
	}
}
