using UnityEngine;
using UnityEngine.EventSystems;

public class TraitBadge : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public PlayerTrait Trait;

    public void OnPointerEnter(PointerEventData _)
        => CharacterSelectManager.Instance?.ShowTraitTooltip(Trait, transform as RectTransform);

    public void OnPointerExit(PointerEventData _)
        => CharacterSelectManager.Instance?.HideTraitTooltip();
}
