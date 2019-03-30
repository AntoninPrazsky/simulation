using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prazsky.Simulation;
using Prazsky.Simulation.Factories;
using System.Collections.Generic;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;

namespace Demo.Scenes
{
	/// <summary>
	/// Scéna zobrazující most tvořený 33 příčkami, jejichž tvar pro fyzikální simulaci je obdélník, avšak
	/// trojrozměrný model je komplexnější, což vytváří vizuálně atraktivní kompozici. Jednotlivé příčky mostu jsou
	/// spojeny ohybnými klouby a na scéně se dále vyskytuje sféra (její dvojrozměrný fyzikální tvar je kruh). Most se
	/// v závislosti na pohybu tohoto tělesa dynamicky pohybuje, jako by jeho jednotlivé části byly spojeny provazem.
	/// Podklad zde tedy není statický jako u ostatních scén.
	///
	/// Na těleso ve tvaru kruhu je při sestavování scény aplikován úhlový impuls o velikosti 5f, čímž dojde k jeho
	/// roztočení okolo osy Z. Při dopadu na povrch mostu se proto kruh začne kutálet ve směru od pozorovatele scény,
	/// což respektuje i trojrozměrný model ve tvaru sféry a společně s navázaným pohybem jednotlivých příček mostu
	/// působí scéna plynule dynamickým dojmem.
	/// </summary>
	internal class BridgeScene : Scene
	{
		public BridgeScene(SimulationDemo demo) : base(demo)
		{
		}

		private Body HiddenBody;

		public override void Construct()
		{
			Demo.Window.Title = "Path Demo";

			Demo.Camera3D.Position = new Vector3(16.36052f, 7.006462f, 1.119595f);
			Demo.Camera3D.Target = new Vector3(15.43861f, 6.659885f, 0.9464932f);

			HiddenBody = Demo.World3D.World2D.CreateBody(Vector2.Zero);

			Path bridgePath = new Path();

			bridgePath.Add(new Vector2(-17, 5));
			bridgePath.Add(new Vector2(17, 5));

			bridgePath.Closed = false;

			Model plankModel = Demo.Content.Load<Model>("Models/bridge");

			PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(0.125f, 0.5f), 20f);

			List<Body> bodies =
				PathManager.EvenlyDistributeShapesAlongPath(Demo.World3D.World2D,
				bridgePath, shape,
				BodyType.Dynamic,
				34);

			JointFactory.CreateRevoluteJoint(
				Demo.World3D.World2D,
				HiddenBody,
				bodies[0],
				new Vector2(0f, 0.5f));
			JointFactory.CreateRevoluteJoint(
				Demo.World3D.World2D,
				HiddenBody,
				bodies[bodies.Count - 1],
				new Vector2(0f, -0.5f));

			PathManager.AttachBodiesWithRevoluteJoint(
				Demo.World3D.World2D,
				bodies,
				new Vector2(0f, 0.5f),
				new Vector2(0f, -0.5f),
				false,
				true);

			foreach (Body b in bodies) Demo.World3D.AddBody3D(new Body3D(plankModel, b));

			Body body = new Body();
			body.CreateCircle(1f, 1f);
			body.SetRestitution(0.7f);

			Body3D body3D =
				Body3DFactory.CreateBody3D(
					Demo.Content.Load<Model>("Models/Primitives/goldBall"),
					Demo.World3D.World2D,
					body,
					new Vector2(8f, 8f));

			Demo.World3D.AddBody3D(body3D);
			body.ApplyAngularImpulse(5f);
		}
	}
}