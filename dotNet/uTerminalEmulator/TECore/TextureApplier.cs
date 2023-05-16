using System;
using UnityEngine;

namespace TerminalEmulator
{
    public class TextureApplier : MonoBehaviour
    {
        public virtual void ApplyTexture(Texture2D tex) {}
    }
    public class MaterialTextureApplier : TextureApplier
    {
        public Material TargetMaterial;
        public string TextureName;
        public override void ApplyTexture(Texture2D tex)
        {

            if (TargetMaterial != null)
            {
                if (TextureName != "")
                    TargetMaterial.SetTexture(TextureName , tex);
                else TargetMaterial.mainTexture = tex;
            }
        }
    }
}
