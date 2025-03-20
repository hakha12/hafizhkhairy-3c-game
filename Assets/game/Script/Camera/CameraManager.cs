using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class CameraManager : MonoBehaviour
{
	const int CAMERA_ROTATION_LIMIT = 45;
	const int CAMERA_ROTATION_UNLIMIT = 180;
    [SerializeField] public CameraState cameraState;
	[SerializeField] private InputManager _input;
	[SerializeField] private CinemachineVirtualCamera _fpsCamera;
	[SerializeField] private CinemachineFreeLook _tpsCamera;
	public Action OnChangePerspective;

	private void Start(){
		_input.OnChangePOV += SwitchCamera;
	}

	private void OnDestroy(){
		_input.OnChangePOV -= SwitchCamera;
	}
	private void SwitchCamera(){
		OnChangePerspective();

		if (cameraState == CameraState.ThirdPerson){
			cameraState = CameraState.FirstPerson;

			_tpsCamera.gameObject.SetActive(false);
			_fpsCamera.gameObject.SetActive(true);
		} else {
			cameraState = CameraState.ThirdPerson;

			_tpsCamera.gameObject.SetActive(true);
			_fpsCamera.gameObject.SetActive(false);
		}
	}

	public void SetFPSClampedCamera(bool isClamped, Vector3 playerRotation){
		CinemachinePOV pov = _fpsCamera.GetCinemachineComponent<CinemachinePOV>();

		if (isClamped){
			pov.m_HorizontalAxis.m_Wrap = false;

			pov.m_HorizontalAxis.m_MinValue = playerRotation.y - CAMERA_ROTATION_LIMIT;
			pov.m_HorizontalAxis.m_MaxValue = playerRotation.y + CAMERA_ROTATION_LIMIT;
		} else {
			pov.m_HorizontalAxis.m_Wrap = true;

			pov.m_HorizontalAxis.m_MinValue = -CAMERA_ROTATION_UNLIMIT;
			pov.m_HorizontalAxis.m_MaxValue = CAMERA_ROTATION_UNLIMIT;
		}
	}

	public void SetTPSFieldOfView(float fieldOfView){
		_tpsCamera.m_Lens.FieldOfView = fieldOfView;
	}
}
