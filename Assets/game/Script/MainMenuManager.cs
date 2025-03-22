using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void Play(){
		SceneManager.LoadScene("GameplayScene");
	}

	public void Exit(){
		Application.Quit();
	}
}
