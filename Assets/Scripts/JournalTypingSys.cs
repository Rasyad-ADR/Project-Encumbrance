using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    string targetText = "The Sisyphus";
    int currentIndex = 0;

    void showJournal()
    {
        Debug.Log(targetText[currentIndex]);
    }


    void characterCheck(char character)
    {
        if (character == targetText[currentIndex])
        {
            Debug.Log("Benar");
            currentIndex += 1;
        }
        else (character != targetText[currentIndex])
        {
            Debug.Log("Salah");
        }
    }

    void Start()
    {
        showJournal();
    }
}
