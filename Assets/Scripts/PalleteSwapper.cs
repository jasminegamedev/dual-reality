using System.Collections.Generic;
using UnityEngine;

// This component lets us control what pallet of colors gets rendered. It will render based on the pallet set in the settings.
public class PalleteSwapper : MonoBehaviour
{
    [Tooltip("List of materials to be used for different pallets.")]
    public List<Material> materials;

    [Tooltip("Hardcoded override to force which pallete should be displayed.")]
    public int Override = -1;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        int pallete = PlayerPrefs.GetInt("Pallete", 0);
        if(Override >= 0)
        {
            pallete = Override;
        }
        if(pallete >= 0)
        {
            Graphics.Blit(src, dest, materials[pallete]);
        }
    }
}
