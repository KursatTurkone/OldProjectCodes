using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionManager : MonoBehaviour
{
    [SerializeField] private InfiniteScrollWithSnapping[] InfiniteScrollWithSnappings;
    [SerializeField] private Button StartGameButton;
    public static CharacterSelectionManager Instance { get; private set; }
    public List<int> selectedCharacterIndexs;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SelectedCharacterChanged()
    {
        selectedCharacterIndexs.Clear();
        for (int i = 0; i < InfiniteScrollWithSnappings.Length; i++)
        {
            InfiniteScrollWithSnappings[i].ChangeColorToNormal();
        }
        for (int i = 0; i < InfiniteScrollWithSnappings.Length; i++)
        {
            
            selectedCharacterIndexs.Add(InfiniteScrollWithSnappings[i].CurrentIndex);
            for (int j = 0; j < InfiniteScrollWithSnappings.Length; j++)
            {
                if (InfiniteScrollWithSnappings[i] == InfiniteScrollWithSnappings[j])
                {
                    continue;
                }

                InfiniteScrollWithSnappings[j].ChangeColorToGrey(InfiniteScrollWithSnappings[i].CurrentIndex);
            }
        }

        HashSet<int> control = new HashSet<int>();
        for (int k = 0;
             k < selectedCharacterIndexs.Count;
             k++)
        {
            if (!control.Add(selectedCharacterIndexs[k]))
            {
                StartGameButton.interactable = false;
                return;
            }

            StartGameButton.interactable = true;
        }
    }
}