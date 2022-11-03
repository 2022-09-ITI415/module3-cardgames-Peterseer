using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card : MonoBehaviour {

	public string    suit;
	public int       rank;
	public Color     color = Color.black;
	public string    colS = "Black";  // or "Red"
	
	public List<GameObject> decoGOs = new List<GameObject>();
	public List<GameObject> pipGOs = new List<GameObject>();
	
	public GameObject back;  // back of card;
	public CardDefinition def;  // from DeckXML.xml		
	public SpriteRenderer[] spriteRenderers;

	// Use this for initialization
	void Start () {
		SetSortOrder(0);
	}

	public void PopulateSpriteRenderers()
	{
		if (spriteRenderers == null || spriteRenderers.Length == 0)
		{
			spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
		}
	}

	public void SetSortingLayerName(string tSLN)
	{
		PopulateSpriteRenderers();

		foreach (SpriteRenderer tSR in spriteRenderers)
		{
			tSR.sortingLayerName = tSLN;
		}
	}

	//Sets the sortingOrder of all SpriteRenderer Components
	public void SetSortOrder(int sOrd)
	{
		PopulateSpriteRenderers();

		//Iterate through all the spriteRenderers as tSR
		foreach (SpriteRenderer tSR in spriteRenderers)
		{
			if (tSR.gameObject == this.gameObject)
			{
				//if the GameObject is this.gameObject, it's the back-ground
				tSR.sortingOrder = sOrd; //Set its order to sOrd
				continue; //And continue to the next iteration of the loop
			}
			//Each of the children of this GameObject are named. Switch based on the names
			//switch based on the names
			switch (tSR.gameObject.name)
			{
				case "back": //If the name is "back"
					tSR.sortingOrder = sOrd + 2; //Set it to the highest layer to cover other sprites
					break;
				case "face": //If the name is "face"
				default: //or if it's anything else
					tSR.sortingOrder = sOrd + 1; //Set it to the middle layer to be above the background
					break;
			}
		}
	}

	public bool faceUp
	{
		get
		{
			return (!back.activeSelf);
		}

		set
		{
			back.SetActive(!value);
		}
	}

	virtual public void OnMouseUpAsButton()
	{
		print(name); // When clicked, this outputs the card name 
	}
   
	// Update is called once per frame
	   void Update () {
	
	}
} // class Card

[System.Serializable]
public class Decorator{
	public string	type;			// For card pips, tyhpe = "pip"
	public Vector3	loc;			// location of sprite on the card
	public bool		flip = false;	//whether to flip vertically
	public float 	scale = 1.0f;
}

[System.Serializable]
public class CardDefinition{
	public string	face;	//sprite to use for face cart
	public int		rank;	// value from 1-13 (Ace-King)
	public List<Decorator>	
					pips = new List<Decorator>();  // Pips Used
}
