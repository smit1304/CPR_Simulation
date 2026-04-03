using UnityEngine;
using UnityEngine.Events;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject[] tutorials;

    public UnityEvent onTutorialCompleted;
    private int currentTutorialIndex = 0;


    private void Awake()
    {
        tutorials[0].SetActive(true);

        for (int idx = 1; idx < tutorials.Length; idx++)
        {
            tutorials[idx].SetActive(false);
        }
    }

    public void NextTutorial()
    { 
        tutorials[currentTutorialIndex].SetActive(false);
        currentTutorialIndex++;
        if(currentTutorialIndex >= tutorials.Length)
        {
            currentTutorialIndex = 0;
            onTutorialCompleted.Invoke();
        }

        tutorials[currentTutorialIndex].SetActive(true);
    }

    public void PreviousTutorial()
    {
        tutorials[currentTutorialIndex].SetActive(false);
        currentTutorialIndex--;
        if (currentTutorialIndex < 0)
        {
            currentTutorialIndex = 0;
        }
        tutorials[currentTutorialIndex].SetActive(true);
    }
}
