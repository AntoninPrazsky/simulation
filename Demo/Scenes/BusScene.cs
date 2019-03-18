using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prazsky.Simulation;
using Prazsky.Simulation.Factories;
using System;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;

namespace Demo.Scenes
{
	internal class BusScene : Scene
	{
		private static readonly float MOTOR_SPEED = -20f;
		private static readonly float MAX_MOTOR_TORQUE = 1000f;

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

		private float _motorSpeed;
		private float _previousMotorSpeed;

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
		}

		public override void Update(KeyboardState currentKeyboardState, KeyboardState previousKeyboardState, GamePadState currentGamePadState, GamePadState previousGamePadState)
		{
			if (DemoHelper.PressedOnce(Keys.Space, Buttons.A, currentKeyboardState, currentGamePadState, previousKeyboardState, previousGamePadState)) Reverse();
			if (DemoHelper.PressedOnce(Keys.LeftControl, Buttons.X, currentKeyboardState, currentGamePadState, previousKeyboardState, previousGamePadState)) ToggleMotorSpeed();

			if (DemoHelper.PressedOnce(Keys.Enter, Buttons.B, currentKeyboardState, currentGamePadState, previousKeyboardState, previousGamePadState))
			{
				_stareAt = !_stareAt;
				_positionStart = Demo.Camera3D.Target;
				_sinValue = 0;
			}

			if (_stareAt && _sinValue <= Math.PI / 2)
			{
				_sinValue += 0.005;

				Demo.Camera3D.Target = Vector3.Lerp(_positionStart, _chassis3D.Position - _stareAtShift, (float)Math.Sin(_sinValue));
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
			else _motorSpeed = Math.Abs(_previousMotorSpeed) > 0f ? _previousMotorSpeed : MOTOR_SPEED;

			_springBack.MotorSpeed = _motorSpeed;
		}

		public override void Construct()
		{
			Demo.Camera3D.Position = new Vector3(17f, 9f, 19f);
			Vector2 defaultPositionShift = new Vector2(-100f, 5f);
			Demo.Camera3D.Target = new Vector3(defaultPositionShift.X, defaultPositionShift.Y, 18f);
			_positionStart = Demo.Camera3D.Target;

			int decorationCount = 50;
			ConstructGround(decorationCount);

			Body3D body3D = Body3DFactory.CreateBody3D(Demo.Content.Load<Model>("Models/groundBlockLongLeft"), Demo.World3D.World2D, Demo.GraphicsDevice, new Vector2(-129.15f, 5.9f), BodyType.Static);
			Demo.World3D.AddBody3D(body3D);

			body3D = Body3DFactory.CreateBody3D(Demo.Content.Load<Model>("Models/groundBlockLongRight"), Demo.World3D.World2D, Demo.GraphicsDevice, new Vector2(124.15f, 5.9f), BodyType.Static);
			Demo.World3D.AddBody3D(body3D);

			#region Pozadí

			int count = (decorationCount / 2) + 4;
			for (int i = -count; i < count; i++)
			{
				Demo.World3D.AddBackdrop3D(new Backdrop3D(_grass, new Vector3(i * 5, 0f, -15f)));
				Demo.World3D.AddBackdrop3D(new Backdrop3D(_grassBend, new Vector3(i * 5, 3.6f, -26.5f)));
				Demo.World3D.AddBackdrop3D(new Backdrop3D(_grassBendNegative, new Vector3(i * 5, 12.1f, -34.93f)));
			}

			#endregion Pozadí

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
				MotorSpeed = MOTOR_SPEED,
				MaxMotorTorque = MAX_MOTOR_TORQUE,
				MotorEnabled = true,
				Frequency = 4f,
				DampingRatio = 0.7f
			};
			Demo.World3D.World2D.Add(_springBack);

			Demo.World3D.AddBody3D(_backWheels3D);

			#endregion Zadní náprava

			#region Rampa

			_ramp3D = Body3DFactory.CreateBody3D(_rampModel, Demo.World3D.World2D, Demo.GraphicsDevice, new Vector2(0f, 5f));
			Demo.World3D.AddBody3D(_ramp3D);

			#endregion Rampa
		}
	}
}