namespace Demo.Scenes
{
    internal class GroundScene : Scene
    {
        public GroundScene(SimulationDemo demo) : base(demo)
        {
        }

        public override void Construct()
        {
            ConstructGround();
        }
    }
}