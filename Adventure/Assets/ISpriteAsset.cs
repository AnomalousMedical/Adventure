using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets
{
    interface ISpriteAsset
    {
        Quaternion GetOrientation() => Quaternion.Identity;

        Sprite CreateSprite();

        SpriteMaterialDescription CreateMaterial();

        void SetupSwap(float h, float s, float l) { }

        public ISpriteAsset CreateAnotherInstance()
        {
            var t = GetType();
            return (ISpriteAsset)t.GetConstructor(new Type[0]).Invoke(new object[0]);
        }
    }
}
