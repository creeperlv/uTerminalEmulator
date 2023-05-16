using System.Collections;
using System.Collections.Generic;
using TerminalEmulator;
using UnityEngine;
using UnityEngine.UI;

public class ImageTextureApplier : TextureApplier
{
    public Image DISP;
    public override void ApplyTexture(Texture2D tex)
    {
        DISP.sprite=Sprite.Create(tex,new Rect(0,0,tex.width,tex.height),new Vector2(.5f,.5f));
    }
}
