using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prazsky.Tools;
using tainicom.Aether.Physics2D.Dynamics;

namespace Prazsky.Simulation
{
	/// <summary>
	/// Představuje trojrozměrný objekt, který je simulovatelný.
	/// </summary>
	public class Body3D : Object3D
	{
		private bool _simEnabled = true;
		private bool _asleep = false;

		/// <summary>
		/// Dvourozměrné těleso pro simulaci fyzikální knihovnou.
		/// </summary>
		public Body Body2D { get; }

		/// <summary>
		/// Výchozí typ simulovaného dvourozměrného tělesa, se kterým bylo inicializováno.
		/// </summary>
		public BodyType DefaultBodyType { get; }

		/// <summary>
		/// Pozice trojrozměrného modelu na ose Z.
		/// </summary>
		public float PositionZ { set; get; }

		/// <summary>
		/// Konstruktor trojrozměrného objektu s podporou dvourozměrné fyzikální simulace.
		/// </summary>
		/// <param name="model3D">Trojrozměrný model pro vykreslování.</param>
		/// <param name="physicalBody2D">Dvourozměrná fyzikální reprezentace modelu pro fyzikální simulaci
		/// knihovnou.</param>
		/// <param name="positionZ">Výchozí pozice modelu na ose Z.</param>
		public Body3D(Model model3D, Body physicalBody2D, float positionZ = 0f)
		{
			Model = model3D;
			Body2D = physicalBody2D;
			PositionZ = positionZ;

			BoundingBox = Geometry.GetBoundingBox(Model);
			BoundingSphere = Geometry.GetBoundingSphere(BoundingBox.Max);

			DefaultBodyType = Body2D.BodyType;

			Transformations = new Matrix[Model.Bones.Count];
			Model.CopyAbsoluteBoneTransformsTo(Transformations);
		}

		/// <summary>
		/// Aktualizuje pozici tělesa v trojrozměrném světě na základě dvourozměrné fyzikální simulace.
		/// </summary>
		public void Update3DPosition()
		{
			Position = new Vector3(Body2D.Position.X, Body2D.Position.Y, PositionZ);
			World = Matrix.CreateRotationZ(Body2D.Rotation) * Matrix.CreateTranslation(Position);

			BoundingSphere = new BoundingSphere(Position, BoundingSphere.Radius);
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
		/// Aktivuje fyzikální simulaci tělesa navrácením jeho <see cref="BodyType"/> na výchozí hodnotu.
		/// (<see cref="DefaultBodyType"/>)
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