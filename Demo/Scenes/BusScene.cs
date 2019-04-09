using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prazsky.Simulation;
using Prazsky.Simulation.Factories;
using System;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;

namespace Demo.Scenes
{
	/// <summary>
	/// Třída BusScene představuje funkční simulaci jedoucího autobusu a množství okolních dekoračních prvků
	/// představujících zastávku, sloup elektrického vedení nebo zelené pozadí. Na silnici před autobusem se zároveň
	/// nachází překážka (rampa) pro demonstraci odpružených kloubů držících nápravy autobusu, z nichž pouze zadní je
	/// hnaná.
	///
	/// Podobně jako scéna TheoJansenWalkerScene poskytuje i scéna BusScene rozšířené ovládání. Ve výchozím stavu
	/// kamera sleduje jedoucí autobus tak, aby se nikdy nevyskytl mimo záběr. Toto uchycení kamery lze zrušit nebo
	/// znovu aktivovat klávesou ENTER. Změnit směr otáčení zadních kol autobusu lze klávesou MEZERNÍK a zcela zabrzdit
	/// je možné klávesou levý CTRL. Ostatní ovládání (pohybu kamery) je identické jako u ostatních scén za
	/// předpokladu, že kamera není uchycena pro sledování autobusu (potom není možné měnit její směr pohledu).
	/// </summary>
	internal class BusScene : Scene
	{
		private static readonly float DEFAULT_MOTOR_SPEED = -20f;
		private static readonly float MAX_MOTOR_TORQUE = 1000f;
		private static float BLOCK_SIZE = 20f;

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

		private Model _grass;
		private Model _grassBend;
		private Model _grassBendNegative;

		private Model _rampModel;
		private Body3D _ramp3D;

		private bool _stareAt = true;
		private Vector3 _positionStart;
		private double _sinValue = 0;
		private Vector3 _stareAtShift;

		private float _motorSpeed = DEFAULT_MOTOR_SPEED;
		private float _previousMotorSpeed;

		private Model _roadCenter, _roadInco, _roadStopA, _roadStopA1, _roadStopB, _roadStopB1, _roadStopA1End,
			_roadStopB1End, _sideA, _sideB;

		private Model _roadBlock;
		private Model _pole;

		public BusScene(SimulationDemo demo) : base(demo)
		{
			_bodyModel = Demo.Content.Load<Model>("Models/Bus/body");
			_frontWheelsModel = Demo.Content.Load<Model>("Models/Bus/frontWheels");
			_backWheelsModel = Demo.Content.Load<Model>("Models/Bus/backWheels");

			_grass = Demo.Content.Load<Model>("Models/Ground/grass");
			_grassBend = Demo.Content.Load<Model>("Models/Ground/grassBend");
			_grassBendNegative = Demo.Content.Load<Model>("Models/Ground/grassBendNegative");

			_rampModel = Demo.Content.Load<Model>("Models/Obstacles/ramp");

			_stareAtShift = new Vector3(-10f, 1.5f, 0f);

			#region Roads

			_roadCenter = Demo.Content.Load<Model>("Models/Roads/roadCenter");
			_roadInco = Demo.Content.Load<Model>("Models/Roads/roadInco");
			_roadStopA = Demo.Content.Load<Model>("Models/Roads/roadStopA");
			_roadStopA1 = Demo.Content.Load<Model>("Models/Roads/roadStopA1");
			_roadStopB = Demo.Content.Load<Model>("Models/Roads/roadStopB");
			_roadStopB1 = Demo.Content.Load<Model>("Models/Roads/roadStopB1");
			_roadStopA1End = Demo.Content.Load<Model>("Models/Roads/roadStopA1End");
			_roadStopB1End = Demo.Content.Load<Model>("Models/Roads/roadStopB1End");
			_sideA = Demo.Content.Load<Model>("Models/Roads/sideA");
			_sideB = Demo.Content.Load<Model>("Models/Roads/sideB");

			_roadBlock = Demo.Content.Load<Model>("Models/Obstacles/roadBlock");
			_pole = Demo.Content.Load<Model>("Models/Decorations/pole");

			#endregion Roads
		}

		public override void Update(
			KeyboardState currentKeyboardState,
			KeyboardState previousKeyboardState,
			GamePadState currentGamePadState,
			GamePadState previousGamePadState)
		{
			if (DemoHelper.PressedOnce(
				Keys.Space,
				Buttons.A,
				currentKeyboardState,
				currentGamePadState,
				previousKeyboardState,
				previousGamePadState))
				Reverse();
			if (DemoHelper.PressedOnce(
				Keys.LeftControl,
				Buttons.X,
				currentKeyboardState,
				currentGamePadState,
				previousKeyboardState,
				previousGamePadState))
				ToggleMotorSpeed();

			if (DemoHelper.PressedOnce(
				Keys.Enter,
				Buttons.B,
				currentKeyboardState,
				currentGamePadState,
				previousKeyboardState,
				previousGamePadState))
			{
				_stareAt = !_stareAt;
				_positionStart = Demo.Camera3D.Target;
				_sinValue = 0;
			}

			if (_stareAt && _sinValue <= Math.PI / 2)
			{
				_sinValue += 0.005;

				Demo.Camera3D.Target =
					Vector3.Lerp(_positionStart, _chassis3D.Position - _stareAtShift, (float)Math.Sin(_sinValue));
			}
			else if (_stareAt)
			{
				Demo.Camera3D.Target = _chassis3D.Position - _stareAtShift;
			}

			base.Update(currentKeyboardState, previousKeyboardState, currentGamePadState, previousGamePadState);
		}

		private void Reverse()
		{
			_motorSpeed *= -1f;
			_springBack.MotorSpeed = _motorSpeed;
		}

		private void ToggleMotorSpeed()
		{
			if (Math.Abs(_motorSpeed) > 0)
			{
				_previousMotorSpeed = _motorSpeed;
				_motorSpeed = 0f;
			}
			else _motorSpeed = Math.Abs(_previousMotorSpeed) > 0f ? _previousMotorSpeed : DEFAULT_MOTOR_SPEED;

			_springBack.MotorSpeed = _motorSpeed;
		}

		private void BuildRoadBlock(Vector2 position, bool center)
		{
			PolygonShape roadShape = new PolygonShape(1f)
			{
				Vertices = PolygonTools.CreateRectangle(10f, 0.15f)
			};

			Body roadCenter = Demo.World3D.World2D.CreateBody();
			roadCenter.BodyType = BodyType.Static;
			roadCenter.Position = position;

			Fixture fixture = roadCenter.CreateFixture(roadShape);
			Demo.World3D.AddBody3D(new Body3D(center ? _roadCenter : _roadInco, roadCenter));
		}

		private void ConstructBusStop()
		{
			float roadYShift = 0.19f;
			int roadCount = 10;

			MultipleBody3DCreator multipleBody3DCreator =
				new MultipleBody3DCreator(Demo.GraphicsDevice, Demo.World3D.World2D);

			Demo.World3D.AddBody3D(
				multipleBody3DCreator.CreateBody3D(
					_roadBlock,
					new Vector2(BLOCK_SIZE * -(roadCount - 2), 2.6f),
					BodyType.Static));
			Demo.World3D.AddBody3D(
				multipleBody3DCreator.CreateBody3D(
					_roadBlock,
					new Vector2(BLOCK_SIZE * (roadCount - 2), 2.6f),
					BodyType.Static));

			BuildRoadBlock(new Vector2(-BLOCK_SIZE, 0f), true);
			BuildRoadBlock(Vector2.Zero, true);
			BuildRoadBlock(new Vector2(BLOCK_SIZE, 0f), true);

			BuildRoadBlock(new Vector2(2 * BLOCK_SIZE, 0f), false);
			BuildRoadBlock(new Vector2(-2 * BLOCK_SIZE, 0f), false);

			for (int i = 3; i < roadCount; i++) BuildRoadBlock(new Vector2(BLOCK_SIZE * i, 0f), true);
			for (int i = 3; i < roadCount; i++) BuildRoadBlock(new Vector2(BLOCK_SIZE * -i, 0f), true);

			#region Dekorační bloky

			Demo.World3D.AddBackdrop3D(
				new Backdrop3D(_roadStopA, new Vector3(-BLOCK_SIZE, roadYShift, -BLOCK_SIZE)));
			Demo.World3D.AddBackdrop3D(
				new Backdrop3D(_roadStopA, new Vector3(0f, roadYShift, -BLOCK_SIZE)));
			Demo.World3D.AddBackdrop3D(
				new Backdrop3D(_roadStopA, new Vector3(BLOCK_SIZE, roadYShift, -BLOCK_SIZE)));

			Demo.World3D.AddBackdrop3D(
				new Backdrop3D(_roadStopA1End, new Vector3(-2 * BLOCK_SIZE, roadYShift, -BLOCK_SIZE)));
			Demo.World3D.AddBackdrop3D(
				new Backdrop3D(_roadStopA1, new Vector3(2 * BLOCK_SIZE, roadYShift, -BLOCK_SIZE)));

			Demo.World3D.AddBackdrop3D(
				new Backdrop3D(_roadStopB, new Vector3(-BLOCK_SIZE, roadYShift, BLOCK_SIZE)));
			Demo.World3D.AddBackdrop3D(
				new Backdrop3D(_roadStopB, new Vector3(0f, roadYShift, BLOCK_SIZE)));
			Demo.World3D.AddBackdrop3D(
				new Backdrop3D(_roadStopB, new Vector3(BLOCK_SIZE, roadYShift, BLOCK_SIZE)));

			Demo.World3D.AddBackdrop3D(
				new Backdrop3D(_roadStopB1End, new Vector3(-2 * BLOCK_SIZE, roadYShift, BLOCK_SIZE)));
			Demo.World3D.AddBackdrop3D(
				new Backdrop3D(_roadStopB1, new Vector3(2 * BLOCK_SIZE, roadYShift, BLOCK_SIZE)));

			for (int i = 3; i < roadCount; i++)
				Demo.World3D.AddBackdrop3D(
					new Backdrop3D(_sideA, new Vector3(BLOCK_SIZE * i, roadYShift, -BLOCK_SIZE)));
			for (int i = 3; i < roadCount; i++)
				Demo.World3D.AddBackdrop3D(
					new Backdrop3D(_sideB, new Vector3(BLOCK_SIZE * i, roadYShift, BLOCK_SIZE)));

			for (int i = 3; i < roadCount; i++)
				Demo.World3D.AddBackdrop3D(
					new Backdrop3D(_sideA, new Vector3(BLOCK_SIZE * -i, roadYShift, -BLOCK_SIZE)));
			for (int i = 3; i < roadCount; i++)
				Demo.World3D.AddBackdrop3D(
					new Backdrop3D(_sideB, new Vector3(BLOCK_SIZE * -i, roadYShift, BLOCK_SIZE)));

			#endregion Dekorační bloky
		}

		private void ConstructBackground()
		{
			#region Pozadí

			int decorationCount = 50;

			int count = (decorationCount / 2) + 4;
			for (int i = -count; i < count; i++)
			{
				Demo.World3D.AddBackdrop3D(
					new Backdrop3D(_grass, new Vector3(i * 5, 0f, -15f - 22f)));
				Demo.World3D.AddBackdrop3D(
					new Backdrop3D(_grassBend, new Vector3(i * 5, 3.6f, -26.5f - 22f)));
				Demo.World3D.AddBackdrop3D(
					new Backdrop3D(_grassBendNegative, new Vector3(i * 5, 12.1f, -34.93f - 22f)));
			}

			Demo.World3D.AddBackdrop3D(new Backdrop3D(_pole, new Vector3(-55f, 12f, -35f)));

			#endregion Pozadí
		}

		public override void Construct()
		{
			Demo.Window.Title = "Game Scene Demo";

			Demo.Camera3D.Position = new Vector3(17f, 9f, 19f);
			Vector2 defaultPositionShift = new Vector2(-100f, 5f);
			Demo.Camera3D.Target = new Vector3(defaultPositionShift.X, defaultPositionShift.Y, 18f);
			_positionStart = Demo.Camera3D.Target;

			ConstructBusStop();
			ConstructBackground();

			PolygonShape shape = new PolygonShape(1f)
			{
				Vertices = PolygonTools.CreateRoundedRectangle(25f, 6.45566f, 1.5f, 1.5f, 4)
			};

			_chassis3D =
				Body3DFactory.CreateBody3D(
					_bodyModel,
					Demo.World3D.World2D,
					Demo.GraphicsDevice,
					defaultPositionShift,
					BodyType.Dynamic,
					new Vector2(25f, 6.45566f));

			Demo.World3D.AddBody3D(_chassis3D);

			#region Přední náprava

			Vector2 frontPosition = new Vector2(6.996475f, -2.9f) + defaultPositionShift;
			Vector2 axis = new Vector2(0f, 1.2f);

			_frontWheels = new Body();
			_frontWheels.CreateCircle(1.3163165f, 1f);

			_frontWheels3D =
				Body3DFactory.CreateBody3D(
					_frontWheelsModel,
					Demo.World3D.World2D,
					_frontWheels,
					frontPosition);

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

			_backWheels3D =
				Body3DFactory.CreateBody3D(
					_backWheelsModel,
					Demo.World3D.World2D,
					_backWheels,
					backPosition);

			_springBack = new WheelJoint(_chassis3D.Body2D, _backWheels, backPosition, axis, true)
			{
				MotorSpeed = DEFAULT_MOTOR_SPEED,
				MaxMotorTorque = MAX_MOTOR_TORQUE,
				MotorEnabled = true,
				Frequency = 4f,
				DampingRatio = 0.7f
			};
			Demo.World3D.World2D.Add(_springBack);

			Demo.World3D.AddBody3D(_backWheels3D);

			#endregion Zadní náprava

			#region Rampa

			_ramp3D = Body3DFactory.CreateBody3D(
				_rampModel,
				Demo.World3D.World2D,
				Demo.GraphicsDevice,
				new Vector2(0f, 5f));
			Demo.World3D.AddBody3D(_ramp3D);

			#endregion Rampa
		}
	}
}