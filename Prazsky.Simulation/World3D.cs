using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Prazsky.Render;
using Prazsky.Simulation.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Dynamics;

namespace Prazsky.Simulation
{
    public class World3D
    {
        #region Parametry pro fyzikální engine

        //Vícevláknové výpočty fyzikálního enginu
        private const int VELOCITY_CONSTRAINTS_MULTITHREAD_THRESHOLD = 256;
        private const int POSITION_CONSTRAINTS_MULTITHREAD_THRESHOLD = 256;
        private const int COLLIDE_MULTITHREAD_THRESHOLD = 256;

        //Minimální obnovovací frekvence simulace
        private const float MINIMUM_SOLVER_ITERATIONS = 1f / 30f; //30 Hz

        #endregion

        private List<Body3D> _body3Ds = new List<Body3D>();

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        public World World2D;
        
        public ICamera Camera3D;

        private BoundingFrustum _boundingFrustum;

        private bool _disableSimulationWhenOutOfBoundingFrustum = true;
        public bool DisableSimulationWhenOutOfBoundingFrustum { get => _disableSimulationWhenOutOfBoundingFrustum; set => _disableSimulationWhenOutOfBoundingFrustum = value; }
        
        #region FORDEBUG

        DebugDraw _debugDraw;

        #endregion

        public World3D(Vector2 gravity)
        {
            Camera3D = null;
            World2D = new World(gravity);

            World2D.ContactManager.VelocityConstraintsMultithreadThreshold = VELOCITY_CONSTRAINTS_MULTITHREAD_THRESHOLD;
            World2D.ContactManager.PositionConstraintsMultithreadThreshold = POSITION_CONSTRAINTS_MULTITHREAD_THRESHOLD;
            World2D.ContactManager.CollideMultithreadThreshold = POSITION_CONSTRAINTS_MULTITHREAD_THRESHOLD;
        }

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            #region FORDEBUG
            _debugDraw = new DebugDraw(graphicsDevice);
            #endregion FORDEBUG
        }

        public void Update(GameTime gameTime)
        {
            float timeStep = Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, MINIMUM_SOLVER_ITERATIONS);
            World2D.Step(timeStep);
        }

        public void Draw(GameTime gameTime)
        {
            _boundingFrustum = new BoundingFrustum(Camera3D.View * Camera3D.Projection);

            if (_body3Ds.Count > 0)
            { 
                foreach (Body3D b in _body3Ds)
                {
                    if (_boundingFrustum.Contains(b.BoundingSphere) != ContainmentType.Disjoint)
                    {
                        if (_disableSimulationWhenOutOfBoundingFrustum)
                        {
                            b.EnableSimulation();
                        }
                        b.Draw();
                    }
                    else
                    {
                        if (_disableSimulationWhenOutOfBoundingFrustum)
                        {
                            b.DisableSimulation();
                        }
                    }

                    #region FORDEBUG
                    if (false)
                    { 
                        _debugDraw.Begin(Camera3D.View, Camera3D.Projection);
                        _debugDraw.DrawWireSphere(b.BoundingSphere, Color.Blue);
                        _debugDraw.DrawWireFrustum(_boundingFrustum, Color.Green);
                        _debugDraw.End();
                    }
                    #endregion



                }
            }
        }

        public bool AddBody3D(Body3D body3D)
        {
            if (_body3Ds.Contains(body3D)) return false;

            _body3Ds.Add(body3D);
            return true;
        }

        
        public bool RemoveBody3D(Body3D body3D)
        {
            if (!_body3Ds.Contains(body3D)) return false;

            _body3Ds.Remove(body3D);
            return true;
        }

        /// <summary>
        /// Vrátí souřadnice bodu v dvourozměrném světě na základě souřadnic (<see cref="Vector2"/>) z dvourozměrné projekce trojrozměrného světa.
        /// </summary>
        /// <param name="screenCoordinates">Souřadnice na dvourozměrné projekci.</param>
        /// <param name="viewport">Dvourozměrné zobrazení trojrozměrného světa.</param>
        /// <returns></returns>
        public Vector2 GetWorld2DCoordinatesFromScreen(Vector2 screenCoordinates, Viewport viewport)
        {
            Vector3 nearPoint = viewport.Unproject(new Vector3(screenCoordinates, 0f), Camera3D.Projection, Camera3D.View, Matrix.Identity);
            Vector3 farPoint = viewport.Unproject(new Vector3(screenCoordinates, 1f), Camera3D.Projection, Camera3D.View, Matrix.Identity);

            Vector3 direction = (farPoint - nearPoint);
            direction.Normalize();

            Ray ray = new Ray(nearPoint, direction);
            Plane plane = new Plane(Vector3.Backward, 0);

            float? intersection = ray.Intersects(plane);
            Vector3 computed = ray.Position + ray.Direction * intersection.GetValueOrDefault(1f);

            return new Vector2(computed.X, computed.Y);
        }

        /// <summary>
        /// Vrátí souřadnice bodu v dvourozměrném světě na základě souřadnic (<see cref="Point"/>) z dvourozměrné projekce trojrozměrného světa.
        /// </summary>
        /// <param name="screenCoordinates">Souřadnice na dvourozměrné projekci.</param>
        /// <param name="viewport">Dvourozměrné zobrazení trojrozměrného světa.</param>
        /// <returns></returns>
        public Vector2 GetWorld2DCoordinatesFromScreen(Point screenCoordinates, Viewport viewport)
        {
            return GetWorld2DCoordinatesFromScreen(new Vector2(screenCoordinates.X, screenCoordinates.Y), viewport);
        }
        
    }
}
