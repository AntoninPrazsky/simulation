using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prazsky.Tools;

namespace Prazsky.Simulation
{
	/// <summary>
	/// Představuje trojrozměrnou kulisu tvořenou trojrozměrným modelem kdekoliv v trojrozměrném světě.
	/// </summary>
	public class Backdrop3D : Object3D
	{
		/// <summary>
		/// Konstruktor trojrozměrné kulisy.
		/// </summary>
		/// <param name="model3D">Trojrozměrný model pro vykreslování.</param>
		/// <param name="position">Statickká pozice modelu.</param>
		public Backdrop3D(Model model3D, Vector3 position)
		{
			Model = model3D;
			Position = position;

			BoundingSphere = Geometry.GetBoundingSphere(Model);

			Transformations = new Matrix[Model.Bones.Count];
			Model.CopyAbsoluteBoneTransformsTo(Transformations);

			World = Matrix.CreateTranslation(Position);

			BoundingSphere = new BoundingSphere(Position, BoundingSphere.Radius);
		}
	}
}