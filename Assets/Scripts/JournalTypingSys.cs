using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    string targetText;
    int currentIndex = 0;

    void showJournal()
    {
        Debug.Log("Jurnal Dimulai");
    }

    void Start()
    {
        showJournal();
    }
}
