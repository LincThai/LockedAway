using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    // Text variables for ui elements
    public Text nameText;
    public Text dialogueText;

    // set queue
    private Queue<string> sentences;

    // Start is called before the first frame update
    void Start()
    {
        // initialise
        sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        //Debug.Log("starting conversation with" + dialogue.name);
        nameText.text = dialogue.name;

        // cears dalogue from previous conversation
        sentences.Clear();

        // for every sentence in the e=array in dialogue class queue up each sentence
        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        // call function
        DisplayNextSentence();
    }

    // displays the next sentence in the queue
    public void DisplayNextSentence()
    {
        //checks if we are out of dialogue
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        // gets next sentence in queue
        string sentence = sentences.Dequeue();
        //Debug.Log(sentence);
        dialogueText.text = sentence;
    }

    // ends the dialogue
    public void EndDialogue()
    {
        Debug.Log("End of conversation");
    }

}
