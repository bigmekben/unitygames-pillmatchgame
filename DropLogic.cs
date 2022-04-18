using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropLogic : MonoBehaviour
{
    private const float lowSpeed = .625f; // 38 frames;
    private const float medSpeed = .3125f; // 19 frames;
    private const float highSpeed = .1875f; // 11 frames;

    private bool leftPressed = false;
    private bool rightPressed = false;
    private bool downPressed = false;
    private bool downDebounced = false;
    private bool rotateLeftPressed = false;
    private bool rotateRightPressed = false;

    private float elapsed = 0;
    private const int startPillRow = 0;
    private const int startPillCol = 4;
    private int pillARow = startPillRow;
    private int pillBRow = startPillRow;
    private int pillACol = startPillCol;
    private int pillBCol = startPillCol + 1;
    private GameObject pillA;
    private GameObject pillB;

    private GameObject nextPillA;
    private GameObject nextPillB;

    public GameObject redPill;
    public GameObject greenPill;
    public GameObject bluePill;

    public Arrangement arrangement;
    public SoundPlayer soundPlayer;

    public const int StateReadyingPill = 0;
    public const int StatePlaying = 1;
    public const int StateMatching = 2;
    public const int StateGameOver = 4;
    public const int StateWin = 5;
    public const int StateDroppingSurvivors = 6;
    public int state = StateReadyingPill;

    private float matchFXcountdown = 0;
    private const float matchFXLength = 0.5f;

    private float nextPillDelayCountdown = 0;
    private const float nextPillDelayLength = .5f;

    private float checkSurvivorsCountdown = 0;
    private const float checkSurvivorsLength = .25f;

    // Start is called before the first frame update
    void Start()
    {
        arrangement.InitializeGame();
        state = StateReadyingPill;
        nextPillA = RandomPill(true);
        nextPillB = RandomPill(false);
        nextPillA.transform.position = new Vector3(nextPillA.transform.position.x + 8, nextPillA.transform.position.y - 5, nextPillA.transform .position.z);
        nextPillB.transform.position = new Vector3(nextPillB.transform.position.x + 8, nextPillB.transform.position.y - 5, nextPillB.transform.position.z);
        // to do: transform the next pill somewhere on the side or above the initial location
        NextPill();
    }

    private void NextPill()
    {
        if (arrangement.IsOccupied(startPillRow, startPillCol))
        {
            state = StateGameOver;
            return;
        }
        state = StateReadyingPill;
        elapsed = 0;
        matchFXcountdown = 0;
        nextPillDelayCountdown = 0;

        pillA = nextPillA;
        pillARow = startPillRow;
        pillACol = startPillCol;
        pillA.transform.position = arrangement.RowColToUnits(pillARow, pillACol);
        nextPillA = RandomPill(true);

        pillB = nextPillB;
        pillBRow = startPillRow;
        pillBCol = startPillCol + 1;
        pillB.transform.position = arrangement.RowColToUnits(pillBRow, pillBCol);
        nextPillB = RandomPill(false);
    }

    private GameObject RandomPill(bool isLeft)
    {
        int col = isLeft ? 0 : 1;
        int roll = Mathf.FloorToInt(UnityEngine.Random.value * 3.0f);
        if (roll == 0)
        {
            return Instantiate(redPill, arrangement.RowColToUnits(startPillRow, startPillCol + col + 8) + new Vector3(0, 1, 0), Quaternion.identity);
        }
        else if (roll == 1)
        {
            return Instantiate(greenPill, arrangement.RowColToUnits(startPillRow, startPillCol + col + 8) + new Vector3(0, 1, 0), Quaternion.identity);
        }
        else
        {
            return Instantiate(bluePill, arrangement.RowColToUnits(startPillRow, startPillCol + col + 8) + new Vector3(0, 1, 0), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (arrangement.NoViruses())
        {
            state = StateWin;
            return;
        }

        if (state == StateReadyingPill)
        {
            nextPillDelayCountdown += Time.deltaTime;
            if (nextPillDelayCountdown > nextPillDelayLength)
            {
                nextPillDelayCountdown = 0;
                state = StatePlaying;
            }
            return;
        }

        if (state == StatePlaying)
        {
            int newRowA = pillARow;
            int newRowB = pillBRow;
            int newColA = pillACol;
            int newColB = pillBCol;
            HandleControls();
            if (leftPressed)
            {
                leftPressed = false;
                soundPlayer.PlayMove();
                if (!arrangement.IsOccupied(pillARow, pillACol - 1) && !arrangement.IsOccupied(pillBRow, pillBCol - 1))
                {
                    newColA--;
                    newColB--;
                }
            }
            if (rightPressed)
            {
                leftPressed = false;
                soundPlayer.PlayMove();
                if (!arrangement.IsOccupied(pillARow, pillACol + 1) && !arrangement.IsOccupied(pillBRow, pillBCol + 1))
                {
                    newColA++;
                    newColB++;
                }
            }
            // note: this allows left/right movement at the same time as rotation, in the same update frame.
            if (rotateLeftPressed)
            {
                soundPlayer.PlayRotate();
                if(pillARow == pillBRow)
                {
                    // pill is horizontal before rotating
                    if (!HorizontalRotationBlocked(pillARow, pillACol, pillBRow, pillBCol))
                    {
                        if (pillACol < pillBCol)
                        {
                            // A is on the left
                            newRowB--;
                            newColB--;
                        }
                        else
                        {
                            // B is on the left
                            newRowA--;
                            newColA--;
                        }
                    }
                }
                else
                {
                    // pill is vertical before rotating
                    if(!VerticalRotationBlocked(pillARow, pillACol, pillBRow, pillBCol))
                    {
                        if (pillBRow < pillARow)
                        {
                            // B is on top
                            newRowB++;
                            newColA++;
                        }
                        else
                        {
                            // A is on top
                            newRowA++;
                            newColB++;
                        }
                    }
                }
            }
            if (rotateRightPressed)
            {
                soundPlayer.PlayRotate();
                if (pillARow == pillBRow)
                {
                    // pill is horizontal before rotating
                    if (!HorizontalRotationBlocked(pillARow, pillACol, pillBRow, pillBCol))
                    {
                        if (pillACol < pillBCol)
                        {
                            // A is on the left
                            newRowA--;
                            newColB--;
                        }
                        else
                        {
                            // B is on the left
                            newRowB--;
                            newColA--;
                        }
                    }
                }
                else
                {
                    // pill is vertical before rotating
                    if (!VerticalRotationBlocked(pillARow, pillACol, pillBRow, pillBCol))
                    {
                        if (pillARow < pillBRow)
                        {
                            // A is on top
                            newRowA++;
                            newColA++;
                        }
                        else
                        {
                            // B is on top
                            newRowB++;
                            newColB++;
                        }
                    }
                }
            }

            // if down key is pressed, move to next row, otherwise wait for the time threshold.
            // if down is pressed, the pill will drop one row per update.
            // This might end up dropping it slightly faster than in the NES original.
            if (downPressed)
            {
                elapsed = 0;
                newRowA++;
                newRowB++;
            }
            else
            {
                elapsed += Time.deltaTime;
                if (elapsed > lowSpeed)
                {
                    elapsed = 0;
                    newRowA++;
                    newRowB++;
                }
            }

            if (arrangement.IsOccupied(newRowA, newColA) || arrangement.IsOccupied(newRowB, newColB))
            {
                // LANDED
                downDebounced = true; // we DON'T want the next pill to fly down until the player lets go of the key
                downPressed = false;
                soundPlayer.PlayLanded();
                arrangement.AddPiece(pillARow, pillACol, pillA);
                arrangement.AddPiece(pillBRow, pillBCol, pillB);
                state = CheckForMatches(pillARow, pillACol, ColorOf(pillA), pillBRow, pillBCol, ColorOf(pillB));
                if (state == StateReadyingPill)
                {
                    NextPill();
                }
            }
            else
            {
                // STILL FALLING
                pillA.transform.position = arrangement.RowColToUnits(newRowA, newColA);
                pillARow = newRowA;
                pillACol = newColA;
                pillB.transform.position = arrangement.RowColToUnits(newRowB, newColB);
                pillBRow = newRowB;
                pillBCol = newColB;
            }
        }

        if (state == StateMatching)
        {
            matchFXcountdown += Time.deltaTime;
            if (matchFXcountdown > matchFXLength)
            {
                state = StateDroppingSurvivors;
            }
        }
        if (state == StateDroppingSurvivors)
        {

            checkSurvivorsCountdown += Time.deltaTime;
            if (checkSurvivorsCountdown > checkSurvivorsLength)
            {
                checkSurvivorsCountdown = 0;
                // Let survivors settle by one row.
                bool movement = SettleSurvivorsOneRow();
                if (!movement)
                {
                    int matchCount = CheckEntirePlayfieldForMatches();
                    if (matchCount == 0)
                    {
                        NextPill();  // need to repeatedly look for matches, until zero matches are found
                    }
                }
            }
        }

    }

    // to verify: i think we can use the same code for rotate left and rotate right.
    private bool HorizontalRotationBlocked(int pillARow, int pillACol, int pillBRow, int pillBCol)
    {
        // prevent rotation if cell above the left-most half of the pill is occupied.
        if(pillACol < pillBCol)
        {
            // side A is on the left.
            return arrangement.IsOccupied(pillARow - 1, pillACol);
        }
        else
        {
            // side B is on the left.
            return arrangement.IsOccupied(pillBRow - 1, pillBCol);
        }
        // TODO: change to an int, and return a bitmask depending on which sides are blocked.
        // 0: not blocked; 1: blocked on both sides; 2: blocked only on the right; 3: blocked only on the left
    }

    private bool VerticalRotationBlocked(int pillARow, int pillACol, int pillBRow, int pillBCol)
    {
        // prevent rotation if cell to right of bottom-most half of pill is occupied.
        if (pillARow > pillBRow)
        {
            // side A is on the bottom.
            return arrangement.IsOccupied(pillARow, pillACol + 1);
        }
        else
        {
            // side B is on the bottom.
            return arrangement.IsOccupied(pillBRow, pillBCol + 1);
        }
    }

    private bool SettleSurvivorsOneRow()
    {
        bool movement = false;
        // for now, don't worry about whether any pieces are connected
        for(int row = 15; row >= 0; row--)
        {
            for (int col = 0; col < 8; col++)
            {
                if(arrangement.TryDropSurvivor(row, col))
                {
                    movement = true;
                }
            }
        }
        if(movement)
        {
            soundPlayer.PlaySettle();
        }
        return movement;
    }

    private string ColorOf(GameObject pill)
    {
        string tag = pill.tag.ToLower();
        if (tag.StartsWith("red"))
        {
            return "red";
        }
        else if (tag.StartsWith("green"))
        {
            return "green";
        }
        else
        {
            return "blue";
        }
    }

    private int CheckEntirePlayfieldForMatches()
    {
        int matchCount = 0;
        for (int r = 15; r >= 0; r--)
        {
            for (int c = 0; c < 8; c++)
            {
                string color = arrangement.ColorAt(r, c);
                if (color != "empty")
                {
                    if (MarkMatches(r, c, color))
                    {
                        matchCount++;
                    }
                }
            }
        }
        arrangement.ReplaceMatches();
        return matchCount;
    }

    private int CheckForMatches(int atRowA, int atColA, string colorA, int atRowB, int atColB, string colorB)
    {

        // by the time this function runs, both sides of the pill have been deposited into the arrangement.
        // starting with (atRowA, atColA), mark horiz then vert matches.
        // repeat for (atRowB, atColB).
        // any marked cells get the same treatment: shrink them visually or whatever, and play a particle system.

        bool pillAMatched = MarkMatches(atRowA, atColA, colorA);
        bool pillBMatched = MarkMatches(atRowB, atColB, colorB);

        // replace the marked items with "exploding" items of the appropriate color.
        arrangement.ReplaceMatches();

        // change state if there was a match.

        if (pillAMatched || pillBMatched)
        {
            if (arrangement.NoViruses())
            {
                return StateWin;
            }
            else
            {
                return StateMatching;
            }
        }
        else
        {
            return StateReadyingPill;
        }

    }


    private bool MarkMatches(int atRow, int atCol, string color)
    {
        bool matchFound = false;
        string targetColor = arrangement.ColorAt(atRow, atCol);
        if (targetColor == "empty")
            return false; // shouldn't happen, but whatever

        // Horizontal checks:
        string nextColor = targetColor; // start with the color of the center piece, of course it matches itself
        int leftMatches = 0;
        int candidateCol = atCol - 1;
        while(nextColor == targetColor)
        {
            nextColor = arrangement.ColorAt(atRow, candidateCol);
            if(nextColor == targetColor)
            {
                leftMatches++;
                candidateCol--;
            }
        }
        nextColor = targetColor;
        int rightMatches = 0;
        candidateCol = atCol + 1;
        while(nextColor == targetColor)
        {
            nextColor = arrangement.ColorAt(atRow, candidateCol);
            if (nextColor == targetColor)
            {
                rightMatches++;
                candidateCol++;
            }
        }

        // Vertical checks:
        int upMatches = 0;
        int candidateRow = atRow - 1;
        nextColor = targetColor;
        while (nextColor == targetColor)
        {
            nextColor = arrangement.ColorAt(candidateRow, atCol);
            if (nextColor == targetColor)
            {
                upMatches++;
                candidateRow--;
            }
        }

        nextColor = targetColor;
        int downMatches = 0;
        candidateRow = atRow + 1;
        while(nextColor == targetColor)
        {
            nextColor = arrangement.ColorAt(candidateRow, atCol);
            if (nextColor == targetColor)
            {
                downMatches++;
                candidateRow++;
            }
        }


        // mark all items in the found ranges, H and V
        if (leftMatches + rightMatches > 2)
        {
            //Debug.Log($"Horizontal matches: {leftMatches + rightMatches + 1}");
            matchFound = true;
            for (int c = atCol - leftMatches; c <= atCol + rightMatches; c++)
            {
                arrangement.MarkMatch(atRow, c, targetColor);
            }
        }

        if (upMatches + downMatches > 2)
        {
            //Debug.Log($"Vertical matches: {upMatches + downMatches + 1}");
            matchFound = true;
            for (int r = atRow - upMatches; r <= atRow + downMatches; r++)
            {
                arrangement.MarkMatch(r, atCol, targetColor);
            }
        }

        if(matchFound)
        {
            soundPlayer.PlayMatch();
        }

        return matchFound;
    }

    private void OnGUI()
    {
        if (state == StateGameOver)
        {
            if (GUI.Button(new Rect(Screen.width / 2 - 200,
                Screen.height / 2 - 50, 400, 50), "Game Over, Try again"))
            {
                // go back to level 0
                state = StateReadyingPill;
                arrangement.InitializeGame();
                NextPill();
            }
        }
        else if (state == StateWin)
        {
            if (GUI.Button(new Rect(Screen.width / 2 - 200,
                Screen.height / 2 - 50, 400, 50), "You WIN!  Next level?"))
            {
                // go to next level
                state = StateReadyingPill;
                arrangement.InitializeGame();
                NextPill();
            }
        }
    }

    private void HandleControls()
    {
        // Left and right must be tapped.
        leftPressed = Input.GetKeyDown("left");
        rightPressed = Input.GetKeyDown("right");

        // Down may be held.
        if (!downDebounced)
        {
            downPressed = Input.GetKey("down");
        }
        else
        {
            downDebounced = !Input.GetKeyUp("down");
        }

        // rotation buttons must be tapped.
        // not sure if we need a debounce.
        rotateLeftPressed = Input.GetKeyDown("a");
        rotateRightPressed = Input.GetKeyDown("d");

    }
}
