using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prazsky.Simulation;
using tainicom.Aether.Physics2D.Diagnostics;

namespace Demo.Scenes
{
	/// <summary>
	/// Základní abstraktní třída představující předka pro všechny demonstrační scény.
	/// </summary>
	public abstract class Scene
	{
		/// <summary>
		/// Hlavní třída představující demonstrační program.
		/// </summary>
		public SimulationDemo Demo { get; private set; }

		protected MultipleBody3DCreator MultipleBody3DCreator = null;
		protected DebugView DebugView;

		/// <summary>
		/// Konstruktor abstraktní třídy představující předka pro všechny demonstrační scény.
		/// </summary>
		/// <param name="demo">Hlavní třída demonstračního programu, do kterého scéna patří.</param>
		public Scene(SimulationDemo demo)
		{
			Demo = demo;
			MultipleBody3DCreator = new MultipleBody3DCreator(Demo.GraphicsDevice, Demo.World3D.World2D);
			DebugView = null;

			Load();
		}

		/// <summary>
		/// Inicializace všech objektů nutných pro existenci scény.
		/// </summary>
		public virtual void Load()
		{
			if (DebugView == null)
			{
				DebugView = new DebugView(Demo.World3D.World2D);
				DebugView.RemoveFlags(DebugViewFlags.Shape);
				DebugView.RemoveFlags(DebugViewFlags.Joint);
				DebugView.DefaultShapeColor = Color.White;
				DebugView.SleepingShapeColor = Color.LightGray;
				DebugView.TextColor = Color.Black;
				DebugView.LoadContent(Demo.GraphicsDevice, Demo.Content);
			}
		}

		/// <summary>
		/// Abstraktní metoda pro sestavení demonstrační scény.
		/// </summary>
		public abstract void Construct();

		/// <summary>
		/// Aktualizace scény mezi jednotlivými grafickými vykresleními.
		/// </summary>
		/// <param name="currentKeyboardState">Aktuální stav klávesnice.</param>
		/// <param name="previousKeyboardState">Předchozí stav klávesnice.</param>
		/// <param name="currentGamePadState">Aktuální stav herního ovladače.</param>
		/// <param name="previousGamePadState">Předchozí stav herního ovladače.</param>
		public virtual void Update(
			KeyboardState currentKeyboardState,
			KeyboardState previousKeyboardState,
			GamePadState currentGamePadState,
			GamePadState previousGamePadState)
		{
			DebugView.UpdatePerformanceGraph(Demo.World3D.World2D.UpdateTime);

			if (DemoHelper.PressedOnce(Keys.F1, currentKeyboardState, previousKeyboardState))
				EnableOrDisableFlag(DebugViewFlags.Shape);
			if (DemoHelper.PressedOnce(Keys.F2, currentKeyboardState, previousKeyboardState))
			{
				EnableOrDisableFlag(DebugViewFlags.DebugPanel);
				EnableOrDisableFlag(DebugViewFlags.PerformanceGraph);
			}
			if (DemoHelper.PressedOnce(Keys.F3, currentKeyboardState, previousKeyboardState))
				EnableOrDisableFlag(DebugViewFlags.Joint);
			if (DemoHelper.PressedOnce(Keys.F4, currentKeyboardState, previousKeyboardState))
			{
				EnableOrDisableFlag(DebugViewFlags.ContactPoints);
				EnableOrDisableFlag(DebugViewFlags.ContactNormals);
			}
			if (DemoHelper.PressedOnce(Keys.F5, currentKeyboardState, previousKeyboardState))
				EnableOrDisableFlag(DebugViewFlags.PolygonPoints);
			if (DemoHelper.PressedOnce(Keys.F6, currentKeyboardState, previousKeyboardState))
				EnableOrDisableFlag(DebugViewFlags.Controllers);
			if (DemoHelper.PressedOnce(Keys.F7, currentKeyboardState, previousKeyboardState))
				EnableOrDisableFlag(DebugViewFlags.CenterOfMass);
			if (DemoHelper.PressedOnce(Keys.F8, currentKeyboardState, previousKeyboardState))
				EnableOrDisableFlag(DebugViewFlags.AABB);
		}

		/// <summary>
		/// Sestaví statický podklad scény.
		/// </summary>
		/// <param name="count">Počet bloků pro sestavení statického podkladu.</param>
		public void ConstructGround(int count = 9)
		{
			MultipleBody3DCreator.BuildRow(Demo.Content.Load<Model>("Models/groundBlockLong"), Demo.World3D, count);
		}

		/// <summary>
		/// Vykreslení jednoho snímku scény.
		/// </summary>
		public virtual void Draw()
		{
			DebugView.RenderDebugData(Demo.Camera3D.Projection, Demo.Camera3D.View);
		}

		private void EnableOrDisableFlag(DebugViewFlags flag)
		{
			if ((DebugView.Flags & flag) == flag)
				DebugView.RemoveFlags(flag);
			else
				DebugView.AppendFlags(flag);
		}
	}
}