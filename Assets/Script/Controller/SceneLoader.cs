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

	private void Start()
	{
		_managersScene.LoadSceneAsync(LoadSceneMode.Single, true);
		//비동기 씬 로드, true : 로딩되면 바로 실행
		//이후 기존에 있는 메모리는 제거되는 듯?
	}

}
