using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Common.Decomposition;
using tainicom.Aether.Physics2D.Common.PolygonManipulation;
using tainicom.Aether.Physics2D.Dynamics;

namespace Prazsky.Simulation
{
	/// <summary>
	/// Slouží pro vytvoření simulovatelného dvourozměrného tělesa typu <see cref="Body"/> pro knihovnu
	/// <see cref="tainicom.Aether.Physics2D"/> na základě tvaru nalezeného v poskytnuté bitmapě.
	/// Těleso je poté zařazeno do dvourozměrného fyzikálního světa (<see cref="World"/>).
	/// </summary>
	public static class BodyCreator
	{
		/// <summary>
		/// Výchozí vzdálenost mezi vrcholy nalezeného tvaru, které mají být sloučeny (pro zjednodušení tvaru).
		/// </summary>
		public const float DEFAULT_REDUCE_VERTICES_DISTANCE = 1f;

		/// <summary>
		/// Výchozí poměr mezi grafickým zobrazením a simulovaným fyzikálním světem.
		/// </summary>
		public const float DEFAULT_GRAPHICS_TO_SIMULATION_RATIO = 1f / 100f;

		/// <summary>
		/// Výchozí hustota tělesa (počet kilogramů na metr čtvereční).
		/// </summary>
		public const float DEFAULT_DENSITY = 1f;

		/// <summary>
		/// Výchozí rotace tělesa v simulovaném světě.
		/// </summary>
		public const float DEFAULT_ROTATION = 0f;

		/// <summary>
		/// Výchozí algoritmus pro rozdělení tvaru na množství menších konvexních polygonů.
		/// </summary>
		public const TriangulationAlgorithm DEFAULT_TRIANGULATION_ALGORITHM = TriangulationAlgorithm.Bayazit;

		/// <summary>
		/// Výchozí typ tělesa.
		/// </summary>
		public const BodyType DEFAULT_BODY_TYPE = BodyType.Dynamic;

		/// <summary>
		/// Vrátí list vrholů pro sestavení tvaru (složeného mnohoúhelníku) na základě ortogonální projekce
		/// trojrozměrného modelu. Provedení této metody je relativně paměťově a výpočetně náročné v závislosti na
		/// velikosti zdrojové bitmapy a zvoleném algoritmu.
		/// </summary>
		/// <param name="orthographicRender">Bitmapa pro nalezení vrcholů.</param>
		/// <param name="reduceVerticesDistance">Vzdálenost mezi vrcholy nalezeného tvaru, které mají být sloučeny
		/// (zjednodušení tvaru).</param>
		/// <param name="triangulationAlgorithm">Algoritmus pro rozdělení tvaru na množství menších konvexních
		/// polygonů.</param>
		/// <param name="graphicsToSimulationRatio">Poměr mezi grafickým zobrazením a simulovaným fyzikálním
		/// světem.</param>
		/// <returns></returns>
		public static List<Vertices> CreateVerticesForBody(
				Texture2D orthographicRender,
				float reduceVerticesDistance = DEFAULT_REDUCE_VERTICES_DISTANCE,
				TriangulationAlgorithm triangulationAlgorithm = DEFAULT_TRIANGULATION_ALGORITHM,
				float graphicsToSimulationRatio = DEFAULT_GRAPHICS_TO_SIMULATION_RATIO)
		{
			//Pole pro data bitmapové textury
			uint[] data = new uint[orthographicRender.Width * orthographicRender.Height];

			//Přenesení dat textury do pole
			orthographicRender.GetData(data);

			//Nalezení vrcholů tvořících obrys tvaru v textuře
			Vertices textureVertices = PolygonTools.CreatePolygon(data, orthographicRender.Width, false);

			//Snížení počtu nalezených vrcholů (zjednodušení)
			if (reduceVerticesDistance > 0)
				textureVertices = SimplifyTools.ReduceByDistance(textureVertices, reduceVerticesDistance);

			//Střed bitmapy
			Vector2 center = new Vector2(-orthographicRender.Width / 2, -orthographicRender.Height / 2);

			//Vystředění nalezených vrcholů
			textureVertices.Translate(ref center);

			List<Vertices> verticesList = new List<Vertices>();

			if (!textureVertices.IsConvex())
			{
				try
				{
					//Konkávní polygon je nutné rozdělit na množství menších konvexních polygonů s využitím
					//preferovaného algoritmu
					verticesList = Triangulate.ConvexPartition(textureVertices, triangulationAlgorithm);
				}
				catch (Exception ex)
				{
					if (ex.Message == "Intersecting Constraints")
						throw new ArgumentException(
								"Tvar se nepodařilo rozdělit na konvexní polygony, zkuste změnit parametr udávající " +
								"vzdálenost vrcholů pro sloučení nebo použijte jiný algoritmus pro rozdělení.",
								"reduceVerticesDistance, triangulationAlgorithm", ex);
				}
			}
			else
				verticesList.Add(textureVertices);

			//Změna velikost polygonu podle poměru ke grafickému zobrazení
			Vector2 vertScale = new Vector2(graphicsToSimulationRatio);
			foreach (Vertices vertices in verticesList)
			{
				vertices.Scale(new Vector2(1f, -1f));
				vertices.Scale(vertScale);
			}

			return verticesList;
		}

		/// <summary>
		/// Vrátí objekt typu <see cref="Body"/> pro provádění dvourozměrných simulací na základě tvaru nalezeného v
		/// dané bitmapě.
		/// </summary>
		/// <param name="orthographicRender">Bitmapa pro nalezení tvaru tělesa.</param>
		/// <param name="world">Objekt typu <see cref="World"/> fyzikální knihovny představující dvourozměrný svět, do
		/// kterého má být vytvořené těleso zařazeno.</param>
		/// <param name="position">Výchozí pozice tělesa v simulovaném světě.</param>
		/// <param name="bodyType">Typ simulovaného tělesa (statické, kinematické nebo dynamické).</param>
		/// <param name="density">Hustota tělesa (počet kilogramů na metr čtvereční).</param>
		/// <param name="rotation">Výchozí rotace tělesa v simulovaném světě.</param>
		/// <param name="reduceVerticesDistance">Vzdálenost mezi vrcholy nalezeného tvaru, které mají být sloučeny
		/// (zjednodušení tvaru).</param>
		/// <param name="triangulationAlgorithm">Algoritmus pro rozdělení tvaru na množství menších konvexních
		/// polygonů.</param>
		/// <param name="graphicsToSimulationRatio">Poměr mezi grafickým zobrazením a simulovaným fyzikálním
		/// světem.</param>
		/// <returns>Dvourozměrný simulovatelný objekt.</returns>
		public static Body CreatePolygonBody(
				Texture2D orthographicRender,
				World world,
				Vector2 position = new Vector2(),
				BodyType bodyType = DEFAULT_BODY_TYPE,
				float density = DEFAULT_DENSITY,
				float rotation = DEFAULT_ROTATION,
				float reduceVerticesDistance = DEFAULT_REDUCE_VERTICES_DISTANCE,
				TriangulationAlgorithm triangulationAlgorithm = DEFAULT_TRIANGULATION_ALGORITHM,
				float graphicsToSimulationRatio = DEFAULT_GRAPHICS_TO_SIMULATION_RATIO)
		{
			List<Vertices> verticesList = CreateVerticesForBody(orthographicRender, reduceVerticesDistance,
					triangulationAlgorithm, graphicsToSimulationRatio);

			return (world.CreateCompoundPolygon(verticesList, density, position, rotation, bodyType));
		}

		/// <summary>
		/// Vytvoří fyzikální těleso (<see cref="Body"/>) na základě vrcholů (<see cref="Vertices"/>).
		/// </summary>
		/// <param name="verticesList">List vrcholů pro nalezení tvaru.</param>
		/// <param name="world">Objekt typu <see cref="World"/> fyzikální knihovny představující dvourozměrný svět, do
		/// kterého má být vytvořené těleso zařazeno.</param>
		/// <param name="position">Výchozí pozice tělesa v simulovaném světě.</param>
		/// <param name="bodyType">Typ simulovaného tělesa (statické, kinematické nebo dynamické).</param>
		/// <param name="density">Hustota tělesa (počet kilogramů na metr čtvereční).</param>
		/// <param name="rotation">Výchozí rotace tělesa v simulovaném světě.</param>
		/// <returns></returns>
		public static Body CreateBodyFromVertices(
				List<Vertices> verticesList,
				World world,
				Vector2 position = new Vector2(),
				BodyType bodyType = DEFAULT_BODY_TYPE,
				float density = DEFAULT_DENSITY,
				float rotation = DEFAULT_ROTATION)
		{
			return (world.CreateCompoundPolygon(verticesList, density, position, rotation, bodyType));
		}
	}
}