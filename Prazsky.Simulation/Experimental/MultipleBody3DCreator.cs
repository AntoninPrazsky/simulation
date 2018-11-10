using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prazsky.Render;
using System.Collections.Generic;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Common.Decomposition;
using tainicom.Aether.Physics2D.Dynamics;

/// <summary>
/// Obsahuje experimentální funkce knihovny.
/// </summary>
namespace Prazsky.Simulation.Experimental
{
    /// <summary>
    /// Slouží pro rychlé vytváření trojrozměrných objektů pro fyzikální simulaci.
    /// </summary>
    public class MultipleBody3DCreator
    {
        private GraphicsDevice _graphicsDevice;
        private Dictionary<Model, List<Vertices>> _modelVerticesPairs = new Dictionary<Model, List<Vertices>>();

        public MultipleBody3DCreator(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public float ReduceVerticesDistance { get; set; } = BodyCreator.DEFAULT_REDUCE_VERTICES_DISTANCE;
        public float GraphicsToSimulationRatio { get; set; } = BodyCreator.DEFAULT_GRAPHICS_TO_SIMULATION_RATIO;
        public float Density { get; set; } = BodyCreator.DEFAULT_DENSITY;
        public float Rotation { get; set; } = BodyCreator.DEFAULT_ROTATION;

        public TriangulationAlgorithm TriangulationAlgorithm { get; set; } = BodyCreator.DEFAULT_TRIANGULATION_ALGORITHM;
        public BodyType BodyType { get; set; } = BodyCreator.DEFAULT_BODY_TYPE;

        public Body3D CreateBody3D(
            Model model,
            World world2D,
            Vector2 position = new Vector2(),
            BodyType bodyType = BodyType.Dynamic,
            BasicEffectParams basicEffectParams = null,
            float positionZ = 0f)
        {
            //Tento model ještě nebyl instancí této třídy zpracováván
            if (!_modelVerticesPairs.ContainsKey(model))
            {
                Texture2D orthoRender = BitmapRenderer.RenderOrthographic(_graphicsDevice, model);

                //Najít v bitmapě tvar je výpočetně náročné, proto se to stane pro každý model zpracovávaný touto třídou jenom jednou
                List<Vertices> verticesList = BodyCreator.CreateVerticesForBody(
                    orthoRender,
                    ReduceVerticesDistance,
                    TriangulationAlgorithm,
                    GraphicsToSimulationRatio);

                orthoRender.Dispose();

                _modelVerticesPairs.Add(model, verticesList);
            }
            return ConstructBody3D(model, world2D, position, bodyType, positionZ, basicEffectParams);
        }

        private Body3D ConstructBody3D(Model model, World world2D, Vector2 position, BodyType bodyType, float positionZ, BasicEffectParams basicEffectParams)
        {
            Body body = BodyCreator.CreateBodyFromVertices(_modelVerticesPairs[model], world2D, position, bodyType,
                Density,
                Rotation);

            Body3D body3D = new Body3D(model, body, positionZ);
            body3D.BasicEffectParams = basicEffectParams;
            return body3D;
        }

        /// <summary>
        /// Sestaví horizontální řadu identických trojrozměrných simulovatelných objektů.
        /// </summary>
        /// <param name="model">Trojrozměrný model pro vytvoření tvaru.</param>
        /// <param name="world3D">Trojrozměrný svět, do kterého má být trojrozměrná řada zařazena.</param>
        /// <param name="count">Počet objektů tvořících řadu.</param>
        /// <param name="center">Souřadnice středu řady.</param>
        /// <param name="bodyType">Typ vytvořených objektů řady (statické, kinematické nebo dynamické).</param>
        /// <param name = "basicEffectParams" > Parametry pro třídu<see cref= "BasicEffect" />.</ param >
        public void BuildRow(Model model, World3D world3D, int count, Vector2 center = new Vector2(), BodyType bodyType = BodyType.Static, BasicEffectParams basicEffectParams = null)
        {
            if (count <= 0) return;

            Body3D body3D = CreateBody3D(model, world3D.World2D, center, bodyType, basicEffectParams);
            world3D.AddBody3D(body3D);

            if (count == 1) return;

            float xSize = body3D.BoundingBox.Max.X * 2;
            int eachSide = count / 2;
            Vector2 position = new Vector2(center.X - xSize, center.Y);

            body3D = CreateBody3D(model, world3D.World2D, position, bodyType, basicEffectParams);
            world3D.AddBody3D(body3D);

            if (count == 2) return;

            for (int i = 2; i <= eachSide; i++)
            {
                position = new Vector2(center.X - i * xSize, center.Y);
                body3D = CreateBody3D(model, world3D.World2D, position, bodyType, basicEffectParams);
                world3D.AddBody3D(body3D);
            }

            for (int i = 1; i < eachSide; i++)
            {
                position = new Vector2(center.X + i * xSize, center.Y);
                body3D = CreateBody3D(model, world3D.World2D, position, bodyType, basicEffectParams);
                world3D.AddBody3D(body3D);
            }

            if (count % 2 != 0)
            {
                position = new Vector2(center.X + eachSide * xSize, center.Y);
                body3D = CreateBody3D(model, world3D.World2D, position, bodyType, basicEffectParams);
                world3D.AddBody3D(body3D);
            }
        }
    }
}