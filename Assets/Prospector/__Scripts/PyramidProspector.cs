using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
//https://cardgames.io/pyramidsolitaire/


public class PyramidProspector : MonoBehaviour {

	static public PyramidProspector S;

	[Header("Set in Inspector")]
	public TextAsset			deckXML;
	public TextAsset layoutXML;
	public float xOffset = 3;
	public float yOffset = -2.5f;
	public Vector3 layoutCenter;
	public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
	public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
	public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
	public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
	public Text gameOverText, roundResultText, highScoreText;

	[Header("Set Dynamically")]
	public PyramidDeck					deck;
	public Layout layout;
	public List<PyramidCardProspector> drawPile;
	public Transform layoutAnchor;
	public PyramidCardProspector target;
	public List<PyramidCardProspector> tableau;
	public List<PyramidCardProspector> discardPile;
	public List<PyramidCardProspector> dropPile;
	public List<PyramidCardProspector> targetPile;
	public FloatingScore fsRun;
    public PyramidCardProspector chosenCard = null;

    void Awake(){
		S = this;
		SetUpUITexts();
	}

	void SetUpUITexts()
    {
		// Set up the HighScore UI Text 
		GameObject go = GameObject.Find("HighScore");
		if (go != null)
		{
			highScoreText = go.GetComponent<Text>();
		}
		// Set up the UI Texts that show at the end of the round 
		go = GameObject.Find("GameOver");
		if (go != null)
		{
			gameOverText = go.GetComponent<Text>();
		}
		// Make the end of round texts invisible 
		ShowResultsUI(false);
	}
	void ShowResultsUI(bool show)
	{
		gameOverText.gameObject.SetActive(show);
		roundResultText.gameObject.SetActive(show);
	}

	void Start() {
		deck = GetComponent<PyramidDeck> ();
		deck.InitDeck (deckXML.text);
		Deck.Shuffle(ref deck.cards);
		layout = GetComponent<Layout>();  // Get the Layout component 
		layout.ReadLayout(layoutXML.text); // Pass LayoutXML to it
		drawPile = ConvertListCardsToListPyramidCardProspectors(deck.cards);
		LayoutGame();
	}

	List<PyramidCardProspector> ConvertListCardsToListPyramidCardProspectors(List<Card> lCD)
	{
		List<PyramidCardProspector> lCP = new List<PyramidCardProspector>();
		PyramidCardProspector tCP;
		foreach (Card tCD in lCD)
		{
			tCP = tCD as PyramidCardProspector;
			lCP.Add(tCP);
		}
		return (lCP);
	}

	PyramidCardProspector Draw()
	{
		PyramidCardProspector cd = drawPile[0]; //Pull the 0th PyramidCardProspector
		drawPile.RemoveAt(0); //Then remove it from List<> drawPile
		return (cd); //And return it
	}

	void LayoutGame()
	{
		if (layoutAnchor == null)
		{
			GameObject tGO = new GameObject("_LayoutAnchor"); 
			layoutAnchor = tGO.transform; 
			layoutAnchor.transform.position = layoutCenter; 
		}

		PyramidCardProspector cp;
		foreach (SlotDef tSD in layout.slotDefs) 
		{
			cp = Draw();
			cp.faceUp = tSD.faceUp;
			cp.transform.parent = layoutAnchor; 

			cp.transform.localPosition = new Vector3(
				layout.multiplier.x * tSD.x,
				layout.multiplier.y * tSD.y,-tSD.layerID); 

			cp.layoutID = tSD.id;
			cp.slotDef = tSD;
			cp.state = CardState.tableau;
			cp.SetSortingLayerName(tSD.layerName);
			tableau.Add(cp); 
		}

		foreach (PyramidCardProspector tCP in tableau)
		{
			foreach (int hid in tCP.slotDef.hiddenBy)
			{
				cp = FindCardByLayoutID(hid);
				if(!tCP.hiddenBy.Contains(cp))
                {
					tCP.hiddenBy.Add(cp);
				}
			}
		}
		// Set up the initial target card 
		MoveToTarget(Draw());

		// Set up the Draw pile
		UpdateDrawPile();
	}

	void MoveToDiscard(PyramidCardProspector cd)
	{
		//Set the state of the card to discard
		cd.state = CardState.discard;
		discardPile.Add(cd); //Add it to the discardPile List<>
		cd.transform.parent = layoutAnchor; //Update its transform parent
		cd.transform.localPosition = new Vector3(
			layout.multiplier.x * layout.discardPile.x,
			layout.multiplier.y * layout.multiplier.y,
			-layout.discardPile.layerID + 0.5f); //Position it on the discardPile
		cd.faceUp = true;

		//Place it on top of the pile for depth sorting
		cd.SetSortingLayerName(layout.discardPile.layerName);
		cd.SetSortOrder(-100 + discardPile.Count);
	}

	//Make cd the new target card
	void MoveToTarget(PyramidCardProspector cd)
	{
		//If there is currently a target card, move it to discardPile
		if (target != null) MoveToDiscard(target);
		target = cd; //cd is the new target
		cd.state = CardState.target;
		targetPile.Add(cd); //Add it to the discardPile List<>
		cd.transform.parent = layoutAnchor;

		//Move to the target position
		cd.transform.localPosition = new Vector3(
			layout.multiplier.x * layout.discardPile.x,
			layout.multiplier.y * layout.multiplier.y,
			-layout.discardPile.layerID);
		cd.faceUp = true; //Make it face-up

		//Set the depth sorting
		cd.SetSortingLayerName(layout.discardPile.layerName);
		cd.SetSortOrder(-100 + discardPile.Count);
		UpdateDrawPile();
	}

	void MoveToDrop(PyramidCardProspector cd)
    {
		cd.state = CardState.drop;
		dropPile.Add(cd); //Add it to the discardPile List<>
		cd.transform.parent = layoutAnchor;
		cd.transform.localPosition = new Vector3(
			layout.multiplier.x * layout.dropPile.x,
			layout.multiplier.y * layout.dropPile.y,
			-layout.dropPile.layerID); //Position it on the discardPile
		cd.faceUp = true;

		//Place it on top of the pile for depth sorting
		print("层名字：" + layout.dropPile.layerName);
		cd.SetSortingLayerName(layout.dropPile.layerName);
		cd.SetSortOrder(-100 + dropPile.Count);
		UpdateDropPile();
	}

	//Arranges all the cards of the drawPile to show how many are left
	void UpdateDrawPile()
	{
		PyramidCardProspector cd;

		//Go through all the cards of the drawPile
		for (int i = 0; i < drawPile.Count; i++)
		{
			cd = drawPile[i];
			cd.transform.parent = layoutAnchor;

			//Position it correctly with the layout.drawPile.stagger
			Vector2 dpStagger = layout.drawPile.stagger;
			cd.transform.localPosition = new Vector3(
				layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
				layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
				layout.drawPile.layerID + 0.1f * i);
			cd.faceUp = false; //Make them all face-down
			cd.state = CardState.drawpile;

			//Set the depth sorting
			cd.SetSortingLayerName(layout.drawPile.layerName);
			cd.SetSortOrder(10 * i);
		}
	}

	void UpdateDiscardPile()
	{
		PyramidCardProspector cd;

		//Go through all the cards of the drawPile
		for (int i = 0; i < discardPile.Count; i++)
		{
			cd = discardPile[i];
			cd.transform.parent = layoutAnchor;

			//Position it correctly with the layout.drawPile.stagger
			Vector2 dpStagger = layout.discardPile.stagger;
			cd.transform.localPosition = new Vector3(
				layout.multiplier.x * (layout.discardPile.x),
				layout.multiplier.y * (layout.discardPile.y),
				-layout.discardPile.layerID + 0.1f * i);
			cd.faceUp = false; //Make them all face-down
			cd.state = CardState.discard;

		}
	}

	void UpdateDropPile()
	{
		PyramidCardProspector cd;

		//Go through all the cards of the drawPile
		for (int i = 0; i < dropPile.Count; i++)
		{
			cd = dropPile[i];
			cd.transform.parent = layoutAnchor;

			//Position it correctly with the layout.drawPile.stagger
			Vector2 dpStagger = layout.dropPile.stagger;
			cd.transform.localPosition = new Vector3(
				layout.multiplier.x * (layout.dropPile.x + i * dpStagger.x),
				layout.multiplier.y * (layout.dropPile.y + i * dpStagger.y),
				-layout.dropPile.layerID + 0.1f * i);
			cd.faceUp = false; //Make them all face-down
			cd.state = CardState.drop;

			//Set the depth sorting
			cd.SetSortingLayerName(layout.dropPile.layerName);
			cd.SetSortOrder(10 * i);
		}
	}

	public void CardClicked(PyramidCardProspector cd)
	{
		//The reaction is determined by the state of the clicked card
		switch (cd.state)
		{
			case CardState.target:
				chosenCard = cd;
				print("已选中卡牌: "+chosenCard);
				//点击后将当前卡属性赋值到chosenCard中
				if(chosenCard.rank == 13)
                {
					MoveToDrop(cd); //Make it the target card
					//ScoreManager.EVENT(eScoreEvent.mine);
					//FloatingScoreHandler(eScoreEvent.mine);
					chosenCard = null;
					break;
				}
				print("这张卡不是K");
				break;
			case CardState.drawpile:
				//Clicking any card in the drawPile will draw the next card
				//MoveToDiscard(target); //Moves the target to the discardPile
				MoveToTarget(Draw()); //Moves the next drawn card to the target
				UpdateDrawPile(); //Restacks the DrawPile
				chosenCard = null;
				//ScoreManager.EVENT(eScoreEvent.draw);
				//FloatingScoreHandler(eScoreEvent.draw);
				break;
			case CardState.tableau:
				bool validMatch = true;
				if (chosenCard != null) //Match with chosenCard that is clicked on target
                {
					print("已选中卡牌: " + chosenCard + " 还有桌面上的卡牌: " + cd);
					if (!AdjacentRank(cd, chosenCard))
					{
						//If it's not an adjacent rank, it's not valid
						validMatch = false;
						print("配对失败！");
						chosenCard = null;
					}
                    else
                    {
						print("配对成功！");
						MoveToDrop(chosenCard); //move to drop pile.
						chosenCard = null;
					}

					if (!validMatch)
					{
						return; //return if not valid
					}
				}
                else
                {
					chosenCard = cd;
					print("选中第一张卡: "+cd);
					//Tableu is 13K
					//Clicking a card in the tableau will check if it's a valid play
					if (!cd.faceUp)
					{
						//If the card is face-down, it's not valid
						validMatch = false;
					}
					if(cd.rank == 13)
                    {
						print("这是张K！");
						chosenCard = null;
					}
					else
                    {
						return;
					}						
				}
				tableau.Remove(cd); //Remove it from the tableau list
				MoveToDrop(cd); //move to discard pile.
				SetTableauFaces();
				//ScoreManager.EVENT(eScoreEvent.mine);
				//FloatingScoreHandler(eScoreEvent.mine);
				break;
		}

		CheckForGameOver();
	}

	void CheckForGameOver()
	{
		//If the tableau is empty, the game is over
		if (tableau.Count == 0)
		{
			//Call GameOver() with a win
			GameOver(true);
			return;
		}

		//If there are still cards in the draw pile, the game's not over
		if (drawPile.Count > 0)
		{
			return;
		}

		//Check for remaining valid plays
		foreach (PyramidCardProspector cd in tableau)
		{
			if (AdjacentRank(cd, target))
			{
				//If there's a valid play, the game's not over
				return;
			}
		}

		//Since there are no valid plays, the game is over
		//Call GameOver with a loss
		GameOver(false);
	}

	void GameOver(bool won)
	{
		int score = ScoreManager.SCORE;
		if (fsRun != null) score += fsRun.score;
		if (won)
		{
			gameOverText.text = "Round Over";
			roundResultText.text = "You won this round!\nRound Score: " + score;
			ShowResultsUI(true);
			//ScoreManager(ScoreEvent.gameWin);
			print("Game Over. You Win!:)");
			ScoreManager.EVENT(eScoreEvent.gameWin);
			FloatingScoreHandler(eScoreEvent.gameWin);
		}
		else
		{
			gameOverText.text = "Game Over";
			if (ScoreManager.HIGH_SCORE <= score)
			{
				string str = "You got the high score!\nHigh score: " + score;
				roundResultText.text = str;
			}
			else
			{
				roundResultText.text = "Your final score was: " + score;
			}
			ShowResultsUI(true);
			//ScoreManager(ScoreEvent.gameLoss);
			print("Game Over. You Lost!:(");
			ScoreManager.EVENT(eScoreEvent.gameLoss);
			FloatingScoreHandler(eScoreEvent.gameLoss);
		}

		//Reload the scene in reloadDelay seconds
		//This will give the score a moment to travel
		Invoke("RestartGame", 5.0f);
	}

	void RestartGame()
    {
		SceneManager.LoadScene("__Prospector_Scene_0");
	}

	public bool AdjacentRank(PyramidCardProspector c0, PyramidCardProspector c1)
	{
		//If either card is face-down, it's not adjacent
		if (!c0.faceUp || !c1.faceUp)
		{
			return (false);
		}

		//If they add up equals to 13
		if (Mathf.Abs(c0.rank + c1.rank) == 13)
		{
			return (true);
		}

		//Joker!
		if (c0.rank == 14 || c1.rank == 14)
		{
			return true;
		}

		//Otherwise, return false
		return false;
	}

	void FloatingScoreHandler(eScoreEvent evt)
	{
		List<Vector2> fsPts;
		switch (evt)
		{
			case eScoreEvent.draw:     // Drawing a card 
			case eScoreEvent.gameWin:  // Won the round 
			case eScoreEvent.gameLoss: // Lost the round 
									   // Add fsRun to the Scoreboard score 
				if (fsRun != null)
				{
					// Create points for the Bézier curve1 
					fsPts = new List<Vector2>();
					fsPts.Add(fsPosRun);
					fsPts.Add(fsPosMid2);
					fsPts.Add(fsPosEnd);
					fsRun.reportFinishTo = Scoreboard.S.gameObject;
					fsRun.Init(fsPts, 0, 1);
					// Also adjust the fontSize 
					fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
					fsRun = null; // Clear fsRun so it's created again 
				}
				break;
			case eScoreEvent.mine: // Remove a mine card 
								   // Create a FloatingScore for this score 
				FloatingScore fs;
				// Move it from the mousePosition to fsPosRun 
				Vector2 p0 = Input.mousePosition;
				p0.x /= Screen.width;

				p0.y /= Screen.height;
				fsPts = new List<Vector2>();
				fsPts.Add(p0);
				fsPts.Add(fsPosMid);
				fsPts.Add(fsPosRun);
				fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts);
				fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
				if (fsRun == null)
				{
					fsRun = fs;
					fsRun.reportFinishTo = null;
				}
				else
				{
					fs.reportFinishTo = fsRun.gameObject;
				}
				break;
		}
	}

	PyramidCardProspector FindCardByLayoutID(int layoutID)
	{
		foreach (PyramidCardProspector tCP in tableau)
		{
			//Search through all cards in the tableau List<>
			if (tCP.layoutID == layoutID)
			{
				//If the card has the same ID, return it
				return tCP;
			}
		}
		//If it'snot found, return null
		return null;
	}

	//This turns cards in the Mine face-up or face-down
	void SetTableauFaces()
	{
		foreach (PyramidCardProspector cd in tableau)
		{
			bool faceUp = true; //Assume the card will be face-up
			foreach (PyramidCardProspector cover in cd.hiddenBy)
			{
				//If either of the covering cards are in the tableau
				if (cover.state == CardState.tableau)
				{
					faceUp = false; //then this card is face-down
				}
			}
			cd.faceUp = faceUp; //Set the value on the card
		}
	}

}
