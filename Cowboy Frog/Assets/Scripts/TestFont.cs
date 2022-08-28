using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestFont : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string[] fontPaths = Font.GetPathsToOSFonts();

        // Create new font object from one of those paths
        Font osFont = new Font(fontPaths[30]);

        // Create new dynamic font asset
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(osFont);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
