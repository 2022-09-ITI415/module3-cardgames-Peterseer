using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardState
{
	drawpile,
	tableau,
	target,
	discard
}

public class PyramidCardProspector : Card
{ 
	public CardState state = CardState.drawpile;

	public List<PyramidCardProspector> hiddenBy = new List<PyramidCardProspector>();

	public int layoutID;

	public SlotDef slotDef;

	override public void OnMouseUpAsButton()
	{
		//Call the CardClicked method on the Prospector singleton
		PyramidProspector.S.CardClicked(this);

		//Also call the base class (Card.cs) version of this method
		base.OnMouseUpAsButton();
	}
}
