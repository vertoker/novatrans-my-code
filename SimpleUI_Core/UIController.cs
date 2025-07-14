using System;

namespace SimpleUI.Core
{
	/// <summary>
	/// Основная исполнительная единица внутри Screen.<br></br>
	/// Вся логика работы UI должна находиться тут.<br></br>
	/// Всегда имеет view, к которому привязан.<br></br>
	/// Поддерживается множественные контроллеры внутри одного screen,
	/// но в разных SubContainer
	/// </summary>
	public abstract class UIController<TView> where TView : UIView
	{
		// Не переименовывать (пожалуйста)
		public TView View { get; }
		
		public Type ControllerType => GetType().UnderlyingSystemType;
		public Type ViewType => typeof(TView).UnderlyingSystemType;
		
		protected UIController(TView view)
		{
			View = view;
		}
	}
}