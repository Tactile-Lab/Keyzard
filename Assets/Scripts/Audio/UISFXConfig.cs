using UnityEngine;

[CreateAssetMenu(fileName = "UISFXConfig", menuName = "Audio/UISFXConfig")]
public class UISFXConfig : DomainSFXAudioConfig
{
	public override bool SupportsKey(SFXEventKey key)
	{
		switch (key)
		{
			case SFXEventKey.UIMenuMove:
			case SFXEventKey.UIMenuConfirm:
			case SFXEventKey.UIOpen:
			case SFXEventKey.UIClose:
			case SFXEventKey.UISceneTransition:
			case SFXEventKey.UIGlossaryOpen:
			case SFXEventKey.UIGlossaryClose:
				return true;
			default:
				return false;
		}
	}
}
