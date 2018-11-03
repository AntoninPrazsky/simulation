using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Common.Decomposition;
using tainicom.Aether.Physics2D.Common.PolygonManipulation;
using tainicom.Aether.Physics2D.Dynamics;

namespace Prazsky.Simulation
{
    /// <summary>
    /// Slouží pro vytvoření simulovatelného dvourozměrného tělesa typu <see cref="Body"/> pro knihovnu <see cref="tainicom.Aether.Physics2D"/> na základě tvaru nalezeného v poskytnuté bitmapě.
    /// Těleso je poté zařazeno do dvourozměrného fyzikálního světa (<see cref="World"/>).
    /// </summary>
    public static class BodyCreator
    {
        private const float DEFAULT_REDUCE_VERTICES_DISTANCE = 1f;
        private const float DEFAULT_GRAPHICS_TO_SIMULATION_RATIO = 1f / 100f;
        private const float DEFAULT_DENSITY = 1f;
        private const float DEFAULT_ROTATION = 0f;

        private const TriangulationAlgorithm TRIANGULATION_ALGORITHM = TriangulationAlgorithm.Bayazit;
        private const BodyType BODY_TYPE = BodyType.Dynamic;

        /// <summary>
        /// Vrátí objekt typu <see cref="Body"/> pro provádění dvourozměrných simulací na základě tvaru nalezeného v dané bitmapě.
        /// </summary>
        /// <param name="orthographicRender">Bitmapa pro nalezení tvaru tělesa.</param>
        /// <param name="world">Objekt typu <see cref="World"/> fyzikální knihovny představující dvourozměrný svět, do kterého má být vytvořené těleso zařazeno.</param>
        /// <param name="position">Výchozí pozice tělesa v simulovaném světě.</param>
        /// <param name="bodyType">Typ tělesa (statické, kinematické nebo dynamické).</param>
        /// <param name="density">Hustota tělesa (počet kilogramů na metr čtvereční).</param>
        /// <param name="rotation">Výchozí rotace tělesa v simulovaném světě.</param>
        /// <param name="reduceVerticesDistance">Vzdálenost mezi vrcholy nalezeného tvaru, které mají být sloučeny (zjednodušení tvaru).</param>
        /// <param name="triangulationAlgorithm">Algoritmus pro rozdělení tvaru na množství menších konvexních polygonů.</param>
        /// <param name="graphicsToSimulationRatio">Poměr mezi grafickým zobrazením a simulovaným fyzikálním světem.</param>
        /// <returns></returns>
        public static Body CreatePolygonBody(
            Texture2D orthographicRender,
            World world,
            Vector2 position = new Vector2(),
            BodyType bodyType = BODY_TYPE,
            float density = DEFAULT_DENSITY,
            float rotation = DEFAULT_ROTATION,
            float reduceVerticesDistance = DEFAULT_REDUCE_VERTICES_DISTANCE,
            TriangulationAlgorithm triangulationAlgorithm = TRIANGULATION_ALGORITHM,
            float graphicsToSimulationRatio = DEFAULT_GRAPHICS_TO_SIMULATION_RATIO)
        {
            //Pole pro data bitmapové textury
            uint[] data = new uint[orthographicRender.Width * orthographicRender.Height];

            //Přenesení dat textury do pole
            orthographicRender.GetData(data);

            //Nalezení vrcholů tvořících obrys tvaru v textuře
            Vertices textureVertices = PolygonTools.CreatePolygon(data, orthographicRender.Width, false);

            //Střed bitmapy
            Vector2 center = new Vector2(-orthographicRender.Width / 2, -orthographicRender.Height / 2);

            //Vystředění nalezených vrcholů
            textureVertices.Translate(ref center);

            //Snížení počtu nalezených vrcholů (zjednodušení)
            textureVertices = SimplifyTools.ReduceByDistance(textureVertices, reduceVerticesDistance);

            //Konkávní polygon je nutné rozdělit na množství menších konvexních polygonů s využitím preferovaného algoritmu
            List<Vertices> list = Triangulate.ConvexPartition(textureVertices, triangulationAlgorithm);

            //Změna velikost polygonu
            Vector2 vertScale = new Vector2(graphicsToSimulationRatio);
            foreach (Vertices vertices in list)
            {
                vertices.Scale(new Vector2(1f, -1f));
                vertices.Scale(vertScale);
            }

            return (world.CreateCompoundPolygon(list, density, position, rotation, bodyType));
        }
    }
}