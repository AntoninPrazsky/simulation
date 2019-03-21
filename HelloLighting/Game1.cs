using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prazsky.Render;
using Prazsky.Simulation;
using Prazsky.Simulation.Camera;
using Prazsky.Simulation.Factories;
using tainicom.Aether.Physics2D.Dynamics;

namespace HelloLighting
{
	/// <summary>
	/// This is an example demonstrating how to use the BasicEffectParams class.
	/// </summary>
	public class Game1 : Game
	{
		private GraphicsDeviceManager graphics;

		private Model arch, sphere;
		private World3D world3D;
		private BasicCamera3D camera3D;

		private BasicEffectParams effectParams;

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
			Vector3 cameraPosition = new Vector3(0f, 4f, 15f);
			camera3D = new BasicCamera3D(cameraPosition, GraphicsDevice.Viewport.AspectRatio);
			camera3D.Target = new Vector3(0f, 0f, 3f);

			Vector2 gravity = new Vector2(0f, -9.80665f);
			world3D = new World3D(gravity, camera3D);

			#region BasicEffectParams

			Vector3 ambientColor = Color.Blue.ToVector3();
			Vector3 specularColor = Color.Red.ToVector3();
			float specularPower = 1f;
			Vector3 emmisiveColor = Color.Black.ToVector3();

			Vector3 directionalLight0Direction = new Vector3(-1.5f, -1.5f, 1f);
			Vector3 directionalLight0DiffuseColor = Color.Green.ToVector3();
			Vector3 directionalLight0SpecularColor = Color.Magenta.ToVector3();

			Vector3 directionalLight1Direction = new Vector3(-1.5f, 1.5f, 0f);
			Vector3 directionalLight1DiffuseColor = Color.Cyan.ToVector3();
			Vector3 directionalLight1SpecularColor = Color.Yellow.ToVector3();

			Vector3 directionalLight2Direction = new Vector3(0, 1.5f, 1.5f);
			Vector3 directionalLight2DiffuseColor = Color.Violet.ToVector3();
			Vector3 directionalLight2SpecularColor = Color.White.ToVector3();

			DirectionalLightParams directionalLight1 =
				new DirectionalLightParams(
					directionalLight0Direction,
					directionalLight0DiffuseColor,
					directionalLight0SpecularColor);

			DirectionalLightParams directionalLight2 =
				new DirectionalLightParams(
					directionalLight1Direction,
					directionalLight1DiffuseColor,
					directionalLight1SpecularColor);

			DirectionalLightParams directionalLight3 =
				new DirectionalLightParams(
					directionalLight2Direction,
					directionalLight2DiffuseColor,
					directionalLight2SpecularColor); ;

			FogParams fogParams = new FogParams(Color.CornflowerBlue.ToVector3(), 10f, 25f);

			effectParams =
				new BasicEffectParams(
					ambientColor,
					specularColor,
					specularPower,
					emmisiveColor,
					directionalLight1,
					directionalLight2,
					directionalLight3,
					fogParams);

			#endregion BasicEffectParams

			base.Initialize();
		}

		protected override void LoadContent()
		{
			arch = Content.Load<Model>("arch");
			sphere = Content.Load<Model>("sphere");

			Vector2 archPosition = new Vector2(0f, -1f);
			Body3D arch3D =
				Body3DFactory.CreateBody3D(
					arch,
					world3D.World2D,
					GraphicsDevice,
					archPosition,
					BodyType.Static,
					basicEffectParams: effectParams);
			world3D.AddBody3D(arch3D);

			Vector2 spherePosition = new Vector2(0f, 5f);
			Body3D sphere3D =
				Body3DFactory.CreateBody3D(
					sphere,
					world3D.World2D,
					GraphicsDevice,
					spherePosition,
					basicEffectParams: effectParams);

			sphere3D.Body2D.SetRestitution(0.8f);
			sphere3D.Body2D.ApplyAngularImpulse(14f);

			world3D.AddBody3D(sphere3D);
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