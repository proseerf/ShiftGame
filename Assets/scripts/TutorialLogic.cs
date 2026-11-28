using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialLogic : MonoBehaviour
{
    public PauseMenu pauseMenu;

    public TextMeshProUGUI textBox;
    public SubmissionLogicScript submissionLogic;






    private int tutorialStep;
    private bool rightClick = false;
    private bool scroll = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && tutorialStep == 2)
        {
            pauseMenu.Resume();
            textBox.text = "Now press \"R\" to reform back into a blob";
            tutorialStep = 3;
        }
        if (Input.GetKeyDown(KeyCode.R) && tutorialStep == 3)
        {
            pauseMenu.Resume();
            textBox.text = "Try heading over to the rug and pressing \"TAB\" to go to the audience view";
        }
        if(Input.GetMouseButton(1) && (tutorialStep == 4))
        {
            rightClick = true;
            if(scroll)
            {
                howToWin();
            }
        }
        if((Input.mouseScrollDelta.y != 0) && (tutorialStep == 4))
        {
            scroll = true;
            if(rightClick)
            {
                howToWin();
            }
        }
        if((submissionLogic.matchPercentage > 40) && (tutorialStep == 6))
        {
            howToDuplicate();
        }
        if (Input.GetKeyDown(KeyCode.X) && (tutorialStep == 7))
        {
            howToWinContinued();
        }
        if(submissionLogic.finished)
        {
            clearInstructions();
        }
    }

    public void howToCopy()
    {     
        //Pause phsyics
        pauseMenu.Pause();
        //Show copy explantion
        textBox.text = "Press SHIFT to shapeshift into certain objects";

        // Wait for "Shift"
        tutorialStep = 2;
    }

    public void howToNavigateTheatre()
    {
        if(tutorialStep == 3)
        {
            textBox.text = "Use \"WASD\" to move Up/Down/Left/Right\nHold right click and move the mouse to rotate\nAnd use the scroll wheel to change size";
            tutorialStep = 4;
        }
    }

    private void howToWin()
    {
        textBox.text = "Your Goal is to cover all of the shapes in the screen Try orienting and sizing yourself to cover one of the holes here";
        tutorialStep = 6;
    }

    private void howToWinContinued()
    {
        textBox.text = "Go back and complete the shape to finish the tutorial";
    }

    private void howToDuplicate()
    {
        textBox.text = "Press \"X\" to leave a body double in the shadows.\nYou can't undo this without resetting the level so be certain";
        tutorialStep = 7;
    }

    public void clearInstructions()
    {
        textBox.text = "";
    }
}
