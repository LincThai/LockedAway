using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    // animation variable
    public Animator animator;

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
        // play open animation
        animator.SetBool("isOpen", true);
        

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
        //dialogueText.text = sentence;

        // stops all coroutines
        StopAllCoroutines();

        // starts the typing
        StartCoroutine(TypeSentence(sentence));
    }

    // makes letters appear one at a time for dialogue
    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        // ToCharArray converts string into character array
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    // ends the dialogue
    public void EndDialogue()
    {
        //Debug.Log("End of conversation");
        // play close animation
        animator.SetBool("isOpen", false);
    }

}
