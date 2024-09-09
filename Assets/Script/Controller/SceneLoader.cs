using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
	[SerializeField] private AssetReference _managersScene;


	[Header("Broadcasting on")]
	[SerializeField] private AssetReference _menuLoadChannel = default;

	private void Start()
	{
		//_managersScene.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += LoadEventChannel;
		//Additive는 로드된 씬에 추가로 넣는 느낌
		//즉 기존 init에 sceneReference를 추가
		//true는 비동기 완료 되면 바로 scene 실행
	}

}
