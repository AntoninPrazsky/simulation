using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prazsky.Simulation;
using Prazsky.Simulation.Factories;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;

namespace Demo.Scenes
{
	internal class BusScene : Scene
	{
		private Model _bodyModel;
		private Body3D _chassis3D;

		private Model _frontWheelsModel;
		private Body _frontWheels;
		private Body3D _frontWheels3D;

		private WheelJoint _springFront;

		private Model _backWheelsModel;
		private Body _backWheels;
		private Body3D _backWheels3D;

		private WheelJoint _springBack;

		public BusScene(SimulationDemo demo) : base(demo)
		{
			_bodyModel = Demo.Content.Load<Model>("Models/Bus/body");
			_frontWheelsModel = Demo.Content.Load<Model>("Models/Bus/frontWheels");
			_backWheelsModel = Demo.Content.Load<Model>("Models/Bus/backWheels");
		}

		public override void Update(KeyboardState currentKeyboardState, KeyboardState previousKeyboardState, GamePadState currentGamePadState, GamePadState previousGamePadState)
		{
			base.Update(currentKeyboardState, previousKeyboardState, currentGamePadState, previousGamePadState);
		}

		public override void Construct()
		{
			Demo.Camera3D.Position = new Vector3(-3.5f, 17f, 20.6f);
			Demo.Camera3D.Target = new Vector3(0f, 7.81207f, 0f);

			ConstructGround(51);

			Vector2 defaultPositionShift = new Vector2(0f, 10f);

			_chassis3D = Body3DFactory.CreateBody3D(_bodyModel, Demo.World3D.World2D, Demo.GraphicsDevice, defaultPositionShift, BodyType.Dynamic, new Vector2(25f, 6.45566f));
			Demo.World3D.AddBody3D(_chassis3D);

			#region Přední náprava

			Vector2 frontPosition = new Vector2(6.996475f, -2.9f) + defaultPositionShift;
			Vector2 axis = new Vector2(0f, 1.2f);

			_frontWheels = new Body();
			_frontWheels.CreateCircle(1.3163165f, 1f);

			_frontWheels3D = Body3DFactory.CreateBody3D(_frontWheelsModel, Demo.World3D.World2D, _frontWheels, frontPosition);

			_springFront = new WheelJoint(_chassis3D.Body2D, _frontWheels, frontPosition, axis, true)
			{
				MotorSpeed = 0f,
				MaxMotorTorque = 10f,
				MotorEnabled = false,
				Frequency = 4f,
				DampingRatio = 0.7f
			};
			Demo.World3D.World2D.Add(_springFront);

			Demo.World3D.AddBody3D(_frontWheels3D);

			#endregion Přední náprava

			#region Zadní náprava

			Vector2 backPosition = new Vector2(-5.973385f, -2.9f) + defaultPositionShift;

			_backWheels = new Body();
			_backWheels.CreateCircle(1.364622f, 1f);

			_backWheels3D = Body3DFactory.CreateBody3D(_backWheelsModel, Demo.World3D.World2D, _backWheels, backPosition);

			_springBack = new WheelJoint(_chassis3D.Body2D, _backWheels, backPosition, axis, true)
			{
				MotorSpeed = 0f,
				MaxMotorTorque = 20f,
				MotorEnabled = false,
				Frequency = 4f,
				DampingRatio = 0.7f
			};
			Demo.World3D.World2D.Add(_springBack);

			Demo.World3D.AddBody3D(_backWheels3D);

			#endregion Zadní náprava
		}
	}
}