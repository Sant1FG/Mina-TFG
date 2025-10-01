using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles all operations relating to the game leaderboard:
/// - Populating the leaderboard.
/// - Adding new entries.
/// - Sorting the leaderboard.
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private Transform entryContainer;
    [SerializeField] private Transform entryTemplate;
    private List<Transform> leaderboardTransformList;

    private Leaderboard leaderboard;
    private readonly float templateHeight = 60f;

    public void PopulateLeaderboard()
    {
        entryTemplate.gameObject.SetActive(false);

        string fromJSON = PlayerPrefs.GetString("leaderboards");
        leaderboard = JsonUtility.FromJson<Leaderboard>(fromJSON);
        if (leaderboard == null)
        {
            leaderboard = new Leaderboard();
            leaderboard.leaderboardList = new List<LeaderboardEntry>();
            //LeaderboardEntry devEntry = new LeaderboardEntry() { score = 850, name = "DEV" };
            LeaderboardEntry leaderboardEntry = new LeaderboardEntry() { score = 0, name = "___" };
            //leaderboard.leaderboardList.Add(devEntry);
            for (int i = 0; i < 10; i++)
            {
                leaderboard.leaderboardList.Add(leaderboardEntry);
            }
            leaderboard.leaderboardList = SortList(leaderboard.leaderboardList);
            string json = JsonUtility.ToJson(leaderboard);
            PlayerPrefs.SetString("leaderboards", json);
            PlayerPrefs.Save();
        }

        leaderboardTransformList = new List<Transform>();

        foreach (var entry in leaderboard.leaderboardList)
        {
            CreateLeaderboardEntryTransform(entry, entryContainer, leaderboardTransformList);
        }
    }

    public int LowestHighScore()
    {
        if (leaderboard.leaderboardList.Count < 10)
        {
            return 0;
        }
        else
        {
            return leaderboard.leaderboardList[leaderboard.leaderboardList.Count - 1].score;
        }
    }

    private List<LeaderboardEntry> SortList(List<LeaderboardEntry> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                if (list[j].score > list[i].score)
                {
                    LeaderboardEntry temp = list[j];
                    list[j] = list[i];
                    list[i] = temp;
                }
            }
        }
        return list;
    }

    private void CreateLeaderboardEntryTransform(LeaderboardEntry leaderboardEntry, Transform container, List<Transform> transformList)
    {
        entryTemplate.gameObject.SetActive(false);

        Transform entryTransform = Instantiate(entryTemplate, container);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
        entryTransform.gameObject.SetActive(true);

        entryTransform.Find("TemplateBackground").gameObject.SetActive((transformList.Count + 1) % 2 == 1);

        entryTransform.Find("TemplatePos").GetComponent<TextMeshProUGUI>().SetText("{0}", transformList.Count + 1);
        entryTransform.Find("TemplateScore").GetComponent<TextMeshProUGUI>().SetText("{0}", leaderboardEntry.score);
        entryTransform.Find("TemplateName").GetComponent<TextMeshProUGUI>().SetText(leaderboardEntry.name);

        transformList.Add(entryTransform);
    }

    public void ClearLeaderboardEntryTransform()
    {
        foreach (Transform item in entryContainer)
        {
            Destroy(item.gameObject);
            leaderboardTransformList.Clear();
        }
    }

    public void AddLeaderboardEntry(int score, string name)
    {
        LeaderboardEntry leaderboardEntry = new LeaderboardEntry() { score = score, name = name };

        string jsonEntry = PlayerPrefs.GetString("leaderboards");
        Leaderboard leaderboard = JsonUtility.FromJson<Leaderboard>(jsonEntry);

        leaderboard.leaderboardList.Add(leaderboardEntry);
        leaderboard.leaderboardList = SortList(leaderboard.leaderboardList);
        if (leaderboard.leaderboardList.Count > 10)
        {
            leaderboard.leaderboardList.RemoveAt(10);
        }

        string json = JsonUtility.ToJson(leaderboard);
        PlayerPrefs.SetString("leaderboards", json);
        PlayerPrefs.Save();
    }

    private class Leaderboard
    {
        public List<LeaderboardEntry> leaderboardList;
    }

    [System.Serializable]
    private class LeaderboardEntry
    {
        public int score;
        public string name;
    }


}


