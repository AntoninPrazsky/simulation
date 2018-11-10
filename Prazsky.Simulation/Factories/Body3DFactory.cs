using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prazsky.Render;
using Prazsky.Simulation.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Dynamics;

namespace Prazsky.Simulation.Factories
{
    public static class Body3DFactory
    {
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

            Body3D body3D = new Body3D(model, body2D);
            body3D.BasicEffectParams = basicEffectParams;

            return body3D;
        }
    }
}
