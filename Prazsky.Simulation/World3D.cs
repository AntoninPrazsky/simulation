using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prazsky.Simulation.Camera;
using System;
using System.Collections.Generic;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;

namespace Prazsky.Simulation
{
    /// <summary>
    /// Představuje trojrozměrný svět, ve kterém probíhá dvourozměrná fyzikální simulace.
    /// </summary>
    public class World3D
    {
        #region Parametry pro fyzikální knihovnu

        //Parametry vícevláknových výpočtů fyzikální knihovny
        private const int VELOCITY_CONSTRAINTS_MULTITHREAD_THRESHOLD = 256;

        private const int POSITION_CONSTRAINTS_MULTITHREAD_THRESHOLD = 256;
        private const int COLLIDE_MULTITHREAD_THRESHOLD = 256;

        //Výchozí minimální obnovovací frekvence simulace (30 Hz)
        private const float DEFAULT_MINIMUM_SOLVER_ITERATIONS = 1f / 30f;

        #endregion Parametry pro fyzikální knihovnu

        private BoundingFrustum _boundingFrustum;
        private List<Body3D> _body3Ds = new List<Body3D>();

        /// <summary>
        /// Deaktivace fyzikální simulace, pokud je trojrozměrné těleso mimo promítací kužel.
        /// </summary>
        public bool DisableSimulationWhenOutOfBoundingFrustum { get; set; } = false;

        /*
        /// <summary>
        /// Uspání těles mimo promítací kužel funkcionalitou fyzikální knihovny <see cref="Body.Awake"/>.
        /// TODO: Promyslet, jestli má tato vlastnost smysl, uživatel může chtít používat Body.Awake k jiným účelům.
        /// </summary>
        public bool SleepWhenOutOfBoundingFrustum { get; set; } = true;
        */

        /// <summary>
        /// Minimální obnovovací frekvence simulace.
        /// </summary>
        public float MinimumSolverIterations { get; set; } = DEFAULT_MINIMUM_SOLVER_ITERATIONS;

        /// <summary>
        /// Dvourozměrný fyzikální svět fyzikální knihovny.
        /// </summary>
        public World World2D;

        /// <summary>
        /// Trojrozměrná kamera, která trojrozměrný svět pozoruje.
        /// </summary>
        public ICamera Camera3D;

        /// <summary>
        /// Konstruktor trojrozměrného světa.
        /// </summary>
        /// <param name="gravity">Výchozí gravitace pro dvourozměrný fyzikální svět.</param>
        /// <param name="camera">Výchozí kamera, která trojrozměrný svět pozoruje.</param>
        public World3D(Vector2 gravity, ICamera camera)
        {
            Camera3D = camera;
            World2D = new World(gravity);

            World2D.ContactManager.VelocityConstraintsMultithreadThreshold = VELOCITY_CONSTRAINTS_MULTITHREAD_THRESHOLD;
            World2D.ContactManager.PositionConstraintsMultithreadThreshold = POSITION_CONSTRAINTS_MULTITHREAD_THRESHOLD;
            World2D.ContactManager.CollideMultithreadThreshold = POSITION_CONSTRAINTS_MULTITHREAD_THRESHOLD;
        }

        /// <summary>
        /// Provede jeden krok fyzikální simulace ve dvourozměrném fyzikálním světě (<see cref="World"/>).
        /// Krok není nikdy menší než hodnota udaná parametrem <see cref="MinimumSolverIterations"/>.
        /// </summary>
        /// <param name="gameTime">Herní čas.</param>
        public void Update(GameTime gameTime)
        {
            float timeStep = Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, MinimumSolverIterations);
            World2D.Step(timeStep);
        }

        /// <summary>
        /// Vykreslí jeden snímek trojrozměrného světa.
        /// </summary>
        public void Draw()
        {
            _boundingFrustum = new BoundingFrustum(Camera3D.View * Camera3D.Projection);

            if (_body3Ds.Count > 0)
            {
                foreach (Body3D body3D in _body3Ds)
                {
                    body3D.Update3DPosition();

                    if (_boundingFrustum.Contains(body3D.BoundingSphere) != ContainmentType.Disjoint)
                    {
                        //Těleso je uvnitř promítacího kuželu, potom se vždy simuluje a vykresluje
                        body3D.EnableSimulation();
                        body3D.Draw(Camera3D);
                    }
                    else
                    {
                        //Těleso není uvnitř promítacího kuželu
                        //Chce uživatel v této situaci neprovádět simulaci?
                        if (DisableSimulationWhenOutOfBoundingFrustum) body3D.DisableSimulation();
                    }
                }
            }
        }

        /// <summary>
        /// Přidá trojrozměrný simulovatelný objekt do trojrozměrného světa.
        /// </summary>
        /// <param name="body3D">Trojrozměrný simulovatelný objekt.</param>
        /// <returns>Vrací <code>true</code>, pokud se přidání podařilo, a <code>false</code>, pokud již stejný objekt
        /// byl přidán.</returns>
        public bool AddBody3D(Body3D body3D)
        {
            if (_body3Ds.Contains(body3D)) return false;

            _body3Ds.Add(body3D);
            return true;
        }

        /// <summary>
        /// Odebere trojrozměrný simulovatelný objekt z trojrozměrného světa.
        /// </summary>
        /// <param name="body3D">Trojrozměrný simulovatelný objekt, který má být odebrán.</param>
        /// <returns>Vrací <code>true</code>, pokud se odebrání podařilo, a <code>false</code>, pokud odebíraný objekt
        /// neexistuje.</returns>
        public bool RemoveBody3D(Body3D body3D)
        {
            if (!_body3Ds.Contains(body3D)) return false;

            _body3Ds.Remove(body3D);
            return true;
        }

        /// <summary>
        /// Vrátí souřadnice bodu v dvourozměrném světě na základě souřadnic (<see cref="Vector2"/>) z dvourozměrné
        /// projekce trojrozměrného světa.
        /// </summary>
        /// <param name="screenCoordinates">Souřadnice na dvourozměrné projekci.</param>
        /// <param name="viewport">Dvourozměrné zobrazení trojrozměrného světa.</param>
        /// <returns></returns>
        public Vector2 GetWorld2DCoordinatesFromScreen(Vector2 screenCoordinates, Viewport viewport)
        {
            Vector3 nearPoint = viewport.Unproject(
                new Vector3(screenCoordinates, 0f), Camera3D.Projection, Camera3D.View, Matrix.Identity);
            Vector3 farPoint = viewport.Unproject(
                new Vector3(screenCoordinates, 1f), Camera3D.Projection, Camera3D.View, Matrix.Identity);

            Vector3 direction = (farPoint - nearPoint);
            direction.Normalize();

            Ray ray = new Ray(nearPoint, direction);
            Plane plane = new Plane(Vector3.Backward, 0);

            float? intersection = ray.Intersects(plane);
            Vector3 computed = ray.Position + ray.Direction * intersection.GetValueOrDefault(1f);

            return new Vector2(computed.X, computed.Y);
        }

        /// <summary>
        /// Vrátí souřadnice bodu v dvourozměrném světě na základě souřadnic (<see cref="Point"/>) z dvourozměrné
        /// projekce trojrozměrného světa.
        /// </summary>
        /// <param name="screenCoordinates">Souřadnice na dvourozměrné projekci.</param>
        /// <param name="viewport">Dvourozměrné zobrazení trojrozměrného světa.</param>
        /// <returns></returns>
        public Vector2 GetWorld2DCoordinatesFromScreen(Point screenCoordinates, Viewport viewport)
        {
            return GetWorld2DCoordinatesFromScreen(new Vector2(screenCoordinates.X, screenCoordinates.Y), viewport);
        }

        /// <summary>
        /// Odebere všechny objekty ze simulovaného světa.
        /// </summary>
        public void Clear()
        {
            World2D.Clear();
            _body3Ds.Clear();
        }

        private FixedMouseJoint _fixedMouseJoint = null;
        private Fixture _foundFixture = null;

        /// <summary>
        /// Zkusí uchopit těleso na dané pozici danou silou.
        /// </summary>
        /// <param name="position">Pozice k uchopení.</param>
        /// <param name="force">Síla uchopení.</param>
        /// <param name="viewport">Dvourozměrné zobrazení trojrozměrného světa.</param>
        public void GrabBody(Point position, float force, Viewport viewport)
        {
            Vector2 positionWorld2D = GetWorld2DCoordinatesFromScreen(position, viewport);

            if (_fixedMouseJoint != null)
            {
                _fixedMouseJoint.WorldAnchorB = positionWorld2D;
                return;
            }
            else
            {
                _foundFixture = World2D.TestPoint(positionWorld2D);

                if (_foundFixture != null)
                {
                    Body body = _foundFixture.Body;
                    _fixedMouseJoint = new FixedMouseJoint(body, positionWorld2D);
                    _fixedMouseJoint.MaxForce = 100f * body.Mass;
                    World2D.Add(_fixedMouseJoint);
                    body.Awake = true;
                }
            }
        }

        /// <summary>
        /// Pustí uchopené těleso.
        /// </summary>
        public void ReleaseGrabbedBody()
        {
            if (_fixedMouseJoint != null)
            {
                World2D.Remove(_fixedMouseJoint);
                _fixedMouseJoint = null;
            }
        }
    }
}