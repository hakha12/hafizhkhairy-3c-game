using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
	[SerializeField] private AudioSource _footstepSFX;
	[SerializeField] private AudioSource _glideSFX;

	private void PlayFootStepSFX(){
		_footstepSFX.volume = Random.Range(0.7f, 1f);
		_footstepSFX.pitch = Random.Range(0.5f, 2.5f);
		_footstepSFX.Play();
	}

	public void PlayGlideSFX(){
		_glideSFX.Play();
	}

	public void StopGlideSFX(){
		_glideSFX.Stop();
	}
}
