using System;
using System.Text;
using UnityEngine;

public class Arrangement : MonoBehaviour
{
    private GameObject[,] gameObjects = new GameObject[16, 8];

    public Transform container;

    public GameObject redVirus;
    public GameObject greenVirus;
    public GameObject blueVirus;

    public ParticleSystem redFX;
    public ParticleSystem greenFX;
    public ParticleSystem blueFX;

    public int virusCount;
    private bool initialized = false;


    private void Reset()
    {
        initialized = false;
        virusCount = 0;
        for(int r = 0; r < 16; r++)
        {
            for(int c = 0; c < 8; c++)
            {
                if(gameObjects[r, c] != null)
                {
                    Destroy(gameObjects[r, c]);
                }
                gameObjects[r, c] = null;
            }
        }
    }



    public void AddPiece(int atRow, int atCol, GameObject piece)
    {
        if (atRow < 0 || atRow > 15)
            return;
        if (atCol < 0 || atCol > 7)
            return;
        //piece.gameObject.transform.position = RowColToUnits(atRow, atCol);
        //Debug.Log($"Adding instance of prefab {piece.gameObject.name} at {piece.gameObject.transform.position}");
        gameObjects[atRow, atCol] = piece;
    }

    public void Remove(int atRow, int atCol)
    {
        if (atRow < 0 || atRow > 15)
            return ;
        if (atCol < 0 || atCol > 7)
            return ;
        if (gameObjects[atRow, atCol] == null)
            return ;
        if(gameObjects[atRow,atCol] != null)
        {
            //gameObjects[atRow, atCol].SetActive(false);
            if (gameObjects[atRow,atCol].tag.ToLower().Contains("virus"))
            {
                virusCount--;
                //Debug.Log($"virus count: {virusCount}");
            }
            Destroy(gameObjects[atRow, atCol]);
        }
    }

    public bool IsOccupied(int atRow, int atCol)
    {
        if (atRow == -1)
            return false;   // allow pill to be vertical while in top row
        if (atRow < -1 || atRow > 15) // to allow pill to be vertical while in top row
            return true;
        if (atCol < 0 || atCol > 7)
            return true;
        if (gameObjects[atRow, atCol] == null)
            return false;
        if (atRow == -1)
            return false;   // allow pill to be vertical while in top row
        if (gameObjects[atRow, atCol].tag.ToLower().StartsWith("blue") ||
            gameObjects[atRow, atCol].tag.ToLower().StartsWith("red") ||
            gameObjects[atRow, atCol].tag.ToLower().StartsWith("green"))
        {
            return true;
        }
        return false;
    }

    public void InitializeGame()
    {
        // future: randomize depending on the game level

        Reset();

        // for now: hard-coded

        AddVirus(15, 0, "blue");
        AddVirus(15, 1, "red");
        AddVirus(8, 6, "green");
        AddVirus(15, 2, "blue");
        AddVirus(14, 0, "red");
        AddVirus(14, 1, "green");
        initialized = true;
    }

    private void AddVirus(int atRow, int atCol, string color)
    {
        var virus = Instantiate(GetVirusType(color), RowColToUnits(atRow, atCol), Quaternion.identity, container);
        AddPiece(atRow, atCol, virus);
        virusCount++;
    }

    private GameObject GetVirusType(string color)
    {
        if(color == "red")
        {
            return redVirus;
        }
        else if (color == "green")
        {
            return greenVirus;
        }
        else
        {
            return blueVirus;
        }
    }

    public Vector3 RowColToUnits(int atRow, int atCol)
    {
        return new Vector3(atCol, 15 - atRow, 0); // passing -1 for atRow should yield a y of 16.0
    }

    internal bool ColorMatches(int atRow, int atCol, string tag)

    {
        if (atRow < 0 || atRow > 15)
            return false;
        if (atCol < 0 || atCol > 7)
            return false;
        GameObject piece = gameObjects[atRow, atCol];
        if (piece == null)
            return false; // shouldn't happen.  There should always be a blankObject at each location.
        if (tag.ToLower().StartsWith("red") && piece.tag.ToLower().StartsWith("red"))
        {
            return true;
        }
        if (tag.ToLower().StartsWith("green") && piece.tag.ToLower().StartsWith("green"))
        {
            return true;
        }
        if (tag.ToLower().StartsWith("blue") && piece.tag.ToLower().StartsWith("blue"))
        {
            return true;
        }
        return false; // means "empty" at this location
    }

    internal bool IsVirus(int atRow, int atCol)
    {
        if (atRow < 0 || atRow > 15)
            return false;
        if (atCol < 0 || atCol > 7)
            return false;
        if(gameObjects[atRow, atCol] == null)
            return false;
        return gameObjects[atRow, atCol].tag.ToLower().Contains("virus");
    }

    internal string ColorAt(int atRow, int atCol)
    {
        if (atRow < 0 || atRow > 15)
            return "empty";
        if (atCol < 0 || atCol > 7)
            return "empty";
        GameObject piece = gameObjects[atRow, atCol];
        if (piece == null)
            return "empty";
        if (piece.tag.ToLower().StartsWith("red"))
        {
            return "red";
        }
        if (piece.tag.ToLower().StartsWith("green"))
        {
            return "green";
        }
        if (piece.tag.ToLower().StartsWith("blue"))
        {
            return "blue";
        }
        return "empty";
    }

    public void DebugMessage()
    {
        Debug.Log("Contents of playfield (shown top to bottom):");
        StringBuilder sb = new StringBuilder();
        for(int r = 0; r < 16; r++)
        {
            string row = "";
            for (int c = 0; c < 8; c++)
            {
                if(gameObjects[r,c] == null || gameObjects[r,c].tag == "Empty")
                {
                    row += "x";
                }
                else
                {
                    row += gameObjects[r, c].tag[0];
                }
            }
            row += "\n";
            sb.Append(row);
        }
        Debug.Log(sb.ToString());
    }

    internal bool NoViruses()
    {
        if (!initialized)
        {
            return false;
        }    
        return (virusCount == 0);
    }

    internal bool TryDropSurvivor(int atRow, int atCol)
    {
        if (atRow < 0 || atRow > 15)
            return false;
        if (atCol < 0 || atCol > 7)
            return false;
        if (gameObjects[atRow, atCol] == null)
            return false;
        if (IsVirus(atRow, atCol))
        {
            return false;
        }
        // TODO also return false if this piece is connected to another piece.
        if (!IsOccupied(atRow+1, atCol))
        {
            //Debug.Log($"Dropping a surviving {gameObjects[atRow, atCol].tag}");
            var temp = gameObjects[atRow, atCol];
            gameObjects[atRow + 1, atCol] = temp;
            gameObjects[atRow + 1, atCol].gameObject.transform.position = RowColToUnits(atRow + 1, atCol);
            gameObjects[atRow, atCol] = null;
            return true;
        }
        return false;
    }

    internal void MarkMatch(int atRow, int atCol, string targetColor)
    {
        if (atRow < 0 || atRow > 15)
            return;
        if (atCol < 0 || atCol > 7)
            return;
        if (gameObjects[atRow, atCol] == null)
            return;
        if (!gameObjects[atRow, atCol].tag.EndsWith("X"))
        {
            gameObjects[atRow, atCol].tag = gameObjects[atRow, atCol].tag + "X";
        }
    }

    // where would score be calculated???

    internal void ReplaceMatches()
    {
        for(int r = 15; r >= 0; r--)
        {
            for (int c = 0; c < 8; c++)
            {
                if (gameObjects[r, c] == null)
                {
                    continue;
                }
                if (gameObjects[r, c].tag.EndsWith("X"))
                {
                    StartParticleSystem(gameObjects[r, c].tag, r, c);
                    var temp = gameObjects[r, c];
                    if (temp.tag.ToLower().Contains("virus"))
                    {
                        virusCount--;
                    }
                    // TODO also check if this piece is connected to another.  If so, clear the "connected to" on the other piece.
                    Destroy(temp, 0.25f);
                    gameObjects[r, c] = null;
                }
            }
        }
    }

    private void StartParticleSystem(string tag, int r, int c)
    {
        if(tag.ToLower().StartsWith("red"))
        {
            if(redFX != null)
            {
                var fx = Instantiate(redFX, RowColToUnits(r, c), Quaternion.identity);
                fx.Play();
                Destroy(fx, .5f);
            }
        }
        else if (tag.ToLower().StartsWith("green"))
        {
            if(greenFX != null)
            {
                var fx = Instantiate(greenFX, RowColToUnits(r, c), Quaternion.identity);
                fx.Play();
                Destroy(fx, .5f);
            }
        }
        else
        {
            if(blueFX != null)
            {
                var fx = Instantiate(blueFX, RowColToUnits(r, c), Quaternion.identity);
                fx.Play();
                Destroy(fx, .5f);
            }
        }
    }
}
