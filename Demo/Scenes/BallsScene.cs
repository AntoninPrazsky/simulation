﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prazsky.Simulation;
using Prazsky.Simulation.Factories;
using tainicom.Aether.Physics2D.Dynamics;

namespace Demo.Scenes
{
	internal class BallsScene : Scene
	{
		private Model _ballModel;

		public BallsScene(SimulationDemo demo) : base(demo)
		{
			_ballModel = Demo.Content.Load<Model>("Models/Primitives/goldBall");
		}

		private void AddBall(float restitution, float positionZ, float positionX, Category collisionCategory)
		{
			Body body = new Body();
			body.CreateCircle(1f, 1f);
			body.SetRestitution(restitution);
			body.SetCollidesWith(collisionCategory);

			Body3D body3D =
				Body3DFactory.CreateBody3D(_ballModel, Demo.World3D.World2D, body, new Vector2(positionX, 5f));
			body3D.PositionZ = positionZ;

			body3D.Body2D.ApplyLinearImpulse(Vector2.One * 10f);
			body3D.Body2D.ApplyAngularImpulse(-40f);

			Demo.World3D.AddBody3D(body3D);
		}

		public override void Construct()
		{
			Demo.Window.Title = "Restitution Demo";

			Demo.Camera3D.Position = new Vector3(22.12281f, 0.4714419f, 5.708819f);
			Demo.Camera3D.Target = new Vector3(21.21403f, 0.5188365f, 5.294256f);

			ConstructGround();

			Body3D leftBlock =
				Body3DFactory.CreateBody3D(
					Demo.Content.Load<Model>("Models/groundBlockLongLeft"),
					Demo.World3D.World2D,
					Demo.GraphicsDevice,
					new Vector2(-24.15f, 5.9f),
					BodyType.Static);
			leftBlock.Body2D.SetCollisionCategories(Category.Cat31);
			Demo.World3D.AddBody3D(leftBlock);

			Body3D rightBlock =
				Body3DFactory.CreateBody3D(
					Demo.Content.Load<Model>("Models/groundBlockLongRight"),
					Demo.World3D.World2D,
					Demo.GraphicsDevice,
					new Vector2(24.15f, 5.9f),
					BodyType.Static);
			rightBlock.Body2D.SetCollisionCategories(Category.Cat31);
			Demo.World3D.AddBody3D(rightBlock);

			AddBall(0.70f, -6f, -6f, Category.Cat31);
			AddBall(0.75f, -4f, -4f, Category.Cat31);
			AddBall(0.80f, -2f, -2f, Category.Cat31);
			AddBall(0.85f, 0f, 0f, Category.Cat31);
			AddBall(0.90f, 2f, 2f, Category.Cat31);
			AddBall(0.95f, 4f, 4f, Category.Cat31);
			AddBall(1.00f, 6f, 6f, Category.Cat31);
		}
	}
}