using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prazsky.Simulation;
using Prazsky.Simulation.Camera;
using Prazsky.Simulation.Factories;
using tainicom.Aether.Physics2D.Dynamics;

namespace HelloSimulation
{
	/// <summary>
	/// This is a simple example of using the Prazsky.Simulation library.
	/// </summary>
	public class Game1 : Game
	{
		private GraphicsDeviceManager graphics;

		private Model ground, cilinder;
		private World3D world3D;
		private BasicCamera3D camera3D;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			graphics.PreferMultiSampling = true;
			graphics.PreferredBackBufferWidth = 1280;
			graphics.PreferredBackBufferHeight = 720;
		}

		protected override void Initialize()
		{
			Vector3 cameraPosition = new Vector3(0f, 5f, 6f);
			camera3D = new BasicCamera3D(cameraPosition, GraphicsDevice.Viewport.AspectRatio);
			camera3D.Target = Vector3.Zero;

			Vector2 gravity = new Vector2(0f, -9.80665f);
			world3D = new World3D(gravity, camera3D);

			base.Initialize();
		}

		protected override void LoadContent()
		{
			ground = Content.Load<Model>("ground");
			cilinder = Content.Load<Model>("cilinder");

			Vector2 groundPosition = new Vector2(0f, -1f);
			Body3D ground3D =
				Body3DFactory.CreateBody3D(
					ground, world3D.World2D, GraphicsDevice, groundPosition, BodyType.Static);
			world3D.AddBody3D(ground3D);

			Vector2 cilinderPosition = new Vector2(0f, 5f);
			Body3D cilinder3D =
				Body3DFactory.CreateBody3D(
					cilinder, world3D.World2D, GraphicsDevice, cilinderPosition);
			world3D.AddBody3D(cilinder3D);
		}

		protected override void UnloadContent()
		{
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
				|| Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			world3D.Update(gameTime);

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			world3D.Draw();

			base.Draw(gameTime);
		}
	}
}