using Sandbox;

namespace Garryware;

public partial class FlashlightPistol : Pistol
{
    protected virtual Vector3 LightOffset => Vector3.Forward * 10;
    
    private SpotLightEntity worldLight;
    private SpotLightEntity viewLight;
    
    public override void Spawn()
    {
        base.Spawn();

        worldLight = CreateLight(false);
        worldLight.SetParent(this, "muzzle", new Transform(LightOffset));
        worldLight.EnableHideInFirstPerson = true;
        worldLight.Enabled = true;
    }
    
    public override void CreateViewModel()
    {
        base.CreateViewModel();

        viewLight = CreateLight(true);
        viewLight.SetParent(ViewModelEntity, "muzzle", new Transform(LightOffset));
        viewLight.EnableViewmodelRendering = true;
        viewLight.Enabled = true;
    }

    private SpotLightEntity CreateLight(bool viewModel)
    {
        return new SpotLightEntity
        {
            Enabled = true,
            DynamicShadows = true,
            Range = viewModel ? 600f : 300f,
            Falloff = 1.5f,
            LinearAttenuation = 0.0f,
            QuadraticAttenuation = 1.0f,
            Brightness = viewModel ? 4 : 1,
            Color = Color.White,
            InnerConeAngle = viewModel ? 15 : 5,
            OuterConeAngle = viewModel ? 30 : 15,
            FogStrength = 1.0f,
            Owner = Owner,
            LightCookie = Texture.Load("materials/effects/lightcookie.vtex")
        };
    }
    
}
