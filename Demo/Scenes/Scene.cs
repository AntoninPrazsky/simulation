using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prazsky.Simulation;
using tainicom.Aether.Physics2D.Diagnostics;

namespace Demo.Scenes
{
    public abstract class Scene
    {
        public SimulationDemo Demo { get; private set; }

        protected MultipleBody3DCreator MultipleBody3DCreator = null;
        protected DebugView DebugView;

        public Scene(SimulationDemo demo)
        {
            Demo = demo;
            MultipleBody3DCreator = new MultipleBody3DCreator(Demo.GraphicsDevice);
            DebugView = null;

            Load();
        }

        public virtual void Load()
        {
            if (DebugView == null)
            {
                DebugView = new DebugView(Demo.World3D.World2D);
                DebugView.RemoveFlags(DebugViewFlags.Shape);
                DebugView.RemoveFlags(DebugViewFlags.Joint);
                DebugView.DefaultShapeColor = Color.White;
                DebugView.SleepingShapeColor = Color.LightGray;
                DebugView.TextColor = Color.Black;
                DebugView.LoadContent(Demo.GraphicsDevice, Demo.Content);
            }
        }

        public abstract void Construct();

        public virtual void Update(KeyboardState currentKeyboardState, KeyboardState previousKeyboardState, GamePadState currentGamePadState, GamePadState previousGamePadState)
        {
            DebugView.UpdatePerformanceGraph(Demo.World3D.World2D.UpdateTime);

            if (DemoHelper.PressedOnce(Keys.F1, currentKeyboardState, previousKeyboardState))
                EnableOrDisableFlag(DebugViewFlags.Shape);
            if (DemoHelper.PressedOnce(Keys.F2, currentKeyboardState, previousKeyboardState))
            {
                EnableOrDisableFlag(DebugViewFlags.DebugPanel);
                EnableOrDisableFlag(DebugViewFlags.PerformanceGraph);
            }
            if (DemoHelper.PressedOnce(Keys.F3, currentKeyboardState, previousKeyboardState))
                EnableOrDisableFlag(DebugViewFlags.Joint);
            if (DemoHelper.PressedOnce(Keys.F4, currentKeyboardState, previousKeyboardState))
            {
                EnableOrDisableFlag(DebugViewFlags.ContactPoints);
                EnableOrDisableFlag(DebugViewFlags.ContactNormals);
            }
            if (DemoHelper.PressedOnce(Keys.F5, currentKeyboardState, previousKeyboardState))
                EnableOrDisableFlag(DebugViewFlags.PolygonPoints);
            if (DemoHelper.PressedOnce(Keys.F6, currentKeyboardState, previousKeyboardState))
                EnableOrDisableFlag(DebugViewFlags.Controllers);
            if (DemoHelper.PressedOnce(Keys.F7, currentKeyboardState, previousKeyboardState))
                EnableOrDisableFlag(DebugViewFlags.CenterOfMass);
            if (DemoHelper.PressedOnce(Keys.F8, currentKeyboardState, previousKeyboardState))
                EnableOrDisableFlag(DebugViewFlags.AABB);
        }

        public void ConstructGround(int count = 9)
        {
            MultipleBody3DCreator.BuildRow(Demo.Content.Load<Model>("Models/groundBlockLong"), Demo.World3D, count);
        }

        public virtual void Draw()
        {
            DebugView.RenderDebugData(Demo.Camera3D.Projection, Demo.Camera3D.View);
        }

        private void EnableOrDisableFlag(DebugViewFlags flag)
        {
            if ((DebugView.Flags & flag) == flag)
                DebugView.RemoveFlags(flag);
            else
                DebugView.AppendFlags(flag);
        }
    }
}