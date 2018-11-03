using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prazsky.Simulation.Camera;
using tainicom.Aether.Physics2D.Dynamics;
using Prazsky.Render;

namespace Prazsky.Simulation
{
    /// <summary>
    /// Představuje trojrozměrný objekt, který je simulovatelný.
    /// </summary>
    public class Body3D
    {
        private Model _model3D;
        private Matrix _world = Matrix.Identity;
        private Vector3 _position3D = Vector3.Zero;
        private BoundingSphere _boundingSphere;
        private bool _simEnabled = true;
        private bool _asleep = false;
        private Matrix[] _transformation;

        /// <summary>
        /// Dvourozměrné těleso pro simulaci fyzikální knihovnou.
        /// </summary>
        public Body Body2D { get; }

        /// <summary>
        /// Výchozí typ simulovaného dvourozměrného tělesa, se kterým bylo inicializováno.
        /// </summary>
        public BodyType DefaultBodyType { get; }

        /// <summary>
        /// Aktivace nebo deaktivace defaultního tříbodového osvětelní trojrozměrného modelu, které je definováno frameworkem MonoGame.
        /// Skládá se z hlavního, doplňkového a zadního světla.
        /// https://blogs.msdn.microsoft.com/shawnhar/2007/04/09/the-standard-lighting-rig/
        /// </summary>
        public bool EnableDefaultLighting { set; get; } = true;

        /// <summary>
        /// Aktivace nebo deaktivace výpočtu osvětlení pro každý pixel při vykreslování.
        /// Při deaktivaci nebo pokud grafické zařízení tento způsob nepodporuje, je osvětlení aplikováno na základě každého vrcholu modelu (vertex lighting).
        /// </summary>
        public bool PreferPerPixelLighting { set; get; } = true;

        /// <summary>
        /// Parametry třídy (<see cref="BasicEffect"/>) frameworku MonoGame pro vykreslování obsažené ve třídě (<see cref="Render.BasicEffectParams"/>).
        /// Používá se, pokud se nepoužívá defaultní osvětlení aktivované parametrem (<see cref="EnableDefaultLighting"/>).
        /// </summary>
        public BasicEffectParams BasicEffectParams { get; set; }

        /// <summary>
        /// Opsaná sféra trojrozměrného modelu.
        /// </summary>
        public BoundingSphere BoundingSphere { get => _boundingSphere; }

        /// <summary>
        /// Pozice trojrozměrného modelu na ose Z.
        /// </summary>
        public float PositionZ { set; get; }

        /// <summary>
        /// Konstruktor trojrozměrného objektu s podporou dvourozměrné fyzikální simulace.
        /// </summary>
        /// <param name="model3D">Trojrozměrný model pro vykreslování.</param>
        /// <param name="physicalBody2D">Dvourozměrná fyzikální reprezentace modelu pro fyzikální simulaci knihovnou.</param>
        /// <param name="positionZ">Výchozí pozice modelu na ose Z.</param>
        public Body3D(Model model3D, Body physicalBody2D, float positionZ = 0f)
        {
            _model3D = model3D;
            Body2D = physicalBody2D;
            PositionZ = positionZ;

            _boundingSphere = Geometry.GetBoundingSphere(_model3D);
            DefaultBodyType = Body2D.BodyType;

            _transformation = new Matrix[_model3D.Bones.Count];
            _model3D.CopyAbsoluteBoneTransformsTo(_transformation);
        }

        /// <summary>
        /// Vykreslí jeden snímek trojrozměrného modelu na odpovídající pozici a s odpovídající rotací na základě dvourozměrné fyzikální simulace.
        /// Aplikuje buď vychozí efekt osvětlení, pokud je povolen parametrem <see cref="EnableDefaultLighting"/>, nebo efekt definovaný parametrem <see cref="BasicEffectParams"/>, nebo žádný efekt.
        /// </summary>
        public void Draw(ICamera camera)
        {
            ModelRenderer.Render(_model3D, _transformation, ref camera, ref _world, BasicEffectParams, EnableDefaultLighting, PreferPerPixelLighting);
        }

        /// <summary>
        /// Aktualizuje pozici tělesa v trojrozměrném světě na základě dvourozměrné fyzikální simulace.
        /// </summary>
        public void Update3DPosition()
        {
            _position3D = new Vector3(Body2D.Position.X, Body2D.Position.Y, PositionZ);
            _world = Matrix.CreateRotationZ(Body2D.Rotation) * Matrix.CreateTranslation(_position3D);

            _boundingSphere.Center = _position3D;
        }

        /// <summary>
        /// Uspí těleso funkcionalitou fyzikální knihovny (<see cref="Body.Awake"/>).
        /// </summary>
        public void Sleep()
        {
            if (!_asleep)
            {
                Body2D.Awake = false;
                _asleep = true;
            }
        }

        /// <summary>
        /// Probudí těleso funkcionalitou fyzikální knihovny (<see cref="Body.Awake"/>).
        /// </summary>
        public void WakeUp()
        {
            if (_asleep)
            {
                Body2D.Awake = true;
                _asleep = false;
            }
        }

        /// <summary>
        /// Deaktivuje fyzikální simulaci tělesa změnou jeho <see cref="BodyType"/> na <see cref="BodyType.Static"/>.
        /// Nemá smysl pro tělesa typu <see cref="BodyType.Kinematic"/> nebo <see cref="BodyType.Static"/>.
        /// </summary>
        public void DisableSimulation()
        {
            if (_simEnabled)
            {
                if (DefaultBodyType == BodyType.Static || DefaultBodyType == BodyType.Kinematic) return;

                Body2D.BodyType = BodyType.Static;

                _simEnabled = false;
            }
        }

        /// <summary>
        /// Aktivuje fyzikální simulaci tělesa navrácením jeho <see cref="BodyType"/> na výchozí hodnotu (<see cref="DefaultBodyType"/>)
        /// Nemá smysl pro tělesa typu <see cref="BodyType.Kinematic"/> nebo <see cref="BodyType.Static"/>.
        /// </summary>
        public void EnableSimulation()
        {
            if (!_simEnabled)
            {
                if (DefaultBodyType == BodyType.Static || DefaultBodyType == BodyType.Kinematic) return;

                Body2D.BodyType = DefaultBodyType;

                _simEnabled = true;
            }
        }
    }
}