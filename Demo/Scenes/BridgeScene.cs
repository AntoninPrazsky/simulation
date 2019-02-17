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
    internal class BridgeScene : Scene
    {
        public BridgeScene(SimulationDemo demo) : base(demo)
        {
        }

        private Body HiddenBody;

        public override void Construct()
        {
            Demo.Camera3D.Position = new Vector3(15.04802f, 5.735241f, 0.8949657f);
            Demo.Camera3D.Target = new Vector3(14.11598f, 5.409653f, 0.7359338f);

            HiddenBody = Demo.World3D.World2D.CreateBody(Vector2.Zero);

            Path bridgePath = new Path();

            bridgePath.Add(new Vector2(-17, 5));
            bridgePath.Add(new Vector2(17, 5));

            bridgePath.Closed = false;

            Model plankModel = Demo.Content.Load<Model>("Models/bridge");

            PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(0.125f, 0.5f), 20f);

            List<Body> bodies = PathManager.EvenlyDistributeShapesAlongPath(Demo.World3D.World2D, bridgePath, shape, BodyType.Dynamic, 34);

            JointFactory.CreateRevoluteJoint(Demo.World3D.World2D, HiddenBody, bodies[0], new Vector2(0f, 0.5f));
            JointFactory.CreateRevoluteJoint(Demo.World3D.World2D, HiddenBody, bodies[bodies.Count - 1], new Vector2(0f, -0.5f));

            PathManager.AttachBodiesWithRevoluteJoint(Demo.World3D.World2D, bodies, new Vector2(0f, 0.5f), new Vector2(0f, -0.5f), false, true);

            foreach (Body b in bodies) Demo.World3D.AddBody3D(new Body3D(plankModel, b));

            Body body = new Body();
            body.CreateCircle(1f, 1f);
            body.SetRestitution(0.8f);

            Body3D body3D = Body3DFactory.CreateBody3D(Demo.Content.Load<Model>("Models/Balls/gold"), Demo.World3D.World2D, body, new Vector2(8f, 8f));

            Demo.World3D.AddBody3D(body3D);
            body.ApplyAngularImpulse(14f);
        }
    }
}