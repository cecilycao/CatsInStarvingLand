namespace DynamicLight2D
{
	using UnityEngine;
	using System.Collections;

#if UNITY_EDITOR
	using UnityEditor;
	[ExecuteInEditMode]
#endif


	public class CookieSpriteLight : AddOnBase {

		// Tags array is used for search results in inspector
		public static string []tags = {"emitter", "radial", "cookie", "sprite", "custom"};
		
		// Brief description of behavior in this Add-on
		public static string description = "Convert current Point Light into Cookie light";

        [FieldDescriptionAttribute("DROP SPRITE HERE!", "cyan")] [SerializeField] Texture2D CookieSprite = null;

       
	
		Material inst = null;
        Material lastMat;

		// Use this for initialization
		public override void Start () {
            #if UNITY_EDITOR
            if (inst == null){

                inst = AssetDatabase.LoadAssetAtPath(CoreUtils.MainPath() + "Prefabs/Lights/Materials/StandardCookieSpriteLight.mat", typeof(Material)) as Material;
                lastMat = dynamicLightInstance.LightMaterial;
                    
            }

            dynamicLightInstance.LightMaterial = inst;
            #endif
        }


        // Update is called once per frame
        public override void Update () {

			base.Update();

            if ((CookieSprite != null) && (Texture2D)inst.mainTexture != CookieSprite)
            {
                inst.mainTexture = CookieSprite;
            }
		}


		void OnDestroy () {
			inst = null;

            dynamicLightInstance.LightMaterial = lastMat;
		}
	}
}