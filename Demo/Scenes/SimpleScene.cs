using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Dynamics;

namespace Demo.Scenes
{
	internal class SimpleScene : Scene
	{
		public SimpleScene(SimulationDemo demo) : base(demo)
		{
		}

		public override void Construct()
		{
			//TODO: Co tato scéna demonstruje? Je na ní 5 židlí, neděje se nic dynamického.

			Demo.Camera3D.Position = new Vector3(-10.6053f, 7.946681f, 14.91888f);
			Demo.Camera3D.Target = new Vector3(-10.17842f, 7.646306f, 14.06592f);

			ConstructGround();
			MultipleBody3DCreator.BuildRow(
				 Demo.Content.Load<Model>("Models/chair"),
				 Demo.World3D,
				 5,
				 new Vector2(0f, 10f),
				 BodyType.Dynamic);
		}
	}
}