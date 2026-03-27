using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilites
{
	public static class LayersAPI
	{
		public static readonly int LayerDefault = LayerMask.NameToLayer("Default");
		public static readonly int LayerTransparentFX = LayerMask.NameToLayer("TransparentFX");
		public static readonly int LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
		public static readonly int LayerWater = LayerMask.NameToLayer("Water");
		public static readonly int LayerUI = LayerMask.NameToLayer("UI");
		public static readonly int LayerProjectiles = LayerMask.NameToLayer("Projectiles");
		public static readonly int LayerCharacters = LayerMask.NameToLayer("Characters");
		public static readonly int LayerEnviroment = LayerMask.NameToLayer("Enviroment");
		public static readonly int LayerLoot = LayerMask.NameToLayer("Loot");
		public static readonly int LayerTextureRender = LayerMask.NameToLayer("TextureRender");

		public static readonly int LayerMaskDefault = 1 << LayerDefault;
		public static readonly int LayerMaskTransparentFX = 1 << LayerTransparentFX;
		public static readonly int LayerMaskIgnoreRaycast = 1 << LayerIgnoreRaycast;
		public static readonly int LayerMaskWater = 1 << LayerWater;
		public static readonly int LayerMaskUI = 1 << LayerUI;
		public static readonly int LayerMaskProjectiles = 1 << LayerProjectiles;
		public static readonly int LayerMaskCharacters = 1 << LayerCharacters;
		public static readonly int LayerMaskEnviroment = 1 << LayerEnviroment;
		public static readonly int LayerMaskLoot = 1 << LayerLoot;
		public static readonly int LayerMaskTextureRender = 1 << LayerTextureRender;

	}
}
