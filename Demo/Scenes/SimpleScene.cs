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