using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utility.ScenLoader
{
    public static class SceneLoader
    {
        public static void LoadTitle()
        {
            SceneManager.LoadScene("Title", LoadSceneMode.Single);
        }
        
        public static void LoadConfig()
        {
            SceneManager.LoadScene("Config", LoadSceneMode.Additive);
        }
        
        public static void UnLoadConfig()
        {
            SceneManager.UnloadSceneAsync("Config");
        }
        
        public static void LoadInGame()
        {
            SceneManager.LoadScene("main1-1", LoadSceneMode.Single);
        }
        
        public static void LoadResult()
        {
            SceneManager.LoadScene("Result", LoadSceneMode.Single);
        }
        
        public static void Exit()
        {
            #if UNITY_EDITOR                                        // Unityエディタ上で実行中の場合
                UnityEditor.EditorApplication.isPlaying = false;    // エディタの再生を停止
            #else                                                   // ビルドされた実行ファイルの場合
                Application.Quit();                                 // アプリケーションを終了
            #endif
        }
    }
}

