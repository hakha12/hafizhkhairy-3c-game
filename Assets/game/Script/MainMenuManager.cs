using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void Play(){
		SceneManager.LoadScene("ForestScene");
	}

	public void Exit(){
		Application.Quit();
	}
}
