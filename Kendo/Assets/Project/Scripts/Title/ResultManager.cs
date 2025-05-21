using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    
    void Start()
    {
        int totalScore = PlayerPrefs.GetInt("TotalScore", 0);
        scoreText.text = $"Score: {totalScore}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
