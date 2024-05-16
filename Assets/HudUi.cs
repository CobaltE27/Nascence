using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class HudUi : MonoBehaviour
{
    public GameObject healthPanel;
    private HorizontalLayoutGroup healthLayout;
    public Sprite healthIconSprite;

    public void Start()
    {
        healthLayout = healthPanel.GetComponent<HorizontalLayoutGroup>();
    }

    public void UpdateHealth(int newHealth)
    {
        DrawHealthIcons(newHealth);
    }

    /// <summary>
    /// right now there's no difference in response when gaining/losing health so just do this
    /// </summary>
    private void DrawHealthIcons(int num)
    {
        for (int i = 0; i < healthLayout.transform.childCount; i++)
        {
            Destroy(healthLayout.transform.GetChild(i).gameObject); //works because they don't truly get destroyed until the end of the frame
		}

        for (int c = 0; c < num; c++) //add num health icons to newly cleared layout
        {
			GameObject newIcon = new GameObject();
            newIcon.transform.SetParent(healthLayout.transform, false);
            newIcon.transform.localScale *= 0.3f;
            UnityEngine.UI.Image newIconSprite = newIcon.AddComponent<UnityEngine.UI.Image>();
            newIconSprite.sprite = healthIconSprite;
        }
    }
}
