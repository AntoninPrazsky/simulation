using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prazsky.Simulation;
using Prazsky.Simulation.Factories;
using tainicom.Aether.Physics2D.Dynamics;

namespace Demo.Scenes
{
	/// <summary>
	/// Třída FrictionScene demonstruje na pěti objektech tvaru kvádru (s dvojrozměrnou reprezentací ve tvaru čtverce)
	/// a pěti rampách simulaci kinematického tření. Všechny simulované objekty mají stejnou velikost, hustotu a
	/// výchozí pozici (vzájemně spolu ovšem nemohou kolidovat a vykreslují se na odlišných pozicích na ose Z).
	/// Postupně od nejvzdálenějšího (po ose Z) se snižuje jejich koeficient tření od hodnoty 0.75f až na hodnotu 0f.
	///
	/// Po spuštění simulace lze pozorovat, že těleso s nejvyšší hodnotou koeficientu tření se zastavilo už na druhé
	/// rampě, kdežto těleso s nulovým koeficientem pokračuje v pohybu i po sklouznutí ze všech ramp. Nulový koeficient
	/// tření je ve skutečném světě prakticky nereálný a zde je přítomen pouze pro srovnání.
	/// </summary>
	internal class FrictionScene : Scene
	{
		private Model _rampAModel, _rampBModel, _cubeModel;
		private MultipleBody3DCreator _creator;
		private Model _leftBlock, _rightBlock;

		public FrictionScene(SimulationDemo demo) : base(demo)
		{
			_cubeModel = Demo.Content.Load<Model>("Models/Primitives/cube");
			_rampAModel = Demo.Content.Load<Model>("Models/Obstacles/rampA");
			_rampBModel = Demo.Content.Load<Model>("Models/Obstacles/rampB");
			_leftBlock = Demo.Content.Load<Model>("Models/groundBlockLongLeft");
			_rightBlock = Demo.Content.Load<Model>("Models/groundBlockLongRight");

			_creator = new MultipleBody3DCreator(Demo.GraphicsDevice, Demo.World3D.World2D);
		}

		public override void Construct()
		{
			Demo.Window.Title = "Friction Demo";

			Demo.Camera3D.Position = new Vector3(-46.62277f, 105.9008f, 79.68979f);
			Demo.Camera3D.Target = new Vector3(-46.25956f, 105.3427f, 78.94375f);

			ConstructGround(23);
			Body3D body3D =
				Body3DFactory.CreateBody3D(
					_leftBlock,
					Demo.World3D.World2D,
					Demo.GraphicsDevice,
					new Vector2(-59.15f, 5.9f),
					BodyType.Static);
			body3D.Body2D.SetCollisionCategories(Category.Cat31);
			Demo.World3D.AddBody3D(body3D);

			body3D =
				Body3DFactory.CreateBody3D(
					_rightBlock,
					Demo.World3D.World2D,
					Demo.GraphicsDevice,
					new Vector2(59.15f, 5.9f),
					BodyType.Static);
			body3D.Body2D.SetCollisionCategories(Category.Cat31);
			Demo.World3D.AddBody3D(body3D);

			ConstructRamps();
		}

		private void AddCube(float friction, float positionZ, float positionX, Category collisionCategory)
		{
			Body body = new Body();
			body.CreateRectangle(2f, 2f, 1f, Vector2.Zero);
			body.SetFriction(friction);
			body.SetCollidesWith(collisionCategory);

			Body3D body3D =
				Body3DFactory.CreateBody3D(
					_cubeModel,
					Demo.World3D.World2D,
					body,
					new Vector2(positionX, 100f));
			body3D.PositionZ = positionZ;

			Demo.World3D.AddBody3D(body3D);
		}

		private void AddRamp(bool typeA, Vector2 position)
		{
			Body3D ramp = _creator.CreateBody3D(typeA ? _rampAModel : _rampBModel, position, BodyType.Static);
			ramp.Body2D.SetCollisionCategories(Category.Cat31);
			Demo.World3D.AddBody3D(ramp);
		}

		private void ConstructRamps()
		{
			AddRamp(true, new Vector2(13f, 15f));
			AddRamp(false, new Vector2(-13f, 30f));
			AddRamp(true, new Vector2(13f, 45f));
			AddRamp(false, new Vector2(-13f, 60f));
			AddRamp(true, new Vector2(13f, 75f));

			AddCube(0.75f, -6.5f, 10f, Category.Cat31);
			AddCube(0.45f, -3f, 10f, Category.Cat31);
			AddCube(0.28f, -0f, 10f, Category.Cat31);
			AddCube(0.17f, 3f, 10f, Category.Cat31);
			AddCube(0f, 6.5f, 10f, Category.Cat31);
		}
	}
}