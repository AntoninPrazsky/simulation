using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prazsky.Simulation.Camera
{
    public interface ICamera
    {
        /// <summary>
        /// Pozice kamery v trojrozměrném světě.
        /// </summary>
        Vector3 Position { get; }
        
        /// <summary>
        /// Směr, kterým se kamera dívá.
        /// </summary>
        Vector3 Target { get; }
        
        /// <summary>
        /// Vektor představující směr nahoru kamery.
        /// </summary>
        Vector3 Up { get; }

        /// <summary>
        /// Matice pohledu.
        /// </summary>
        Matrix View { get; }

        /// <summary>
        /// Matice projekce.
        /// </summary>
        Matrix Projection { get; }

        /// <summary>
        /// Přední ořezová plocha.
        /// </summary>
        float NearPlane { get; set; }

        /// <summary>
        /// Zadní ořezová plocha.
        /// </summary>
        float FarPlane { get; set; }

        //void Update(GameTime gameTime);
    }
}
