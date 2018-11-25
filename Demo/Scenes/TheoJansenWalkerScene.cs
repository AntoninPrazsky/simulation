using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prazsky.Simulation;
using Prazsky.Simulation.Factories;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;

namespace Demo.Scenes
{
    /*
     * Original source Farseer Physics Engine:
     * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
     * Microsoft Permissive License (Ms-PL) v1.1
     */

    internal class TheoJansenWalkerScene : Scene
    {
        private Model _body;
        private Body _chassis;
        private Model _engine;
        private Model _leftLeg;
        private Body[] _leftLegs;
        private Model _leftShoulder;
        private Body[] _leftShoulders;
        private RevoluteJoint _motorJoint;
        private float _motorSpeed;
        private Vector2 _position;
        private Model _rightLeg;
        private Body[] _rightLegs;

        private Model _rightShoulder;
        private Body[] _rightShoulders;
        
        private Body _wheel;

        public TheoJansenWalkerScene(SimulationDemo demo) : base(demo)
        {
        }

        private void CreateChassis(World world, Vector2 pivot)
        {
            {
                PolygonShape shape = new PolygonShape(1f)
                {
                    Vertices = PolygonTools.CreateRectangle(2.5f, 1f)
                };

                _body = Demo.Content.Load<Model>("Models/Theo/body");

                _chassis = world.CreateBody();
                _chassis.BodyType = BodyType.Dynamic;
                _chassis.Position = pivot + _position;

                Fixture fixture = _chassis.CreateFixture(shape);
                fixture.CollisionGroup = -1;

                Demo.World3D.AddBody3D(new Body3D(_body, _chassis));
            }

            {
                CircleShape shape = new CircleShape(1.6f, 1f);

                _engine = Demo.Content.Load<Model>("Models/Theo/engine");

                _wheel = world.CreateBody();
                _wheel.BodyType = BodyType.Dynamic;
                _wheel.Position = pivot + _position;

                Fixture fixture = _wheel.CreateFixture(shape);
                fixture.CollisionGroup = -1;

                Demo.World3D.AddBody3D(new Body3D(_engine, _wheel));
            }

            {
                _motorJoint = new RevoluteJoint(_wheel, _chassis, _wheel.GetLocalPoint(_chassis.Position), Vector2.Zero);
                _motorJoint.CollideConnected = false;
                _motorJoint.MotorSpeed = _motorSpeed;
                _motorJoint.MaxMotorTorque = 4000f;
                _motorJoint.MotorEnabled = true;
                world.Add(_motorJoint);
            }
        }

        private void CreateLeg(World world, float direction, Vector2 wheelAnchor, out Body shoulder, out Body leg)
        {
            Vector2 p1 = new Vector2(5.4f * direction, -6.1f);
            Vector2 p2 = new Vector2(7.2f * direction, -1.2f);
            Vector2 p3 = new Vector2(4.3f * direction, -1.9f);

            Vector2 p4 = new Vector2(3.1f * direction, 0.8f);
            Vector2 p5 = new Vector2(6.0f * direction, 1.5f);
            Vector2 p6 = new Vector2(2.5f * direction, 3.7f);

            PolygonShape shoulderPolygon;
            PolygonShape legPolygon;

            Vertices vertices = new Vertices(3);

            if (direction > 0f)
            {
                vertices.Add(p1);
                vertices.Add(p2);
                vertices.Add(p3);
                shoulderPolygon = new PolygonShape(vertices, 1f);

                vertices[0] = Vector2.Zero;
                vertices[1] = p5 - p4;
                vertices[2] = p6 - p4;
                legPolygon = new PolygonShape(vertices, 2f);
            }
            else
            {
                vertices.Add(p1);
                vertices.Add(p3);
                vertices.Add(p2);
                shoulderPolygon = new PolygonShape(vertices, 1f);

                vertices[0] = Vector2.Zero;
                vertices[1] = p6 - p4;
                vertices[2] = p5 - p4;
                legPolygon = new PolygonShape(vertices, 2f);
            }

            leg = world.CreateBody();
            leg.BodyType = BodyType.Dynamic;
            leg.Position = _position;
            leg.AngularDamping = 10f;

            shoulder = world.CreateBody();
            shoulder.BodyType = BodyType.Dynamic;
            shoulder.Position = p4 + _position;
            shoulder.AngularDamping = 10f;

            Fixture f1 = leg.CreateFixture(shoulderPolygon);
            f1.CollisionGroup = -1;

            Fixture f2 = shoulder.CreateFixture(legPolygon);
            f2.CollisionGroup = -1;

            Vector2 lsp2 = leg.GetLocalPoint(p2 + _position);
            Vector2 lsp5 = shoulder.GetLocalPoint(p5 + _position);
            DistanceJoint djd = new DistanceJoint(leg, shoulder, lsp2, lsp5)
            {
                DampingRatio = 0.5f,
                Frequency = 10f
            };

            world.Add(djd);

            DistanceJoint djd2 = new DistanceJoint(leg, shoulder, leg.GetLocalPoint(p3 + _position), shoulder.GetLocalPoint(p4 + _position))
            {
                DampingRatio = 0.5f,
                Frequency = 10f
            };

            world.Add(djd2);

            DistanceJoint djd3 = new DistanceJoint(leg, _wheel, leg.GetLocalPoint(p3 + _position), _wheel.GetLocalPoint(wheelAnchor + _position))
            {
                DampingRatio = 0.5f,
                Frequency = 10f
            };

            world.Add(djd3);

            DistanceJoint djd4 = new DistanceJoint(shoulder, _wheel, shoulder.GetLocalPoint(p6 + _position), _wheel.GetLocalPoint(wheelAnchor + _position))
            {
                DampingRatio = 0.5f,
                Frequency = 10f
            };

            world.Add(djd4);

            Vector2 anchor = p4 - new Vector2(0f, 0.8f);
            RevoluteJoint rjd = new RevoluteJoint(shoulder, _chassis, shoulder.GetLocalPoint(_chassis.GetWorldPoint(anchor)), anchor);
            world.Add(rjd);
        }

        private void ConstructWalker()
        {
            _position = new Vector2(-10f, 10f);
            _motorSpeed = 2.0f;

            _leftShoulders = new Body[3];
            _rightShoulders = new Body[3];
            _leftLegs = new Body[3];
            _rightLegs = new Body[3];

            Vector2 pivot = new Vector2(0f, 0.8f);

            CreateChassis(Demo.World3D.World2D, pivot);

            Vector2 wheelAnchor = pivot + new Vector2(0f, -0.8f);

            CreateLeg(Demo.World3D.World2D, -1f, wheelAnchor, out _leftShoulders[0], out _leftLegs[0]);
            CreateLeg(Demo.World3D.World2D, 1f, wheelAnchor, out _rightShoulders[0], out _rightLegs[0]);

            _wheel.SetTransform(_wheel.Position, 120f * MathHelper.Pi / 180f);
            CreateLeg(Demo.World3D.World2D, -1f, wheelAnchor, out _leftShoulders[1], out _leftLegs[1]);
            CreateLeg(Demo.World3D.World2D, 1f, wheelAnchor, out _rightShoulders[1], out _rightLegs[1]);

            _wheel.SetTransform(_wheel.Position, -120f * MathHelper.Pi / 180f);
            CreateLeg(Demo.World3D.World2D, -1f, wheelAnchor, out _leftShoulders[2], out _leftLegs[2]);
            CreateLeg(Demo.World3D.World2D, 1f, wheelAnchor, out _rightShoulders[2], out _rightLegs[2]);

            _leftShoulder = Demo.Content.Load<Model>("Models/Theo/leftShoulder");
            _leftLeg = Demo.Content.Load<Model>("Models/Theo/leftLeg");

            _rightShoulder = Demo.Content.Load<Model>("Models/Theo/rightShoulder");
            _rightLeg = Demo.Content.Load<Model>("Models/Theo/rightLeg");

            Demo.World3D.AddBody3D(new Body3D(_leftShoulder, _leftShoulders[0], -2f));
            Demo.World3D.AddBody3D(new Body3D(_leftLeg, _leftLegs[0], -2f));

            Demo.World3D.AddBody3D(new Body3D(_rightShoulder, _rightShoulders[0], -2f));
            Demo.World3D.AddBody3D(new Body3D(_rightLeg, _rightLegs[0], -2f));

            Demo.World3D.AddBody3D(new Body3D(_leftShoulder, _leftShoulders[1], 0f));
            Demo.World3D.AddBody3D(new Body3D(_leftLeg, _leftLegs[1], 0f));

            Demo.World3D.AddBody3D(new Body3D(_rightShoulder, _rightShoulders[1], 0f));
            Demo.World3D.AddBody3D(new Body3D(_rightLeg, _rightLegs[1], 0f));

            Demo.World3D.AddBody3D(new Body3D(_leftShoulder, _leftShoulders[2], 2f));
            Demo.World3D.AddBody3D(new Body3D(_leftLeg, _leftLegs[2], 2f));

            Demo.World3D.AddBody3D(new Body3D(_rightShoulder, _rightShoulders[2], 2f));
            Demo.World3D.AddBody3D(new Body3D(_rightLeg, _rightLegs[2], 2f));
        }

        private void Reverse()
        {
            _motorSpeed *= -1f;
            _motorJoint.MotorSpeed = _motorSpeed;
        }

        public override void Update(KeyboardState currentState, KeyboardState previousState)
        {
            if (DemoHelper.PressedOnce(Keys.Space, currentState, previousState)) Reverse();

            base.Update(currentState, previousState);
        }

        public override void Construct()
        {
            ConstructGround(51);

            DebugView.AppendFlags(tainicom.Aether.Physics2D.Diagnostics.DebugViewFlags.Joint);

            Body3D body3D = Body3DFactory.CreateBody3D(Demo.Content.Load<Model>("Models/groundBlockLongLeft"), Demo.World3D.World2D, Demo.GraphicsDevice, new Vector2(-129.15f, 5.9f), BodyType.Static);
            Demo.World3D.AddBody3D(body3D);

            body3D = Body3DFactory.CreateBody3D(Demo.Content.Load<Model>("Models/groundBlockLongRight"), Demo.World3D.World2D, Demo.GraphicsDevice, new Vector2(129.15f, 5.9f), BodyType.Static);
            Demo.World3D.AddBody3D(body3D);

            ConstructWalker();
        }
    }
}