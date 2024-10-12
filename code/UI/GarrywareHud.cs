using System;
using Sandbox;

namespace Garryware.UI;

public class GarrywareHud : PanelComponent
{
	private SimplePopupPanel InstructionPanel { get; set; }
	
	protected override void OnUpdate()
	{
		base.OnUpdate();

		// @todo: for real
		if (Input.Pressed("Jump"))
		{
			ShowInstructions("do something!", 999f);
		}

		if (Input.Pressed("Reload"))
		{
			ShowResult();
		}
	}

	private void ClearInstructions()
	{
		if (!InstructionPanel.IsValid())
			return;

		InstructionPanel.Delete();
		InstructionPanel = null;
	}
	
	private void ShowInstructions(string text, float duration)
	{
		ClearInstructions();

		// Empty instructions just clear the previous ones
		if (text.Length == 0 || duration < 0.1f)
			return;
		
		InstructionPanel = new SimplePopupPanel
		{
			Parent = Panel,
			Type = SimplePopupPanel.PopupType.Instruction,
			Text = text,
			Lifetime = duration
		};
	}

	private void ShowResult()
	{
		// @todo: for real
		new SimplePopupPanel
		{
			Parent = Panel,
			Type = SimplePopupPanel.PopupType.ResultFail,
			Text = "You win!",
			Lifetime = 4
		};
	}
	
}
