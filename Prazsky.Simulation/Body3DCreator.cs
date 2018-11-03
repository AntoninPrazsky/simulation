using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prazsky.Simulation
{
    public class Body3DCreator : IDisposable
    {
        private GraphicsDevice _graphicsDevice;

        public Body3DCreator(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }



        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
