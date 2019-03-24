using Microsoft.Xna.Framework;

namespace Demo.Scenes
{
	internal class GroundScene : Scene
	{
		public GroundScene(SimulationDemo demo) : base(demo)
		{
		}

		public override void Construct()
		{
			Demo.Window.Title = "Empty Scene Demo";

			//TODO: Na této scéně nic není.

			Demo.Camera3D.Position = new Vector3(-2.411215f, 33.17371f, 3.442724f);
			Demo.Camera3D.Target = new Vector3(-2.363182f, 32.17855f, 3.357064f);

			ConstructGround();
		}
	}
}