using Microsoft.Xna.Framework;
using System;

namespace Prazsky.Simulation.Camera
{
    /// <summary>
    /// Představuje základní perspektivní kameru. Umožňuje pohyb po osách X, Y a Z a rotaci okolo os X a Y.
    /// </summary>
    public class BasicCamera3D : ICamera
    {
        private const float FAR_PLANE_DISTANCE = 500f;
        private const float MOVE_SPEED = 0.01f;
        private const float NEAR_PLANE_DISTANCE = 0.01f;
        private const float ROTATION_SPEED = 0.001f;
        private Vector3 _defaultTarget = new Vector3(0, 0, -1);
        private Vector3 _defaultUp = Vector3.Up;
        private float _rotationX = 0f;
        private float _rotationY = 0f;

        /// <summary>
        /// Konstruktor základní perspektivní kamery.
        /// </summary>
        /// <param name="position">Pozice kamery v trojrozměrném světě.</param>
        /// <param name="aspectRatio">Poměr stran zobrazení.</param>
        /// <param name="fieldOfView">Zorné pole kamery.</param>
        public BasicCamera3D(Vector3 position, float aspectRatio, float fieldOfView = MathHelper.PiOver4)
        {
            _position = position;
            _aspectRatio = aspectRatio;
            _fieldOfView = fieldOfView;

            Recalculate();
        }

        /// <summary>
        /// Inkrement nebo dekrement pro pohyb kamery po osách X, Y a Z vyjádřený trojrozměrným vektorem.
        /// </summary>
        /// <param name="shift">Složky X, Y a Z vektoru vyjadřují inkrement nebo dekrement pohybu po odpovídajících osách (běžně -1f až 1f).</param>
        /// <param name="gameTime"></param>
        public void Move(Vector3 shift, GameTime gameTime)
        {
            //Pohyb kamery ve směru, kterým se dívá
            _position += _moveSpeed * Vector3.Transform(shift, GetCameraRotation()) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Recalculate();
        }

        /// <summary>
        /// Inkrement nebo dekrement pro pohyb kamery po osách X, Y a Z.
        /// </summary>
        /// <param name="X">Inkrement nebo dekrement pohybu po ose X (běžně -1f až 1f).</param>
        /// <param name="Y">Inkrement nebo dekrement pohybu po ose Y (běžně -1f až 1f).</param>
        /// <param name="Z">Inkrement nebo dekrement pohybu po ose Z (běžně -1f až 1f).</param>
        /// <param name="gameTime"></param>
        public void Move(float X, float Y, float Z, GameTime gameTime)
        {
            Move(new Vector3(X, Y, Z), gameTime);
        }

        /// <summary>
        /// Inkrement nebo dekrement pro rotaci kamery okolo os X a Y vyjádřený dvourozměrným vektorem.
        /// </summary>
        /// <param name="rotation">Složky X a Y vektoru vyjadřují inkrement nebo dekrement rotace okolo odpovídajících os (běžně -1f až 1f).</param>
        /// <param name="gameTime">Herní čas.</param>
        public void Rotate(Vector2 rotation, GameTime gameTime)
        {
            _rotationX += _rotationSpeed * rotation.X * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _rotationY += _rotationSpeed * rotation.Y * (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            //Hodnoty rotace by teoreticky mohly dosáhnout System.Single.MaxValue nebo System.Single.MinValue
            if (_rotationX >= MathHelper.TwoPi || _rotationX <= -MathHelper.TwoPi) _rotationX = 0f;
            if (_rotationY >= MathHelper.TwoPi || _rotationY <= -MathHelper.TwoPi) _rotationY = 0f;

            Recalculate();
        }

        /// <summary>
        /// Inkrement nebo dekrement pro rotaci kamery okolo os X a Y.
        /// </summary>
        /// <param name="X">Inkrement nebo dekrement rotace okolo osy X (běžně -1f až 1f).</param>
        /// <param name="Y">Inkrement nebo dekrement rotace okolo osy Y (běžně -1f až 1f).</param>
        /// <param name="gameTime">Herní čas.</param>
        public void Rotate(float X, float Y, GameTime gameTime)
        {
            Rotate(new Vector2(X, Y), gameTime);
        }

        private Matrix GetCameraRotation()
        {
            return Matrix.CreateRotationX(_rotationX) * Matrix.CreateRotationY(_rotationY);
        }

        private void Recalculate()
        {
            //Rotace kamery okolo osy X a Y
            Matrix rotation = GetCameraRotation();

            //Přepočítání bodu, do kterého se kamera dívá, na základě rotace kamery
            _target = _position + Vector3.Transform(_defaultTarget, rotation);

            //Přepočítání vektoru směřujícího relativně vzhůru od kamery
            _up = Vector3.Transform(_defaultUp, rotation);

            //Přepočítání matice pohledu a projekce
            _view = Matrix.CreateLookAt(_position, _target, _up);
            _projection = Matrix.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, _nearPlane, _farPlane);
        }

        #region Členové ICamera

        private float _farPlane = FAR_PLANE_DISTANCE;
        private float _nearPlane = NEAR_PLANE_DISTANCE;
        private Vector3 _position;

        private Matrix _projection;

        private Vector3 _target = Vector3.Zero;

        private Vector3 _up = Vector3.Up;

        private Matrix _view;

        /// <summary>
        /// Vzdálenost zadní ořezové plochy od pozice kamery. Objekty za touto plochou se nezobrazují.
        /// </summary>
        public float FarPlane
        {
            get => _farPlane;
            set
            {
                if (value <= _nearPlane) throw new ArgumentException("Vzdálenost zadní ořezové plochy nemůže být menší nebo rovna vzdálenosti přední ořezové plochy.", "FarPlane");
                _farPlane = value;
            }
        }

        /// <summary>
        /// Vzdálenost přední ořezové plochy od pozice kamery. Objekty před touto plochou se nezobrazují.
        /// </summary>
        public float NearPlane
        {
            get => _nearPlane;
            set
            {
                if (value >= _farPlane) throw new ArgumentException("Vzdálenost přední ořezové plochy nemůže být větší nebo rovna vzdálenosti zadní ořezové plochy.", "NearPlane");
                if (value <= 0) throw new ArgumentException("Vzdálenost přední ořezové plochy nemůže být menší nebo rovna nule.", "NearPlane");
                _nearPlane = value;
            }
        }

        /// <summary>
        /// Pozice kamery v trojrozměrném světě.
        /// </summary>
        public Vector3 Position { get => _position; set => _position = value; }

        /// <summary>
        /// Matice projekce.
        /// </summary>
        public Matrix Projection { get => _projection; }

        /// <summary>
        /// Bod v trojrozměrném prostoru, do kterého se kamera dívá.
        /// </summary>
        public Vector3 Target { get => _target; }

        /// <summary>
        /// Vektor udávající směr nahoru relativně ke kameře.
        /// </summary>
        public Vector3 Up { get => _target; }

        /// <summary>
        /// Matice pohledu.
        /// </summary>
        public Matrix View { get => _view; }

        #endregion Členové ICamera

        #region Parametry pro perspektivní a pohyblivou kameru

        private float _aspectRatio;
        private float _fieldOfView = MathHelper.PiOver4;

        private float _moveSpeed = MOVE_SPEED;

        private float _rotationSpeed = ROTATION_SPEED;

        /// <summary>
        /// Poměr stran zobrazení.
        /// </summary>
        public float AspectRatio { get => _aspectRatio; set => _aspectRatio = value; }

        /// <summary>
        /// Zorné pole kamery. Musí být větší než 0 a menší než π (0° - 180°). Jiné hodnoty jsou oříznuty do tohoto intervalu.
        /// </summary>
        public float FieldOfView { get => _fieldOfView; set => _fieldOfView = MathHelper.Clamp(value, float.Epsilon, MathHelper.Pi); }

        /// <summary>
        /// Relativní rychlost inkrementálního pohybu kamery.
        /// </summary>
        public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }

        /// <summary>
        /// Relativní rychlost inkrementální rotace kamery.
        /// </summary>
        public float RotationSpeed { get => _rotationSpeed; set => _rotationSpeed = value; }

        #endregion Parametry pro perspektivní a pohyblivou kameru
    }
}