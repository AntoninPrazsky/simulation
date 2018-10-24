using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prazsky.Simulation.Camera;
using tainicom.Aether.Physics2D.Dynamics;

namespace Prazsky.Simulation
{
    public class Body3D
    {
        public Body Body2D { get; }

        private Model _model3D;
        private ICamera _camera;

        private Matrix _world = Matrix.Identity;
        private Vector3 _position3D = Vector3.Zero;
        private BoundingSphere _boundingSphere;

        #region Způsoby vykreslování a grafické efekty

        public bool EnableDefaultLighting { set; get; } = true;
        public bool PreferPerPixelLighting { set; get; } = true;


        #endregion Způsoby vykreslování a grafické efekty

        public float PositionZ { set; get; }

        public BoundingSphere BoundingSphere { set => value = _boundingSphere; get => _boundingSphere; }

        public BodyType DefaultBodyType { get; }

        public int DrawOrder => throw new NotImplementedException();

        public bool Visible => throw new NotImplementedException();

        public BasicEffectParams BasicEffectParams { get; set; }

        /// <summary>
        /// TODO summary
        /// </summary>
        /// <param name="model3D">Trojrozměrný model pro vykreslování</param>
        /// <param name="physicalBody2D">Dvourozměrná fyzikální reprezenttace modelu pro fyzikální simulaci</param>
        /// <param name="camera">Kamera, která drží matici pohledu a projekce</param>
        /// <param name="positionZ">Pozice na ose Z</param>
        public Body3D(Model model3D, Body physicalBody2D, ICamera camera, float positionZ = 0f)
        {
            _model3D = model3D;
            Body2D = physicalBody2D;
            _camera = camera;
            PositionZ = positionZ;

            _boundingSphere = Geometry.GetBoundingSphere(_model3D);
            
            DefaultBodyType = Body2D.BodyType;
        }

        public void Draw()
        {
            Update3DPosition();

            Matrix[] transformation = new Matrix[_model3D.Bones.Count];
            _model3D.CopyAbsoluteBoneTransformsTo(transformation);

            foreach (ModelMesh mesh in _model3D.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    if (EnableDefaultLighting) effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = PreferPerPixelLighting;

                    if (BasicEffectParams != null)
                    {
                        effect.LightingEnabled = true;

                        #region Aplikace světelných parametrů, pokud jsou nastaveny

                        if (BasicEffectParams.AmbientLightColor != null) effect.AmbientLightColor = BasicEffectParams.AmbientLightColor;
                        if (BasicEffectParams.SpecularColor != null)
                        {
                            effect.SpecularColor = BasicEffectParams.SpecularColor;
                            effect.SpecularPower = BasicEffectParams.SpecularPower;
                        }
                        if (BasicEffectParams.EmmisiveColor != null) effect.EmissiveColor = BasicEffectParams.EmmisiveColor;

                        if (BasicEffectParams.DirectionalLight0 != null)
                        {
                            effect.DirectionalLight0.Direction = BasicEffectParams.DirectionalLight0.Direction;
                            effect.DirectionalLight0.DiffuseColor = BasicEffectParams.DirectionalLight0.DiffuseColor;
                            effect.DirectionalLight0.SpecularColor = BasicEffectParams.DirectionalLight0.SpecularColor;
                            effect.DirectionalLight0.Enabled = true;
                        }

                        if (BasicEffectParams.DirectionalLight1 != null)
                        {
                            effect.DirectionalLight1.Direction = BasicEffectParams.DirectionalLight1.Direction;
                            effect.DirectionalLight1.DiffuseColor = BasicEffectParams.DirectionalLight1.DiffuseColor;
                            effect.DirectionalLight1.SpecularColor = BasicEffectParams.DirectionalLight1.SpecularColor;
                            effect.DirectionalLight1.Enabled = true;
                        }

                        if (BasicEffectParams.DirectionalLight2 != null)
                        {
                            effect.DirectionalLight2.Direction = BasicEffectParams.DirectionalLight2.Direction;
                            effect.DirectionalLight2.DiffuseColor = BasicEffectParams.DirectionalLight2.DiffuseColor;
                            effect.DirectionalLight2.SpecularColor = BasicEffectParams.DirectionalLight2.SpecularColor;
                            effect.DirectionalLight2.Enabled = true;
                        }

                        #endregion

                        #region Aplikace efektu mlhy, pokud je nastaven

                        if (BasicEffectParams.Fog != null)
                        {
                            effect.FogColor = BasicEffectParams.Fog.FogColor;
                            effect.FogStart = BasicEffectParams.Fog.FogStart;
                            effect.FogEnd = BasicEffectParams.Fog.FogEnd;
                            effect.FogEnabled = true;
                        }

                        #endregion
                    }

                    effect.World = transformation[mesh.ParentBone.Index] * _world;
                    effect.View = _camera.View;
                    effect.Projection = _camera.Projection;

                }
                mesh.Draw();
            }
        }

        private void Update3DPosition()
        {
            //Pozice modelu v trojrozměrném světě na základě pozice z dvourozměrné fyzikální simulace
            _position3D = new Vector3(Body2D.Position.X, Body2D.Position.Y, PositionZ);
            _world = Matrix.CreateRotationZ(Body2D.Rotation) * Matrix.CreateTranslation(_position3D);



            _boundingSphere.Center = _position3D;
        }

        private bool _simEnabled;
        public void DisableSimulation()
        {
            if (_simEnabled)
            {
                if (DefaultBodyType == BodyType.Static || DefaultBodyType == BodyType.Kinematic) return;

                Body2D.BodyType = BodyType.Static;
                _simEnabled = false;
            }
        }

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