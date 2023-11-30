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

        ISprite CreateSprite();

        SpriteMaterialDescription CreateMaterial();

        void SetupSwap(float h, float s, float l) { }

        ISpriteAsset CreateAnotherInstance();
    }
}
