using Microsoft.Xna.Framework;

namespace Prazsky.Simulation
{
    public class BasicEffectParams
    {
        public BasicEffectParams(Vector3 ambientLightColor, Vector3 specularColor, float specularPower, Vector3 emmisiveColor, DirectionalLight directionalLight0, DirectionalLight directionalLight1, DirectionalLight directionalLight2, Fog fog)
        {
            AmbientLightColor = ambientLightColor;
            SpecularColor = specularColor;
            SpecularPower = specularPower;
            EmmisiveColor = emmisiveColor;
            DirectionalLight0 = directionalLight0;
            DirectionalLight1 = directionalLight1;
            DirectionalLight2 = directionalLight2;
            Fog = fog;
        }

        public Vector3 AmbientLightColor { get; set; }
        public Vector3 SpecularColor { get; set; }
        public float SpecularPower { get; set; }
        public Vector3 EmmisiveColor { get; set; }

        public DirectionalLight DirectionalLight0 { get; set; }
        public DirectionalLight DirectionalLight1 { get; set; }
        public DirectionalLight DirectionalLight2 { get; set; }

        public Fog Fog { get; set; }
    }

    public class DirectionalLight
    {
        public DirectionalLight(Vector3 direction, Vector3 diffuseColor, Vector3 specularColor)
        {
            Direction = direction;
            DiffuseColor = diffuseColor;
            SpecularColor = specularColor;
        }

        public Vector3 Direction { get; set; }
        public Vector3 DiffuseColor { get; set; }
        public Vector3 SpecularColor { get; set; }
    }

    public class Fog
    {
        public Fog(Vector3 fogColor, float fogStart, float fogEnd)
        {
            FogColor = fogColor;
            FogStart = fogStart;
            FogEnd = fogEnd;
        }

        public Vector3 FogColor { get; set; }
        public float FogStart { get; set; }
        public float FogEnd { get; set; }
    }
}