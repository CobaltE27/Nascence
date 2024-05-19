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

    public GameObject steamPanel;
    private RectTransform steamPanelTransform;
	public UnityEngine.UI.Image capacityBar;
	public UnityEngine.UI.Image baseBar;
	public UnityEngine.UI.Image steamBar;
    private int maxPossibleCapacity = 250;

	public void Start()
    {
        healthLayout = healthPanel.GetComponent<HorizontalLayoutGroup>();
		steamPanelTransform = steamPanel.GetComponent<RectTransform>();
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

    public void UpdateSteamLevel(int newSteam)
	{
		EditBarValue(steamBar, newSteam);
	}

	private void EditBarValue(UnityEngine.UI.Image bar, int newValue)
	{
		float panelWidth = steamPanelTransform.rect.width;
		float valueFractionOfMax = (float)newValue / maxPossibleCapacity;
		float newBarWidth = panelWidth * valueFractionOfMax;

		RectTransform barTransform = bar.GetComponent<RectTransform>();
		barTransform.sizeDelta = new Vector2(newBarWidth, barTransform.sizeDelta.y);
		barTransform.anchoredPosition = new Vector2(newBarWidth / 2, 0);
	}

	/// <summary>
	/// Updates the UI to reflect the given base steam and extra capacity
	/// </summary>
	/// <param name="newBase"></param>
	/// <param name="newExtraCapacity">Remember that this is capacity beyond the base capacity</param>
	public void SetSteamParameters(int newBase, int newExtraCapacity) //for now assumes max capacity is 5 hits of steam
    {
        EditBarValue(baseBar, newBase);
        EditBarValue(capacityBar, newBase + newExtraCapacity);
    }
}
