using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prazsky.Render;
using tainicom.Aether.Physics2D.Dynamics;

namespace Prazsky.Simulation.Factories
{
    /// <summary>
    /// Vytváří objekty typu <see cref="Body3D"/> na základě zadaných parametrů.
    /// </summary>
    public static class Body3DFactory
    {
        /// <summary>
        /// Vrátí objekt typu <see cref="Body3D"/>.
        /// </summary>
        /// <param name="model">Trojrozměrný model.</param>
        /// <param name="world2D">Dvourozměrný svět, do kterého má být těleso zařazeno.</param>
        /// <param name="graphicsDevice">Grafické zařízení.</param>
        /// <param name="position">Výchozí pozice objektu v dvourozměrném světě.</param>
        /// <param name="bodyType">Typ simulovaného tělesa (statické, kinematické nebo dynamické).</param>
        /// <param name="basicEffectParams">Parametry pro třídu <see cref="BasicEffect"/>.</param>
        /// <returns></returns>
        public static Body3D CreateBody3D(
            Model model,
            World world2D,
            GraphicsDevice graphicsDevice,
            Vector2 position = new Vector2(),
            BodyType bodyType = BodyType.Dynamic,
            BasicEffectParams basicEffectParams = null)
        {
            Texture2D orthoRender = BitmapRenderer.RenderOrthographic(graphicsDevice, model);
            Body body2D = BodyCreator.CreatePolygonBody(orthoRender, world2D, position, bodyType);
            orthoRender.Dispose();

            Body3D body3D = new Body3D(model, body2D)
            {
                BasicEffectParams = basicEffectParams
            };

            return body3D;
        }

        /// <summary>
        /// Vrátí objekt typu <see cref="Body3D"/>.
        /// </summary>
        /// <param name="model">Trojrozměrný model.</param>
        /// <param name="world2D">Dvourozměrný svět, do kterého má být těleso zařazeno.</param>
        /// <param name="body">Dvourozměrné těleso.</param>
        /// <param name="position">Výchozí pozice objektu v dvourozměrném světě.</param>
        /// <param name="bodyType">Typ simulovaného tělesa (statické, kinematické nebo dynamické).</param>
        /// <param name="basicEffectParams">Parametry pro třídu <see cref="BasicEffect"/>.</param>
        /// <returns></returns>
        public static Body3D CreateBody3D(
            Model model,
            World world2D,
            Body body,
            Vector2 position = new Vector2(),
            BodyType bodyType = BodyType.Dynamic,
            BasicEffectParams basicEffectParams = null)
        {
            body.Position = position;
            body.BodyType = bodyType;
            world2D.Add(body);

            Body3D body3D = new Body3D(model, body)
            {
                BasicEffectParams = basicEffectParams
            };

            return body3D;
        }
    }
}